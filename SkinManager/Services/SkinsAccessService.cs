using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkinManager.Services;

public class SkinsAccessService(
    IServiceScopeFactory scopeFactory,
    HttpClient httpClient,
    Locations locations,
    IMessenger theMessenger)
{
    private GameInfo _psoGame = default!;
    private List<Skin> _gameSkins = [];
    private Dictionary<(string,string),Skin> _appliedSkins = [];
    private SkinsSource _skinsSource = SkinsSource.Ephinea;
    
    private void AddAppliedSkin(string appliedSkinName)
    {
        Skin appliedSkin = _gameSkins.First(x => x.Name == appliedSkinName);
        _appliedSkins[(appliedSkin.SkinType, appliedSkin.SubType)] = appliedSkin;
    }
    private void RemoveAppliedSkin(Skin appliedSkin)
    {
        _appliedSkins.Remove((appliedSkin.SkinType, appliedSkin.SubType));
    }

    public async Task<IEnumerable<Skin>> RefreshSkins(bool includeWeb)
    {
        await RefreshLocalSkins();
        
        if (includeWeb)
        {
            await RefreshWebSkinsAsync();
        }

        return _gameSkins;
    }
    private async Task RefreshLocalSkins()
    {
        using IServiceScope serviceScope = scopeFactory.CreateScope();

        LocalSkinsAccessService skinsAccessService =
            serviceScope.ServiceProvider.GetRequiredService<LocalSkinsAccessService>();

        IEnumerable<Skin> skins = await skinsAccessService.GetAvailableSkinsAsync(_psoGame
            .SkinsLocation, _psoGame.SkinTypes);

        UpdateSkins(skins);
    }
    private async Task RefreshWebSkinsAsync()
    {
        using IServiceScope serviceScope = scopeFactory.CreateScope();

        ISkinsAccessService skinsAccessService;

        if (_skinsSource == SkinsSource.Ephinea)
        {
            skinsAccessService =
                serviceScope.ServiceProvider.GetRequiredService<EphineaWebAccessService>();
        }
        else
        {
            skinsAccessService = serviceScope.ServiceProvider.GetRequiredService<UniversePSWebAccessService>();
        }

        DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
        List<SkinType> tempSkinTypes = [];

        foreach (SkinType currentSkinType in _psoGame.SkinTypes)
        {

            if ((currentDate.DayNumber - currentSkinType.LastOnlineChecks[_skinsSource].DayNumber) > 1)
            {
                UpdateSkins(await skinsAccessService.GetAvailableSkinsForSpecificTypeAsync(currentSkinType));
            }

            Dictionary<SkinsSource, DateOnly> newDates = new Dictionary<SkinsSource, DateOnly>();
            foreach (SkinsSource currenSkinsSource in currentSkinType.LastOnlineChecks.Keys)
            {
                newDates.Add(currenSkinsSource,
                    currenSkinsSource == _skinsSource
                        ? currentDate
                        : currentSkinType.LastOnlineChecks[currenSkinsSource]);
            }

            tempSkinTypes.Add(currentSkinType with { LastOnlineChecks = newDates });
        }

        _psoGame = _psoGame with { SkinTypes = tempSkinTypes };

    }

    public async Task<string> DownloadSkin(string skinName, int downloadLink)
    {
        try
        {
            Skin skinToDownload = _gameSkins.First(x => x.Name == skinName);
            HttpResponseMessage response =
                await httpClient.GetAsync(skinToDownload.Locations[downloadLink]);

            string fileExtension = response.RequestMessage?.RequestUri?.OriginalString.Split('.').Last() ??
                                   string.Empty;

            string filePath = Path.Combine(_psoGame.SkinsLocation, $"{skinToDownload.Name}.{fileExtension}");

            await using Stream webStream = await httpClient.GetStreamAsync(skinToDownload.Locations[downloadLink]);

            await using Stream skinFileStream = File.Create(filePath);
            await webStream.CopyToAsync(skinFileStream);
            return filePath;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return string.Empty;
        }
    }
    private void ChangeWebSkinToLocalSkin(string skinName, string newLocalSkinPath)
    {
        Skin currentSkin = _gameSkins.First(x => x.Name == skinName);
        _gameSkins.Remove(currentSkin);

        currentSkin = currentSkin with { Locations = [newLocalSkinPath], Source = SkinsSource.Local};
        
        _gameSkins.Add(currentSkin);
    }
    private void UpdateSkins(IEnumerable<Skin> skins)
    {
        if (_gameSkins.Count == 0)
        {
            _gameSkins.AddRange(skins);
        }
        else
        {
            List<Skin> newSkins = skins
                .Where(tempSkin => !_gameSkins
                    .Select(currentSkin => currentSkin.Name).Contains(tempSkin.Name)).ToList();

            _gameSkins.AddRange(newSkins);
        }
    }
    public async Task<bool> ApplySkin(string skinName, bool isEphinea)
    {
        string skinDirectory = _gameSkins.First(x => x.Name == skinName).Locations[0];
        string gameDirectory = _psoGame.GameLocation;
        if (await FileAccessService.ApplySkinAsync(skinDirectory, gameDirectory))
        {
            AddAppliedSkin(skinName);
            return true;
        }

        return false;
    }
    public async Task<bool> CreateBackup(string skinName)
    {
        Skin selectedSkin = _gameSkins.First(x => x.Name == skinName);
        string backupLocation = GetBackupLocation(skinName);
        return await FileAccessService.CreateBackUpAsync(selectedSkin.Locations[0], backupLocation,_psoGame.GameLocation);
    }

    public bool FolderStructureIsCreated()
    {
        return _psoGame.StructureCreated;
    }
    public async Task<bool> RestoreBackup(string skinName)
    {
        Skin selectedSkin = _gameSkins.First(x => x.Name == skinName);

        if (await FileAccessService.RestoreBackupAsync(selectedSkin.Locations[0], _psoGame.GameLocation))
        {
            RemoveAppliedSkin(selectedSkin);
            return true;
        }
        else
        {
            return false;
        }
    }
    public async Task<bool> BackUpExists(string selectedSkinName)
    {
        if(_gameSkins.FirstOrDefault(x => x.Name == selectedSkinName) is { } selectedSkin)
        {
            string backupPath =  Path.Combine(_psoGame.SkinsLocation, selectedSkin.SkinType, "Originals", selectedSkin.SubType);
            return await FileAccessService.FolderHasFilesAsync(backupPath);
        }
        
        return false;
    }

    public string GetAppliedSkinName(string selectedSkinTypeName, string selectedSkinSubTypeName)
    {
        return _appliedSkins.TryGetValue((selectedSkinTypeName, selectedSkinSubTypeName), out Skin? appliedSkin) ? appliedSkin.Name : "None";
    }
    public IEnumerable<SkinType> GetSkinTypes()
    {
        return _psoGame.SkinTypes;
    }
    public IEnumerable<string> GetSkinSubTypes(string selectedSkinTypeName)
    {
        return GetSkinTypes().First(x => x.Name == selectedSkinTypeName).SubTypes;
    }
    private string GetBackupLocation(string selectedSkinName)
    {
        if(_gameSkins.FirstOrDefault(x => x.Name == selectedSkinName) is { } selectedSkin)
        {
            return Path.Combine(_psoGame.SkinsLocation, selectedSkin.SkinType, "Originals", selectedSkin.SubType);
        }
        else
        {
            return string.Empty;
        }
    }
    public string GetGameExecutableLocation()
    {
        return _psoGame.GameExecutable;
    }
    public string GetGameLocation()
    {
        return _psoGame.GameLocation;
    }
    public string GetSkinsLocation()
    {
        return _psoGame.SkinsLocation;
    }

    public async Task<bool> ExtractSkin(string skinName, string archivePath)
    {
        string fileExtension = Path.GetExtension(archivePath);
        
        if (fileExtension == "zip")
        {
            Skin skinToDownload = _gameSkins.First(x => x.Name == skinName);
            string newSkinPath = Path.Combine(_psoGame.SkinsLocation, skinToDownload.SkinType,
                skinToDownload.SubType, skinToDownload.Name);
            
            if (await FileAccessService.ExtractSkinAsync(archivePath, newSkinPath))
            {
                ChangeWebSkinToLocalSkin(skinToDownload.Name, newSkinPath);
                return true;
            }
        }
        
        return false;
    }
    public async Task<bool> SaveScreenshots(string skinName)
    {
        Skin theSkin  = _gameSkins.First(x => x.Name == skinName);
        
        if (theSkin.Screenshots.Any())
        {
            string screenshotsPath = Path.Combine(_psoGame.SkinsLocation, theSkin.SkinType, theSkin.SubType, theSkin.Name, "Screenshots");
            return await FileAccessService.SaveScreenshotsAsync(screenshotsPath, theSkin.Screenshots);
        }

        return false;
    }
    public async Task LoadInformation()
    {

        if(await FileAccessService.LoadJsonToObject<GameInfo>(locations.GameInfoFile) is not { } gameInfo)
        {
            theMessenger.Send(new FatalErrorMessage("IO Exception", $"Unable to find the {locations.GameInfoFile} file."));
        }
        else
        {
            _psoGame = gameInfo;
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-2));
            for (int i = 0; i < _psoGame.SkinTypes.Count; i++)
            {
                if (_psoGame.SkinTypes[i].LastOnlineChecks is null)
                {
                    Dictionary<SkinsSource, DateOnly> newChecks = [];

                    foreach (SkinsSource currentSource in Enum.GetValues(typeof(SkinsSource)))
                    {
                        if(currentSource != SkinsSource.Local)
                        {
                            newChecks.Add(currentSource, currentDate);                            
                        }
                        _psoGame.SkinTypes[i] = _psoGame.SkinTypes[i] with { LastOnlineChecks = newChecks };
                    }                    
                }
            }
        }

        UpdateSkins(await FileAccessService.LoadJsonToIEnumerable<Skin>(locations.CachedSkinsFile));
        UpdateAppliedSkins(await FileAccessService.LoadJsonToIEnumerable<Skin>(locations.AppliedSkinsFile));

    }

    private void UpdateAppliedSkins(IEnumerable<Skin> appliedSkins)
    {
        foreach (Skin currentSkin in appliedSkins)
        {
            _appliedSkins.Add((currentSkin.SkinType, currentSkin.SubType), currentSkin);
        }
    }

    public async Task SaveInformation()
    {
        List<Task> tasks =
        [
            FileAccessService.SaveIEnumerableToJson(_gameSkins, locations.CachedSkinsFile),
            FileAccessService.SaveObjectToJson(_psoGame, locations.GameInfoFile),
            FileAccessService.SaveIEnumerableToJson(_appliedSkins, locations.AppliedSkinsFile)
        ];
        await Task.WhenAll(tasks);
    }
    public void SetSkinsSource(SkinsSource skinsSource)
    {
        _skinsSource = skinsSource;
    }
    public IEnumerable<Skin> GetSkins()
    {
        return _gameSkins;
    }

    public async Task CreateStructureAsync()
    {
        await FileAccessService.CreateStructureAsync(_psoGame.SkinTypes, _psoGame.SkinsLocation);
    }

    public void SetSkinsLocation(string skinsLocation)
    {
        _psoGame = _psoGame with { SkinsLocation = skinsLocation};
    }

    public void SetGameLocation(string gameLocation)
    {
        _psoGame = _psoGame with { GameLocation = gameLocation };
    }

    public void SetGameExecutableLocation(string gameExecutableLocation)
    {
        _psoGame = _psoGame with { GameExecutable = gameExecutableLocation };
    }
}