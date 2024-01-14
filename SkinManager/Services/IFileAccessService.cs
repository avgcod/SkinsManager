using SkinManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public interface IFileAccessService
    {
        void ApplySkin(string skinDirectoryName, string gameDirectoryName);
        Task ApplySkinAsync(string skinDirectoryName, string gameDirectoryName);
        void CreateBackUp(string skinDirectoryName, string backUpDirectoryName, string gameDirectoryName);
        Task CreateBackUpAsync(string skinDirectoryName, string backUpDirectoryName, string gameDirectoryName);
        void CreateStructure(IEnumerable<SkinType> skinTypes, string skinsFolderName);
        Task CreateStructureAsync(IEnumerable<SkinType> skinTypes, string skinsFolderName);
        Dictionary<string, List<Skin>> LoadCachedWebSkins(IEnumerable<string> gameNames);
        IEnumerable<GameInfo> LoadGameInfo(string gameInfoFileName);
        IEnumerable<KnownGameInfo> LoadKnownGamesInfo(string knownGameInfoFileName);
        Dictionary<string, List<Skin>> LoadWebSkins(IEnumerable<string> gameNames);
        bool RestoreBackup(string skinDirectoryName, string gameDirectoryName);
        Task<bool> RestoreBackupAsync(string skinDirectoryName, string gameDirectoryName);
        void SaveGameInfo(IEnumerable<GameInfo> gameInfo, string gameInfoFileName);
        void SaveKnownGamesList(IEnumerable<KnownGameInfo> knownGamesList, string fileName);
        void SaveWebSkinsList(Dictionary<string, List<Skin>> webSkins);
        void StartGame(string fileLocation);
        Task StartGameAsync(string fileLocation);
    }
}