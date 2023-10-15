using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public interface ISkinsAccessService
    {
        IEnumerable<GameInfo> GetGameInfo(string gameInfoFileName);
        IEnumerable<string> GetAppliedSkins(string appliedSkinsFile);
        IEnumerable<Skin> GetAvailableSkins(string skinsFolder);
        IEnumerable<SkinType> GetSkinTypes(string subTypesFile);
        bool SaveGameInfo(IEnumerable<GameInfo> gameInfo, string gameInfoFileName);
        bool SaveAppliedSkins(IEnumerable<string> appliedSkins, string appliedSkinsFileName);
        bool SaveSkinTypes(IEnumerable<SkinType> skinTypes, string skinTypeSubTypesFile);
        bool CreateFolder(string location);
        bool CreateBackUp(string sourceLocation, string backUpLocation, string installationLocation);
        bool CreateStructure(IEnumerable<SkinType> skinTypes, string skinsFolder);
        bool ApplySkin(string sourceLocation, string destination);
        void StartGame(string fileLocation);
        bool RestoreBackup(string sourceLocation, string destinationLocation);
        Task<IEnumerable<GameInfo>> GetGameInfoAsync(string gameInfoFileName);
        Task<IEnumerable<string>> GetAppliedSkinsAsync(string appliedSkinsFile);
        Task<IEnumerable<Skin>> GetAvailableSkinsAsync(string skinsFolder);
        Task<IEnumerable<SkinType>> GetSkinTypesAsync(string subTypesFile);
        Task<bool> SaveGameInfoAsync(IEnumerable<GameInfo> gameInfo, string gameInfoFileName);
        Task<bool> SaveAppliedSkinsAsync(IEnumerable<string> appliedSkins, string appliedSkinsFileName);
        Task<bool> SaveSkinTypesAsync(IEnumerable<SkinType> skinTypes, string skinTypeSubTypesFile);
        Task<bool> CreateFolderAsync(string location);
        Task<bool> CreateBackUpAsync(string sourceLocation, string backUpLocation, string installationLocation);
        Task<bool> CreateStructureAsync(IEnumerable<SkinType> skinTypes, string skinsFolder);
        Task<bool> ApplySkinAsync(string sourceLocation, string destination);
        Task<bool> RestoreBackupAsync(string sourceLocation, string destinationLocation);
        Task StartGameAsync(string fileLocation);
    }
}
