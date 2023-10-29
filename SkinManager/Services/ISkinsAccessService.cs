using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public interface ISkinsAccessService
    {
        IEnumerable<string> GetAppliedSkins(string appliedSkinsFile, IMessenger messenger);
        IEnumerable<Skin> GetAvailableSkins(string skinsFolder, IMessenger messenger);
        IEnumerable<SkinType> GetSkinTypes(string subTypesFile, IMessenger messenger);
        void SaveAppliedSkins(IEnumerable<string> appliedSkins, string appliedSkinsFileName, IMessenger messenger);
        void SaveSkinTypes(IEnumerable<SkinType> skinTypes, string skinTypeSubTypesFile, IMessenger messenger);
        void CreateFolder(string location, IMessenger messenger);
        void CreateBackUp(string sourceLocation, string backUpLocation, string installationLocation, IMessenger messenger);
        void CreateStructure(IEnumerable<SkinType> skinTypes, string skinsFolder, IMessenger messenger);
        void ApplySkin(string sourceLocation, string destination, IMessenger messenger);
        void StartGame(string fileLocation, IMessenger messenger);
        void RestoreBackup(string sourceLocation, string destinationLocation, IMessenger messenger);
        Task<IEnumerable<string>> GetAppliedSkinsAsync(string appliedSkinsFile, IMessenger messenger);
        Task<IEnumerable<Skin>> GetAvailableSkinsAsync(string skinsFolder, IMessenger messenger);
        Task<IEnumerable<SkinType>> GetSkinTypesAsync(string subTypesFile, IMessenger messenger);
        Task SaveAppliedSkinsAsync(IEnumerable<string> appliedSkins, string appliedSkinsFileName, IMessenger messenger);
        Task SaveSkinTypesAsync(IEnumerable<SkinType> skinTypes, string skinTypeSubTypesFile, IMessenger messenger);
        Task CreateFolderAsync(string location, IMessenger messenger);
        Task CreateBackUpAsync(string sourceLocation, string backUpLocation, string installationLocation, IMessenger messenger);
        Task CreateStructureAsync(IEnumerable<SkinType> skinTypes, string skinsFolder, IMessenger messenger);
        Task ApplySkinAsync(string sourceLocation, string destination, IMessenger messenger);
        Task RestoreBackupAsync(string sourceLocation, string destinationLocation, IMessenger messenger);
        Task StartGameAsync(string fileLocation, IMessenger messenger);
    }
}
