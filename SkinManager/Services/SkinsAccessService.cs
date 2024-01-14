using Microsoft.Extensions.DependencyInjection;
using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public class SkinsAccessService(IServiceScopeFactory scopeFactory, IFileAccessService fileAccessService, HttpClient httplCient, Locations locations) : ISkinsAccessService
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
                    if (!_gameSkins[currentPair.Key].Any(x => x.Name.Equals(currentSkin.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        _gameSkins[currentPair.Key].Add(currentSkin);
                    }
                }
            }
        }
        public void AddGame(GameInfo currentGame)
        {
            currentGame.SkinTypes = PopulateDefaultSkinTypes();
            _games.Add(currentGame);
        }

        public async Task<bool> DownloadSkin(Skin skinToDownload, string skinsFolder)
        {
            if (_knownGames.Any(x => x.GameName == _games[_currentGameIndex].GameName))
            {
                if (_games[_currentGameIndex].GameName == "Phantasy Star Online BB")
                {
                    using IServiceScope serviceScope = _scopeFactory.CreateScope();
                    PSOUniversePSWebAccessService skinsAccessService = serviceScope.ServiceProvider.GetRequiredService<PSOUniversePSWebAccessService>();

                    try
                    {
                        HttpResponseMessage response = await _httpClient.GetAsync(skinToDownload.Location);
                        string fileExtension = response.RequestMessage?.RequestUri?.OriginalString.Split('.').Last() ?? string.Empty;
                        string filePath = Path.Combine(skinsFolder, $"{skinToDownload.Name}.{fileExtension}");
                        await using Stream webStream = await _httpClient.GetStreamAsync(skinToDownload.Location);
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
            KnownGameInfo? currentKnownGame = _knownGames.SingleOrDefault(x => x.GameName == _games[_currentGameIndex].GameName);
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
            if (currentKnownGame is not null)
            {
                if (_games[_currentGameIndex].GameName == "Phantasy Star Online BB")
                {
                    using IServiceScope serviceScope = _scopeFactory.CreateScope();
                    PSOUniversePSWebAccessService webAccessService = serviceScope.ServiceProvider.GetRequiredService<PSOUniversePSWebAccessService>();
                    webAccessService.SkinTypes = _games[_currentGameIndex].SkinTypes;
                    if (!_gameSkins.ContainsKey(_games[_currentGameIndex].GameName))
                    {
                        _gameSkins.Add(_games[_currentGameIndex].GameName, []);
                    }
                    foreach (Skin currentSkin in await webAccessService.GetAvailableSkinsAsync())
                    {
                        if (!_gameSkins[_games[_currentGameIndex].GameName].Any(x => x.Name == currentSkin.Name))
                        {
                            _gameSkins[_games[_currentGameIndex].GameName].Add(currentSkin);
                        }
                    }
                    int knownGameIndex = _knownGames.FindIndex(x => x.GameName == currentKnownGame.GameName);
                    for (int i = 0; i < _knownGames[knownGameIndex].SkinTypes.Count; i++)
                    {
                        _knownGames[knownGameIndex].SkinTypes[i].LastOnlineCheck = DateOnly.FromDateTime(DateTime.Now);
                    }
                    return _gameSkins[_games[_currentGameIndex].GameName];
                }
                return [];
            }
            else
            {
                return [];
            }
        }
        public string GetAppliedSkinNameFromLocation(string selectedSkinTypeName, string selectedSkinSubTypeName)
        {
            foreach (string skinLocation in _games[_currentGameIndex].AppliedSkins)
            {
                Skin? foundSkin = _gameSkins[_games[_currentGameIndex].GameName].SingleOrDefault(x => x.Location == skinLocation);
                if (foundSkin?.SkinType.Name == selectedSkinTypeName && foundSkin?.SubType == selectedSkinSubTypeName)
                {
                    return foundSkin.Name;
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
            foreach (string currentSkinName in _gameSkins[_games[_currentGameIndex].GameName].Where(x => x.SkinType.Name == selectedSkinTypeName
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
            List<string> games = [];
            games.AddRange(_games.Select(x => x.GameName));
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
            LocalSkinsAccessService skinsAccessService = serviceScope.ServiceProvider.GetRequiredService<LocalSkinsAccessService>();
            foreach (Skin currentSkin in await skinsAccessService.GetAvailableSkinsAsync(_games[_currentGameIndex].SkinsLocation))
            {
                if (!_gameSkins[_games[_currentGameIndex].GameName].Any(x => x.Name == currentSkin.Name))
                {
                    _gameSkins[_games[_currentGameIndex].GameName].Add(currentSkin);
                }
            }

            AddCachedGameWebSkins(_fileAccessService.LoadCachedWebSkins(_knownGames.Select(x => x.GameName)));

            return _gameSkins[_games[_currentGameIndex].GameName].OrderBy(x => x.IsWebSkin).ThenBy(x => x.Name);
        }
        public IEnumerable<string> GetOriginalSkinNames()
        {
            foreach (string currentSkinName in _gameSkins[_games[_currentGameIndex].GameName].Where(x => x.IsOriginal).Select(x => x.Name))
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
        public async Task<IEnumerable<Skin>> GetWebSkinsForSpecificSkinType(string skinTypeName)
        {
            KnownGameInfo? currentKnownGame = _knownGames.Single(x => x.GameName == _games[_currentGameIndex].GameName);
            int knownGameIndex = _knownGames.FindIndex(x => x.GameName == currentKnownGame.GameName);
            int skinTypeIndex = _knownGames[knownGameIndex].SkinTypes.FindIndex(x => x.Name == skinTypeName);
            if ((DateOnly.FromDateTime(DateTime.Now).DayNumber - _knownGames[knownGameIndex].SkinTypes[skinTypeIndex].LastOnlineCheck.DayNumber) > 1)
            {
                using IServiceScope serviceScope = _scopeFactory.CreateScope();
                PSOUniversePSWebAccessService webAccessService = serviceScope.ServiceProvider.GetRequiredService<PSOUniversePSWebAccessService>();
                webAccessService.SkinTypes = _games[_currentGameIndex].SkinTypes.DistinctBy(x => x.Name).ToList();
                if (!_gameSkins.ContainsKey(_games[_currentGameIndex].GameName))
                {
                    _gameSkins.Add(_games[_currentGameIndex].GameName, []);
                }
                foreach (Skin currentSkin in await webAccessService.GetAvailableSkinsForSpecificTypeAsync(skinTypeName))
                {
                    if (!_gameSkins[_games[_currentGameIndex].GameName].Any(x => x.Name == currentSkin.Name))
                    {
                        _gameSkins[_games[_currentGameIndex].GameName].Add(currentSkin);
                    }
                }

                _knownGames[knownGameIndex].SkinTypes[skinTypeIndex].LastOnlineCheck = DateOnly.FromDateTime(DateTime.Now);
                return _gameSkins[_games[_currentGameIndex].GameName];
            }
            else
            {
                return _gameSkins[_games[_currentGameIndex].GameName];
            }
        }
        #endregion        
        public void LoadGamesInformation()
        {
            LoadKnownGames(_locations.KnownGamesFile);
            LoadGames(_locations.GameInfoFile);
        }
        public void LoadGames(string gamesInfoFile)
        {
            _games = new(_fileAccessService.LoadGameInfo(gamesInfoFile));
        }
        public void LoadKnownGames(string knownGamesInfoFile)
        {
            _knownGames = new(_fileAccessService.LoadKnownGamesInfo(knownGamesInfoFile));
            for (int i = 0; i < _knownGames.Count; i++)
            {
                _knownGames[i].SkinTypes = new(_knownGames[i].SkinTypes.DistinctBy(x => x.Name));
            }
        }

        /// <summary>
        /// Generates a collection of default SkinTypes for games that are not known.
        /// </summary>
        /// <returns>The default collection of SkinType</returns>
        private List<SkinType> PopulateDefaultSkinTypes()
        {
            List<SkinType> skinTypes = [];
            skinTypes.Add(new SkinType() { Name = "Area", SubTypes = ["Forest"] });
            skinTypes.Add(new SkinType() { Name = "Audio", SubTypes = ["System", "Effect", "Character", "Enemy"] });
            skinTypes.Add(new SkinType() { Name = "Class", SubTypes = ["Warrior"] });
            skinTypes.Add(new SkinType() { Name = "Effect", SubTypes = ["Fire"] });
            skinTypes.Add(new SkinType() { Name = "Enemy", SubTypes = ["Grunt"] });
            skinTypes.Add(new SkinType() { Name = "Equipment", SubTypes = ["Armor", "Shield", "Weapon"] });
            skinTypes.Add(new SkinType() { Name = "Object", SubTypes = ["Box"] });
            skinTypes.Add(new SkinType() { Name = "Helper", SubTypes = ["Guardian"] });
            skinTypes.Add(new SkinType() { Name = "NPC", SubTypes = ["GuildMaster"] });
            skinTypes.Add(new SkinType() { Name = "UI", SubTypes = ["HUD"] });
            return skinTypes;
        }

        public void RemoveAppliedSkin(string removedSkinName)
        {
            if (!_games[_currentGameIndex].AppliedSkins.Contains(removedSkinName))
            {
                _games[_currentGameIndex].AppliedSkins.Remove(removedSkinName);
            }
        }

        public void SaveCachedWebSkins()
        {
            _fileAccessService.SaveWebSkinsList(GetCachedWebSkins());
        }
        public void SaveGames()
        {
            _fileAccessService.SaveGameInfo(GetGamesCollection(), _locations.GameInfoFile);
        }
        public void SaveGamesInformation()
        {
            SaveKnownGames();
            SaveGames();
            SaveCachedWebSkins();
        }
        public void SaveKnownGames()
        {
            _fileAccessService.SaveKnownGamesList(GetKnownGames(), _locations.KnownGamesFile);
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
