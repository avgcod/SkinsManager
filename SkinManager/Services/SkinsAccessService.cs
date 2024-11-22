using Microsoft.Extensions.DependencyInjection;
using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using SkinManager.Extensions;

namespace SkinManager.Services;

public class SkinsAccessService(
    IServiceScopeFactory scopeFactory,
    FileAccessService fileAccessService,
    HttpClient httpClient,
    Locations locations)
{
    private GameInfo _psoGame = default!;
    private List<Skin> _gameSkins = [];
    private SkinsSource _skinsSource = SkinsSource.Ephinea;

    public void AddAppliedSkin(string appliedSkinName)
    {
        if (!_psoGame.AppliedSkins.Contains(appliedSkinName))
        {
            _psoGame.AppliedSkins.Add(appliedSkinName);
        }
    }

    public async Task<(bool downloaded, bool extracted)> DownloadSkin(Skin skinToDownload, int downloadLink, IEnumerable<string> screenshots)
    {
        (bool downloaded, bool extracted) fullStatus = (false, false);

        try
        {
            HttpResponseMessage response =
                await httpClient.GetAsync(skinToDownload.Locations[downloadLink]);

            string fileExtension = response.RequestMessage?.RequestUri?.OriginalString.Split('.').Last() ??
                                   string.Empty;

            string filePath = Path.Combine(_psoGame.SkinsLocation, $"{skinToDownload.Name}.{fileExtension}");

            await using Stream webStream = await httpClient.GetStreamAsync(skinToDownload.Locations[downloadLink]);

            await using Stream skinFileStream = File.Create(filePath);
            await webStream.CopyToAsync(skinFileStream);
            skinFileStream.Close();

            fullStatus = (true, false);

            if (fileExtension == "zip")
            {
                fullStatus = (true, await ExtractSkin(skinToDownload.Name, filePath));
                
                if (skinToDownload.Screenshots.Any())
                {
                    await AddScreenShots(skinToDownload, screenshots);
                }

                if (fullStatus is { downloaded: true, extracted: true })
                {
                    string newSkinPath = Path.Combine(_psoGame.SkinsLocation, skinToDownload.SkinType,
                        skinToDownload.SubType, skinToDownload.Name);
                    ChangeWebSkinToLocalSkin(skinToDownload.Name, newSkinPath);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return fullStatus;
    }

    private void ChangeWebSkinToLocalSkin(string skinName, string newLocalSkinPath)
    {
        Skin currentSkin = _gameSkins.First(x => x.Name == skinName);
        _gameSkins.Remove(currentSkin);

        currentSkin = currentSkin with { Locations = [newLocalSkinPath] };
        
        _gameSkins.Add(currentSkin);
    }

    private async Task AddScreenShots(Skin skin, IEnumerable<string> screenshots)
    {
        foreach (var screenshot in screenshots)
        {
            string path = Path.Combine(_psoGame.SkinsLocation, skin.SkinType, skin.SubType, skin.Name,"Screenshots");
            await fileAccessService.SaveScreenshot(path, screenshots);
        }
    }

    public async Task<IEnumerable<Skin>> RefreshWebSkinsAsync()
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
                if (currenSkinsSource == _skinsSource)
                {
                    newDates.Add(currenSkinsSource, currentDate);
                }
                else
                {
                    newDates.Add(currenSkinsSource, currentSkinType.LastOnlineChecks[currenSkinsSource]);
                }
            }

            tempSkinTypes.Add(currentSkinType with { LastOnlineChecks = newDates });
        }

        _psoGame = _psoGame with { SkinTypes = tempSkinTypes };

        return _gameSkins;
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

    public async Task<IEnumerable<Skin>> RefreshLocalSkinsAsync()
    {
        using IServiceScope serviceScope = scopeFactory.CreateScope();

        LocalSkinsAccessService skinsAccessService =
            serviceScope.ServiceProvider.GetRequiredService<LocalSkinsAccessService>();

        IEnumerable<Skin> skins = await skinsAccessService.GetAvailableSkinsAsync(_psoGame
            .SkinsLocation, _psoGame.SkinTypes);

        UpdateSkins(skins);

        return _gameSkins;
    }

    public IEnumerable<string> GetOriginalSkinNames()
    {
        foreach (string currentSkinName in _gameSkins
                     .Where(currentSkin => currentSkin.IsOriginal())
                     .Select(x => x.Name))
        {
            yield return currentSkinName;
        }
    }

    public string GetSkinsLocation()
    {
        return _psoGame.SkinsLocation;
    }

    private async Task<bool> ExtractSkin(string skinName, string archivePath)
    {
        Skin currentSkin = _gameSkins.First(x => x.Name == skinName);
        string destination = Path.Combine(_psoGame.SkinsLocation, currentSkin.SkinType, currentSkin.SubType, currentSkin.Name);
        return await fileAccessService.ExtractSkin(archivePath, destination);
    }

    public async Task LoadInformation()
    {
        _psoGame = await fileAccessService.LoadGameInfo(locations.GameInfoFile);

        UpdateSkins(await fileAccessService.LoadWebSkins(locations.WebSkinsFile));
    }

    public void RemoveAppliedSkin(string removedSkinName)
    {
        _psoGame.AppliedSkins.Remove(removedSkinName);
    }


    public async Task SaveInformation()
    {
        List<Task> tasks =
        [
            fileAccessService.SaveWebSkins(_gameSkins, locations.WebSkinsFile),
            fileAccessService.SaveGameInfo(_psoGame, locations.GameInfoFile)
        ];
        await Task.WhenAll(tasks);
    }

    public void SetSkinsSource(SkinsSource skinsSource)
    {
        _skinsSource = skinsSource;
    }

    public IEnumerable<Skin> GetCurrentSkins()
    {
        return _gameSkins;
    }
}