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
        Task<Dictionary<string, List<Skin>>> LoadCachedWebSkins(IEnumerable<string> gameNames);
        Task<IEnumerable<GameInfo>> LoadGameInfo(string gameInfoFileName);
        Task<IEnumerable<KnownGameInfo>> LoadKnownGamesInfo(string knownGameInfoFileName);
        Task<Dictionary<string, List<Skin>>> LoadWebSkins(IEnumerable<string> gameNames);
        bool RestoreBackup(string skinDirectoryName, string gameDirectoryName);
        Task<bool> RestoreBackupAsync(string skinDirectoryName, string gameDirectoryName);
        Task SaveGameInfo(IEnumerable<GameInfo> gameInfo, string gameInfoFileName);
        Task SaveKnownGamesList(IEnumerable<KnownGameInfo> knownGamesList, string fileName);
        Task SaveWebSkinsList(Dictionary<string, List<Skin>> webSkins);
        void StartGame(string fileLocation);
        Task StartGameAsync(string fileLocation);
    }
}