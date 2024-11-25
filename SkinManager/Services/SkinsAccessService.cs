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
    Locations locations)
{
    private GameInfo _psoGame = default!;
    private List<Skin> _gameSkins = [];
    private SkinsSource _skinsSource = SkinsSource.Ephinea;
    
    private void AddAppliedSkin(string appliedSkinName)
    {
        if (!_psoGame.AppliedSkins.Contains(appliedSkinName))
        {
            _psoGame.AppliedSkins.Add(appliedSkinName);
        }
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
    public string GetAppliedSkinNameFromLocation(string selectedSkinTypeName, string selectedSkinSubTypeName)
    {
        foreach (string skinLocation in _psoGame.AppliedSkins)
        {
            var possibleSkin = _gameSkins
                .SingleOrDefault(x => x.Locations.Contains(skinLocation)) is { } foundSkinLocation
                ? Something<Skin>.Create(foundSkinLocation)
                : Nothing<Skin>.Create();

            if (possibleSkin is Something<Skin> foundSkin && foundSkin.Value.Name == selectedSkinTypeName &&
                foundSkin.Value.SubType == selectedSkinSubTypeName)
            {
                return foundSkin.Value.Name;
            }
        }

        return "None";
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
            _psoGame.AppliedSkins.Remove(skinName);
            return true;
        }
        else
        {
            return false;
        }
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
    public async Task<bool> BackUpExists(string selectedSkinName)
    {
        if(_gameSkins.FirstOrDefault(x => x.Name == selectedSkinName) is { } selectedSkin)
        {
            string backupPath =  Path.Combine(_psoGame.SkinsLocation, selectedSkin.SkinType, "Originals", selectedSkin.SubType);
            return await FileAccessService.FolderHasFilesAsync(backupPath);
        }
        
        return false;
    }
    public IEnumerable<SkinType> GetSkinTypes()
    {
        return _psoGame.SkinTypes;
    }
    public string GetGameExecutableLocation()
    {
        return _psoGame.GameExecutable;
    }
    public string GetGameLocation()
    {
        return _psoGame.GameLocation;
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
        _psoGame = await FileAccessService.LoadGameInfoAsync(locations.GameInfoFile);

        UpdateSkins(await FileAccessService.LoadCachedWebSkinsAsync(locations.WebSkinsFile));
    }
    public async Task SaveInformation()
    {
        List<Task> tasks =
        [
            FileAccessService.SaveSkinsAsync(_gameSkins, locations.WebSkinsFile),
            FileAccessService.SaveGameInfoAsync(_psoGame, locations.GameInfoFile)
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

    public async Task<bool> CreateStructureAsync()
    {
        return await FileAccessService.CreateStructureAsync(_psoGame.SkinTypes, _psoGame.SkinsLocation);
    }
}