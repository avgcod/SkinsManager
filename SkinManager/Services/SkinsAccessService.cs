using Microsoft.Extensions.DependencyInjection;
using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using SkinManager.Extensions;

namespace SkinManager.Services
{
    public class SkinsAccessService(
        IServiceScopeFactory scopeFactory,
        IFileAccessService fileAccessService,
        HttpClient httplCient,
        Locations locations) : ISkinsAccessService
    {
        private readonly Locations _locations = locations;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private List<KnownGameInfo> _knownGames = [];
        private List<GameInfo> _games = [];
        private int _currentGameIndex = -1;
        private SkinsSource _skinsSource = SkinsSource.Local;
        private readonly IFileAccessService _fileAccessService = fileAccessService;
        private readonly HttpClient _httpClient = httplCient;
        private Dictionary<string, List<Skin>> _gameSkins = [];

        public void AddAppliedSkin(string appliedSkinName)
        {
            if (!_games[_currentGameIndex].AppliedSkins.Contains(appliedSkinName))
            {
                _games[_currentGameIndex].AppliedSkins.Add(appliedSkinName);
            }
        }

        private void AddCachedGameWebSkins(Dictionary<string, List<Skin>> webSkins)
        {
            foreach (KeyValuePair<string, List<Skin>> currentPair in webSkins)
            {
                foreach (Skin currentSkin in currentPair.Value)
                {
                    if (!_gameSkins[currentPair.Key]
                            .Any(x => x.Name.Equals(currentSkin.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        _gameSkins[currentPair.Key].Add(currentSkin);
                    }
                }
            }
        }

        public void AddGame(GameInfo currentGame)
        {
            var newGame = currentGame.With(("SkinTypes", PopulateDefaultSkinTypes()));
            
            if (newGame is Something<GameInfo> something)
            {
                _games.Add(something.Value);
            }
        }

        public async Task<bool> DownloadSkin(Skin skinToDownload, string skinsFolder, int downloadLink)
        {
            if (_knownGames.Any(x => x.GameName == _games[_currentGameIndex].GameName))
            {
                if (_games[_currentGameIndex].GameName == "Phantasy Star Online BB")
                {
                    using IServiceScope serviceScope = _scopeFactory.CreateScope();
                    
                    EphineaWebAccessService skinsAccessService =
                        serviceScope.ServiceProvider.GetRequiredService<EphineaWebAccessService>();

                    try
                    {
                        HttpResponseMessage response = await _httpClient.GetAsync(skinToDownload.Locations[downloadLink]);
                        
                        string fileExtension = response.RequestMessage?.RequestUri?.OriginalString.Split('.').Last() ??
                                               string.Empty;
                        
                        string filePath = Path.Combine(skinsFolder, $"{skinToDownload.Name}.{fileExtension}");
                        
                        await using Stream webStream = await _httpClient.GetStreamAsync(skinToDownload.Locations[downloadLink]);
                        
                        await using Stream skinFileStream = File.Create(filePath);
                        
                        await webStream.CopyToAsync(skinFileStream);
                        
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        #region Get Methods

        private async Task<IEnumerable<Skin>> GetAllWebSkins()
        {
            if (_knownGames.SingleOrDefault(x => x.GameName == _games[_currentGameIndex].GameName) is
                { } currentKnownGame)
            {
                if (_games[_currentGameIndex].GameName == "Phantasy Star Online BB")
                {
                    using IServiceScope serviceScope = _scopeFactory.CreateScope();

                    var webAccessService =
                        serviceScope.ServiceProvider.GetRequiredService<EphineaWebAccessService>();

                    AddGameToGameSkinsIfMissing();

                    AddGameSkinsIfMissing(await webAccessService.GetAvailableSkinsAsync(currentKnownGame.SkinTypes));
                    
                    _knownGames.Remove(currentKnownGame);
                    
                    List<SkinType> newSkinTypes = [];
                    
                    foreach (SkinType skinType in currentKnownGame.SkinTypes)
                    {
                        newSkinTypes.Add(skinType with {LastOnlineCheck = DateOnly.FromDateTime(DateTime.Now)});
                        
                    }
                    
                    _knownGames.Add(currentKnownGame with { SkinTypes = newSkinTypes });
                    
                    return _gameSkins[_games[_currentGameIndex].GameName];
                }

                return [];
            }
            else
            {
                return [];
            }
        }

        private void AddGameSkinsIfMissing(IEnumerable<Skin> skins)
        {
            if (_gameSkins.Count == 0)
            {
                _gameSkins[_games[_currentGameIndex].GameName].AddRange(skins);
            }
            else
            {
                foreach (Skin currentSkin in skins)
                {
                    if (!_gameSkins[_games[_currentGameIndex].GameName].Exists(x => x.Name == currentSkin.Name))
                    {
                        _gameSkins[_games[_currentGameIndex].GameName].Add(currentSkin);
                    }
                } 
            }
        }

        private void AddGameToGameSkinsIfMissing()
        {
            if (!_gameSkins.ContainsKey(_games[_currentGameIndex].GameName))
            {
                _gameSkins.Add(_games[_currentGameIndex].GameName, []);
            }
        }

        public string GetAppliedSkinNameFromLocation(string selectedSkinTypeName, string selectedSkinSubTypeName)
        {
            foreach (string skinLocation in _games[_currentGameIndex].AppliedSkins)
            {
                var possibleSkin = (_gameSkins[_games[_currentGameIndex].GameName]
                    .SingleOrDefault(x => x.Locations.Contains(skinLocation))) is { } foundSkinLocation
                    ? Something<Skin>.Create(foundSkinLocation)
                    : Nothing<Skin>.Create();

                if (possibleSkin is Something<Skin> foundSkin && foundSkin.Value.Name == selectedSkinTypeName && foundSkin.Value.SubType == selectedSkinSubTypeName)
                {
                    return foundSkin.Value.Name;
                }
            }

            return "None";
        }

        public async Task<IEnumerable<Skin>> GetAvailableSkinsAsync()
        {
            if (_skinsSource == SkinsSource.Local)
            {
                return await GetLocalSkins();
            }
            else
            {
                return await GetAllWebSkins();
            }
        }

        public Dictionary<string, List<Skin>> GetCachedWebSkins()
        {
            Dictionary<string, List<Skin>> cachedWebSkins = [];
            
            foreach (KeyValuePair<string, List<Skin>> currentPair in _gameSkins)
            {
                cachedWebSkins.Add(currentPair.Key, currentPair.Value.Where(x => x.IsWebSkin).ToList());
            }

            return _gameSkins;
        }

        public IEnumerable<string> GetAvailableSkinNames(string selectedSkinTypeName, string selectedSkinSubTypeName)
        {
            foreach (string currentSkinName in _gameSkins[_games[_currentGameIndex].GameName].Where(x =>
                         x.SkinType == selectedSkinTypeName
                         && x.SubType == selectedSkinSubTypeName).Select(x => x.Name))
            {
                yield return currentSkinName;
            }
        }

        public IEnumerable<SkinType> GetAvailableSkinTypes()
        {
            return _games[_currentGameIndex].SkinTypes.DistinctBy(x => x.Name);
        }

        public IEnumerable<GameInfo> GetGamesCollection()
        {
            return _games;
        }

        public string GetGameExecutableLocation()
        {
            return _games[_currentGameIndex].GameExecutable;
        }

        public string GetGameLocation()
        {
            return _games[_currentGameIndex].GameLocation;
        }

        public IEnumerable<string> GetGameNames()
        {
            List<string> games = [.._games.Select(x => x.GameName)];
            
            foreach (KnownGameInfo currentKnownGame in _knownGames)
            {
                if (!games.Contains(currentKnownGame.GameName))
                {
                    games.Add(currentKnownGame.GameName);
                }
            }

            return games;
        }

        public IEnumerable<KnownGameInfo> GetKnownGames()
        {
            return _knownGames;
        }

        private async Task<IEnumerable<Skin>> GetLocalSkins()
        {
            if (!_gameSkins.ContainsKey(_games[_currentGameIndex].GameName))
            {
                _gameSkins.Add(_games[_currentGameIndex].GameName, []);
            }

            using IServiceScope serviceScope = _scopeFactory.CreateScope();
            
            LocalSkinsAccessService skinsAccessService =
                
                serviceScope.ServiceProvider.GetRequiredService<LocalSkinsAccessService>();

            IEnumerable<Skin> skins = await skinsAccessService.GetAvailableSkinsAsync(_games[_currentGameIndex]
                .SkinsLocation, _games[_currentGameIndex].SkinTypes);
            
            AddGameSkinsIfMissing(skins);

            //AddCachedGameWebSkins(_fileAccessService.LoadCachedWebSkins(_knownGames.Select(x => x.GameName)));

            return _gameSkins[_games[_currentGameIndex].GameName].OrderBy(x => x.IsWebSkin).ThenBy(x => x.Name);
        }

        public IEnumerable<string> GetOriginalSkinNames()
        {
            foreach (string currentSkinName in _gameSkins[_games[_currentGameIndex].GameName].Where(currentSkin => currentSkin.IsOriginal())
                         .Select(x => x.Name))
            {
                yield return currentSkinName;
            }
        }

        public string GetSkinsLocation()
        {
            return _games[_currentGameIndex].SkinsLocation;
        }

        public IEnumerable<SkinType> GetSkinTypesForWeb()
        {
            if (_knownGames.Any(x => x.GameName == _games[_currentGameIndex].GameName))
            {
                return _knownGames.Single(x => x.GameName == _knownGames[_currentGameIndex].GameName).SkinTypes;
            }
            else
            {
                return [];
            }
        }
        public async Task<IEnumerable<Skin>> GetWebSkinsForSpecificSkinType(SkinType skinType)
        {
            if (_knownGames.SingleOrDefault(x => x.GameName == _games[_currentGameIndex].GameName) is
                { } currentKnownGame)
            {
                if (_games[_currentGameIndex].GameName == "Phantasy Star Online BB")
                {
                    using IServiceScope serviceScope = _scopeFactory.CreateScope();

                    var webAccessService =
                        serviceScope.ServiceProvider.GetRequiredService<EphineaWebAccessService>();

                    AddGameSkinsIfMissing(await webAccessService.GetAvailableSkinsForSpecificTypeAsync(skinType));

                    List<SkinType> currentSkinTypes = currentKnownGame.SkinTypes;

                    SkinType tempSkinType = currentSkinTypes
                        .First(x => x.Name == skinType.Name);
                    
                    currentSkinTypes.Remove(tempSkinType);
                    
                    currentSkinTypes.Add(tempSkinType with {LastOnlineCheck = DateOnly.FromDateTime(DateTime.Now)});
                    
                    _knownGames.Remove(currentKnownGame);
                    
                    _knownGames.Add(currentKnownGame with { SkinTypes = currentSkinTypes });
                    
                    return _gameSkins[_games[_currentGameIndex].GameName];
                }

                return [];
            }
            else
            {
                return [];
            }
        }

        #endregion

        public async Task LoadGamesInformation()
        {
            await LoadKnownGames(_locations.KnownGamesFile);
            await LoadGames(_locations.GameInfoFile);
        }

        private async Task LoadGames(string gamesInfoFile)
        {
            _games = [..await _fileAccessService.LoadGameInfo(gamesInfoFile)];
        }

        private async Task LoadKnownGames(string knownGamesInfoFile)
        {
            _knownGames = [..await _fileAccessService.LoadKnownGamesInfo(knownGamesInfoFile)];
        }

        /// <summary>
        /// Generates a collection of default SkinTypes for games that are not known.
        /// </summary>
        /// <returns>The default collection of SkinType</returns>
        private List<SkinType> PopulateDefaultSkinTypes()
        {
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
            
            List<SkinType> skinTypes =
            [
                SkinType.Create("Area", ["Forest"], currentDate),
                SkinType.Create("Audio", ["System", "Effect", "Character", "Enemy"], currentDate),
                SkinType.Create("Class", ["Warrior"], currentDate),
                SkinType.Create("Effect", ["Fire"], currentDate),
                SkinType.Create("Enemy", ["Grunt"], currentDate),
                SkinType.Create("Equipment", ["Armor", "Shield", "Weapon"], currentDate),
                SkinType.Create("Object", ["Box"], currentDate),
                SkinType.Create("Helper", ["Guardian"], currentDate),
                SkinType.Create("NPC", ["GuildMaster"], currentDate),
                SkinType.Create("UI", ["HUD"], currentDate)

            ];

            return skinTypes;
        }

        public void RemoveAppliedSkin(string removedSkinName)
        {
            if (!_games[_currentGameIndex].AppliedSkins.Contains(removedSkinName))
            {
                _games[_currentGameIndex].AppliedSkins.Remove(removedSkinName);
            }
        }

        private async Task SaveCachedWebSkins()
        {
            await _fileAccessService.SaveWebSkinsList(GetCachedWebSkins());
        }

        private async Task SaveGames()
        {
            await _fileAccessService.SaveGameInfo(GetGamesCollection(), _locations.GameInfoFile);
        }

        public async Task SaveGamesInformation()
        {
            await SaveKnownGames();
            await SaveGames();
            await SaveCachedWebSkins();
        }

        private async Task SaveKnownGames()
        {
            await _fileAccessService.SaveKnownGamesList(GetKnownGames(), _locations.KnownGamesFile);
        }

        public bool SelectedGameIsKnown(string gameName)
        {
            return _knownGames.Any(x => x.GameName == gameName);
        }

        public void SetCurrentGame(string currentGameName)
        {
            _currentGameIndex = _games.FindIndex(x => x.GameName == currentGameName);
        }

        public void SetSkinsSource(SkinsSource skinsSource)
        {
            _skinsSource = skinsSource;
        }
    }
}