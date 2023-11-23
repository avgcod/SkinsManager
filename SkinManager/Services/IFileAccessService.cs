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
        IEnumerable<string> GetAppliedSkins(string appliedSkinsFile);
        Task<IEnumerable<string>> GetAppliedSkinsAsync(string appliedSkinsFile);
        IEnumerable<SkinType> GetSkinTypes(string skinTypesFileName);
        Task<IEnumerable<SkinType>> GetSkinTypesAsync(string skinTypesFileName);
        void RestoreBackup(string skinDirectoryName, string gameDirectoryName);
        Task RestoreBackupAsync(string skinDirectoryName, string gameDirectoryName);
        void SaveAppliedSkins(IEnumerable<string> appliedSkins, string appliedSkinsFileName);
        Task SaveAppliedSkinsAsync(IEnumerable<string> appliedSkins, string appliedSkinsFileName);
        void SaveSkinTypes(IEnumerable<SkinType> skinTypes, string skinTypesFileName);
        Task SaveSkinTypesAsync(IEnumerable<SkinType> skinTypes, string skinTypesFileName);
        void StartGame(string fileLocation);
        Task StartGameAsync(string fileLocation);
    }
}