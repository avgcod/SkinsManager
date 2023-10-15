using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public class UniversePSAccessService : ISkinsAccessService
    {
        public bool ApplySkin(string sourceLocation, string destination)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ApplySkinAsync(string sourceLocation, string destination)
        {
            throw new NotImplementedException();
        }

        public bool CreateBackUp(string sourceLocation, string backUpLocation, string installationLocation)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateBackUpAsync(string sourceLocation, string backUpLocation, string installationLocation)
        {
            throw new NotImplementedException();
        }

        public bool CreateFolder(string location)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateFolderAsync(string location)
        {
            throw new NotImplementedException();
        }

        public bool CreateStructure(IEnumerable<SkinType> skinTypes, string skinsFolder)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateStructureAsync(IEnumerable<SkinType> skinTypes, string skinsFolder)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DirectoryExistsAsync(string directoryName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> FileExistsAsync(string fileName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetAppliedSkins(string appliedSkinsFile)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetAppliedSkinsAsync(string appliedSkinsFile)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Skin> GetAvailableSkins(string skinsFolder)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Skin>> GetAvailableSkinsAsync(string skinsFolder)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<GameInfo> GetGameInfo(string gameInfoFileName)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<GameInfo>> GetGameInfoAsync(string gameInfoFileName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SkinType> GetSkinTypes(string subTypesFile)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<SkinType>> GetSkinTypesAsync(string subTypesFile)
        {
            throw new NotImplementedException();
        }

        public bool RestoreBackup(string originalLocation, string backupLocation)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RestoreBackupAsync(string originalLocation, string backupLocation)
        {
            throw new NotImplementedException();
        }

        public bool SaveAppliedSkins(IEnumerable<string> appliedSkins, string appliedSkinsFileName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveAppliedSkinsAsync(IEnumerable<string> appliedSkins, string appliedSkinsFileName)
        {
            throw new NotImplementedException();
        }

        public bool SaveGameInfo(IEnumerable<GameInfo> gameInfo, string gameInfoFileName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveGameInfoAsync(IEnumerable<GameInfo> gameInfo, string gameInfoFileName)
        {
            throw new NotImplementedException();
        }

        public bool SaveSkinTypes(IEnumerable<SkinType> skinTypes, string skinTypeSubTypesFile)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveSkinTypesAsync(IEnumerable<SkinType> skinTypes, string skinTypeSubTypesFile)
        {
            throw new NotImplementedException();
        }

        public void StartGame(string fileLocation)
        {
            throw new NotImplementedException();
        }

        public Task StartGameAsync(string fileLocation)
        {
            throw new NotImplementedException();
        }
    }
}
