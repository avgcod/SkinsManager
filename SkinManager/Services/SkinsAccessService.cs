/*using SkinManager.Types;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LanguageExt;
using SkinManager.Extensions;

namespace SkinManager.Services;

public class SkinsAccessService{
    private GameInfo _psoGame = new(string.Empty, string.Empty, string.Empty);
    private ImmutableList<Either<WebSkin, LocalSkin>> _gameSkins =[];
    private ImmutableDictionary<(string, string), LocalSkin> _appliedSkins = ImmutableDictionary<(string, string), LocalSkin>.Empty;
    private SkinsSource _skinsSource = SkinsSource.Ephinea;
    private ImmutableDictionary<SkinsSource, AddressBook> _addressBooks = ImmutableDictionary<SkinsSource, AddressBook>.Empty;

    private ImmutableDictionary<(string, string), LocalSkin> AddAppliedSkin(string appliedSkinName){
        LocalSkin appliedSkin = _gameSkins.Rights().First(x => x.SkinName == appliedSkinName);
            return _appliedSkins
                .Remove((appliedSkin.SkinType, appliedSkin.SkinSubType))
                .Add((appliedSkin.SkinType, appliedSkin.SkinSubType), appliedSkin);
        
    }
    private ImmutableDictionary<(string, string), LocalSkin> RemoveAppliedSkin(LocalSkin appliedSkin){
        return _appliedSkins.Remove((appliedSkin.SkinType, appliedSkin.SkinSubType));
    }
    public async Task RefreshSkins(bool includeWeb){
        await RefreshLocalSkins();

        if (includeWeb){
            await RefreshWebSkinsAsync();
        }
    }
    private async Task RefreshLocalSkins(){

        (await LocalSkinsAccessService.GetAvailableSkinsAsync(_psoGame
            .SkinsLocation)).IfSucc(UpdateSkins);
    }
    private async Task RefreshWebSkinsAsync(){

        if (_skinsSource == SkinsSource.Ephinea){
            var ephineaSkins = await GetEphineaSkins();
            _gameSkins = _gameSkins.RemoveRange(ephineaSkins.Select(currentSkin => new Either.Left<WebSkin, LocalSkin>(currentSkin)));
            
            _gameSkins = _gameSkins.AddRange(ephineaSkins.Select(currentSkin => new Either.Left<WebSkin, LocalSkin>(currentSkin)));
        }
        else{
            var universePsSkins = await GetUniversePsSkins();
            _gameSkins = _gameSkins.RemoveRange(universePsSkins.Select(currentSkin => new Either.Left<WebSkin, LocalSkin>(currentSkin)));
            
            _gameSkins = _gameSkins.AddRange(universePsSkins.Select(currentSkin => new Either.Left<WebSkin, LocalSkin>(currentSkin)));
        }
    }
    private async Task<ImmutableList<WebSkin>> GetEphineaSkins(){
        var skinTypes = await EphineaService.GetAvailableSkinTypes(new HttpClient(), _addressBooks[_skinsSource].Main, _addressBooks[_skinsSource].Base);

        ImmutableList<SkinSiteInformation> newWebSkinSiteInfos = [];
        foreach (var skinTypeSiteInformation in skinTypes){
            newWebSkinSiteInfos = newWebSkinSiteInfos.AddRange(await EphineaService.GetAvailableSkins(new HttpClient(), _addressBooks[_skinsSource].Base,
                skinTypeSiteInformation.Address, skinTypeSiteInformation.Name));
        }

        ImmutableList<WebSkin> newWebSkins = [];
            
        foreach (var webSkinSiteInfo in newWebSkinSiteInfos){
            newWebSkins = newWebSkins.Add(await EphineaService.GetSkin(new HttpClient(), webSkinSiteInfo.Address, _addressBooks[_skinsSource].BaseScreenshots, webSkinSiteInfo.SkinType,  webSkinSiteInfo.SkinSubType));
        }

        return newWebSkins;
    }
    private async Task<ImmutableList<WebSkin>> GetUniversePsSkins() => await UniversePsWebAccessService.GetAvailableSkinsAsync(new HttpClient());
    public async Task<string> DownloadSkin(HttpClient httpClient, string skinName, int downloadLinkNumber){
        try{
            WebSkin skinToDownload = _gameSkins.Lefts().First(x => x.SkinName == skinName);
            string downloadLink = skinToDownload.DownloadLinks[downloadLinkNumber-1];
            HttpResponseMessage response =
                await httpClient.GetAsync(downloadLink);

            string fileExtension = response.RequestMessage?.RequestUri?.OriginalString.Split('.').Last() ??
                                   string.Empty;

            string filePath = Path.Combine(_psoGame.SkinsLocation, $"{skinToDownload.SkinName}.{fileExtension}");

            await using Stream webStream = await httpClient.GetStreamAsync(downloadLink);

            await using Stream skinFileStream = File.Create(filePath);
            await webStream.CopyToAsync(skinFileStream);
            return filePath;
        }
        catch (Exception ex){
            Console.WriteLine(ex);
            return string.Empty;
        }
    }
    private ImmutableList<Either<WebSkin, LocalSkin>> ChangeWebSkinToLocalSkin(string skinName, string newLocalSkinPath){
        WebSkin currentSkin = _gameSkins.Lefts().First(x => x.SkinName == skinName);
        return _gameSkins.Remove(currentSkin).Add(currentSkin.ToLocalSkin(newLocalSkinPath));
    }
    private void UpdateSkins(IEnumerable<LocalSkin> skins){
        if (_gameSkins.Count == 0){
            _gameSkins = _gameSkins.AddRange(skins.Select(currentSkin => new Either.Right<WebSkin, LocalSkin>(currentSkin)));
        }
        else{
            IEnumerable<LocalSkin> newSkins = skins
                .Where(tempSkin => !_gameSkins.Rights()
                    .Select(currentSkin => currentSkin.SkinName).Contains(tempSkin.SkinName));

           _gameSkins = _gameSkins.AddRange(newSkins.Select(currentSkin => new Either.Right<WebSkin, LocalSkin>(currentSkin)));
        }
    }
    private void UpdateSkins(IEnumerable<WebSkin> skins){
        if (_gameSkins.Count == 0){
            _gameSkins = _gameSkins.AddRange(skins.Select(currentSkin => new Either.Left<WebSkin, LocalSkin>(currentSkin)));
        }
        else{
            IEnumerable<WebSkin> newSkins = skins
                .Where(tempSkin => !_gameSkins.Lefts()
                    .Select(currentSkin => currentSkin.SkinName).Contains(tempSkin.SkinName));

            _gameSkins = _gameSkins.AddRange(newSkins.Select(currentSkin => new Either.Left<WebSkin, LocalSkin>(currentSkin)));
        }
    }
    public async Task<bool> ApplySkin(string skinName, bool isEphinea){
        string skinDirectory = _gameSkins.Rights().First(x => x.SkinName == skinName).SkinLocation;
        string gameDirectory = _psoGame.GameLocation;
        if (await FileAccessService.ApplySkinAsync(skinDirectory, gameDirectory)){
            _appliedSkins = AddAppliedSkin(skinName);
            return true;
        }

        return false;
    }
    public async Task<Fin<bool>> CreateBackup(string skinName){
        LocalSkin selectedSkin = _gameSkins.Rights().First(x => x.SkinName == skinName);
        string backupLocation = GetBackupLocation(skinName);
        return await FileAccessService.CreateBackUpAsync(selectedSkin.SkinLocation, backupLocation,
            _psoGame.GameLocation);
    }
    public async Task<bool> RestoreBackup(string skinName){
        LocalSkin selectedSkin = _gameSkins.Rights().First(x => x.SkinName == skinName);

        if (await FileAccessService.RestoreBackupAsync(selectedSkin.SkinLocation, _psoGame.GameLocation)){
            _appliedSkins = RemoveAppliedSkin(selectedSkin);
            return true;
        }
        else{
            return false;
        }
    }
    public async Task<Fin<bool>> BackUpExists(string selectedSkinName){
        if (_gameSkins.Rights().FirstOrDefault(x => x.SkinName == selectedSkinName) is{ } selectedSkin){
            string backupPath = Path.Combine(_psoGame.SkinsLocation, selectedSkin.SkinType, "Originals",
                selectedSkin.SkinSubType);
            return await FileAccessService.FolderHasFilesAsync(backupPath);
        }

        return false;
    }
    public string GetAppliedSkinName(string selectedSkinTypeName, string selectedSkinSubTypeName){
        return _appliedSkins.TryGetValue((selectedSkinTypeName, selectedSkinSubTypeName), out LocalSkin? appliedSkin)
            ? appliedSkin.SkinName
            : "None";
    }
    public ImmutableList<string> GetSkinTypes(){
        return _gameSkins.Lefts().Select(currentWebSkin => currentWebSkin.SkinType)
            .Concat(_gameSkins.Rights().Select(currentLocalSkin => currentLocalSkin.SkinType)).Distinct().ToImmutableList();
    }
    public ImmutableList<string> GetSkinSubTypes(string selectedSkinTypeName){
        return _gameSkins.Lefts().Where(currentWebSkin => currentWebSkin.SkinType  == selectedSkinTypeName)
            .Select(currentWebSkin => currentWebSkin.SkinSubType)
            .Concat(_gameSkins.Rights().Where(currentLocalSkin => currentLocalSkin.SkinType == selectedSkinTypeName)
                .Select(currentLocalSkin => currentLocalSkin.SkinSubType)).Distinct().ToImmutableList();
    }
    public ImmutableList<string> GetAvailableSkinNames(string skinType, string skinSubType){
        return _gameSkins.Lefts()
            .Where(x => x.SkinType == skinType &&
                        x.SkinSubType == skinSubType)
            .Select(x => x.SkinName)
            .Concat(_gameSkins.Rights()
                .Where(x => x.SkinType == skinType && x.SkinSubType == skinSubType)
                .Select(x => x.SkinName)).ToImmutableList();
    }
    public bool GetIsWebSkinSelected(string selectedSkinName) => _gameSkins.Lefts().Any(currentWebSkin => currentWebSkin.SkinName == selectedSkinName);
    public ImmutableList<string> GetSkinScreenshots(string skinName){
        return _gameSkins.Lefts().FirstOrDefault(currentSkin => currentSkin.SkinName == skinName)?.ScreenshotLinks
               ?? _gameSkins.Rights().First(currentSkin => currentSkin.SkinName == skinName).ScreenshotFileNames;
    }
    private string GetBackupLocation(string selectedSkinName){
        if (_gameSkins.Rights().FirstOrDefault(x => x.SkinName == selectedSkinName) is{ } selectedSkin){
            return Path.Combine(_psoGame.SkinsLocation, selectedSkin.SkinType, "Originals",
                selectedSkin.SkinSubType);
        }
        else{
            return string.Empty;
        }
    }
    public string GetGameExecutableLocation(){
        return _psoGame.GameExecutable;
    }
    public string GetGameLocation(){
        return _psoGame.GameLocation;
    }
    public string GetSkinsLocation(){
        return _psoGame.SkinsLocation;
    }
    public async Task<bool> ExtractSkin(string skinName, string archivePath){
        string fileExtension = Path.GetExtension(archivePath);

        if (fileExtension == "zip"){
            WebSkin skinToDownload = _gameSkins.Lefts().First(x => x.SkinName == skinName);
            string newSkinPath = Path.Combine(_psoGame.SkinsLocation, skinToDownload.SkinType,
                skinToDownload.SkinSubType, skinToDownload.SkinName);

            if (await FileAccessService.ExtractSkinAsync(archivePath, newSkinPath)){
                _gameSkins = ChangeWebSkinToLocalSkin(skinToDownload.SkinName, newSkinPath);
                return true;
            }
        }

        return false;
    }
    public async Task<Fin<bool>> SaveScreenshots(string skinName){
        LocalSkin theSkin = _gameSkins.Rights().First(x => x.SkinName == skinName);

        if (theSkin.ScreenshotFileNames.Any()){
            string screenshotsPath = Path.Combine(_psoGame.SkinsLocation, theSkin.SkinType,
                theSkin.SkinSubType, theSkin.SkinName, "Screenshots");
            //return await FileAccessService.SaveScreenshotsAsync(screenshotsPath, theSkin.ScreenshotFileNames);
            return true;
        }

        return false;
    }
    public async Task LoadInformation(Locations locations){
        if (await FileAccessService.LoadJsonToObject<GameInfo>(nameof(GameInfo) + ".json") is not{ } gameInfoResult){
            /*theMessenger.Send(new FatalErrorMessage("IO Exception",
                $"Unable to find the {locations.GameInfoFile} file."));#1#
        }
        else{
            gameInfoResult.IfSucc(possibleGameInfo => {
                possibleGameInfo.IfSome(gameInfo => {
                    _psoGame = gameInfo;
                });
            });
        }
        
        if (await FileAccessService.LoadJsonToObject<AddressBook>(nameof(SkinsSource.Ephinea) + "AddressBook.json") is not{ } addressBookResult){
            /*theMessenger.Send(new FatalErrorMessage("IO Exception",
                $"Unable to find the {nameof(SkinsSource.Ephinea) + "AddressBook.json"} file."));#1#
        }
        else{
            addressBookResult.IfSucc(possibleAddressBook => {
                possibleAddressBook.IfSome(theAddressBook => {
                    _addressBooks = _addressBooks.Add(SkinsSource.Ephinea,theAddressBook);
                });
            });
        }

        (await FileAccessService.LoadJsonToImmutableList<WebSkin>("CachedSkins.json")).IfSucc(UpdateSkins);
        (await FileAccessService.LoadJsonToImmutableList<LocalSkin>("AppliedSkins.json")).IfSucc(appliedSkins => _appliedSkins = UpdateAppliedSkins(appliedSkins));
    }
    private ImmutableDictionary<(string, string), LocalSkin> UpdateAppliedSkins(IEnumerable<LocalSkin> appliedSkins)
        => _appliedSkins.AddRange(appliedSkins.Select(currentSkin => new KeyValuePair<(string, string), LocalSkin>((currentSkin.SkinType, currentSkin.SkinSubType), currentSkin)));
    
    public async Task SaveInformation(Locations locations){
        List<Task> tasks =[
            FileAccessService.SaveObjectToJson(_psoGame, locations.GameInfoFile),
            FileAccessService.SaveObjectToJson(_gameSkins.Lefts(), locations.CachedSkinsFile),
            FileAccessService.SaveIEnumerableToJson(_appliedSkins, locations.AppliedSkinsFile)
        ];
        await Task.WhenAll(tasks);
    }
    public void SetSkinsSource(SkinsSource skinsSource){
        _skinsSource = skinsSource;
    }
    public void SetSkinsLocation(string skinsLocation){
        _psoGame = _psoGame with{ SkinsLocation = skinsLocation };
    }
    public void SetGameLocation(string gameLocation){
        _psoGame = _psoGame with{ GameLocation = gameLocation };
    }
    public void SetGameExecutableLocation(string gameExecutableLocation){
        _psoGame = _psoGame with{ GameExecutable = gameExecutableLocation };
    }
}*/