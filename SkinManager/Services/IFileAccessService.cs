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
        Dictionary<string, List<Skin>> GetCachedWebSkins(IEnumerable<string> gameNames);
        bool RestoreBackup(string skinDirectoryName, string gameDirectoryName);
        Task<bool> RestoreBackupAsync(string skinDirectoryName, string gameDirectoryName);
        void StartGame(string fileLocation);
        Task StartGameAsync(string fileLocation);
    }
}