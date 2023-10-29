using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public interface ISkinsAccessService
    {
        IEnumerable<string> GetAppliedSkins(string appliedSkinsFileName, IMessenger messenger);
        IEnumerable<Skin> GetAvailableSkins(string skinsDirectoryName, IMessenger messenger);
        IEnumerable<SkinType> GetSkinTypes(string skinTypesFileName, IMessenger messenger);
        void SaveAppliedSkins(IEnumerable<string> appliedSkins, string appliedSkinsFileName, IMessenger messenger);
        void SaveSkinTypes(IEnumerable<SkinType> skinTypes, string skinTypesFileName, IMessenger messenger);
        void CreateBackUp(string skinDirectoryName, string backUpDirectoryName, string gameDirectoryName, IMessenger messenger);
        void CreateStructure(IEnumerable<SkinType> skinTypes, string skinsFolderName, IMessenger messenger);
        void ApplySkin(string skinDirectoryName, string gameDirectoryName, IMessenger messenger);
        void StartGame(string gameExecutableFileName, IMessenger messenger);
        void RestoreBackup(string backupDirectoryName, string gameDirectoryName, IMessenger messenger);
        Task<IEnumerable<string>> GetAppliedSkinsAsync(string appliedSkinsFileName, IMessenger messenger);
        Task<IEnumerable<Skin>> GetAvailableSkinsAsync(string skinsDirectoryName, IMessenger messenger);
        Task<IEnumerable<SkinType>> GetSkinTypesAsync(string skinTypesFileName, IMessenger messenger);
        Task SaveAppliedSkinsAsync(IEnumerable<string> appliedSkins, string appliedSkinsFileName, IMessenger messenger);
        Task SaveSkinTypesAsync(IEnumerable<SkinType> skinTypes, string skinTypesFileName, IMessenger messenger);
        Task CreateBackUpAsync(string skinDirectoryName, string backUpDirectoryName, string gameDirectoryName, IMessenger messenger);
        Task CreateStructureAsync(IEnumerable<SkinType> skinTypes, string skinsFolderName, IMessenger messenger);
        Task ApplySkinAsync(string skinDirectoryName, string gameDirectoryName, IMessenger messenger);
        Task RestoreBackupAsync(string backupDirectoryName, string gameDirectoryName, IMessenger messenger);
        Task StartGameAsync(string gameExecutableFileName, IMessenger messenger);
    }
}
