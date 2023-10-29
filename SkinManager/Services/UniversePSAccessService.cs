using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public class UniversePSAccessService : ISkinsAccessService
    {
        private readonly string _siteAddress;

        public UniversePSAccessService(string siteAddress)
        {
            _siteAddress = siteAddress;
        }
        public void ApplySkin(string sourceLocation, string destination, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public Task ApplySkinAsync(string sourceLocation, string destination, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public void CreateBackUp(string sourceLocation, string backUpLocation, string installationLocation, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public Task CreateBackUpAsync(string sourceLocation, string backUpLocation, string installationLocation, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public void CreateFolder(string location, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public Task CreateFolderAsync(string location, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public void CreateStructure(IEnumerable<SkinType> skinTypes, string skinsFolder, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public Task CreateStructureAsync(IEnumerable<SkinType> skinTypes, string skinsFolder, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetAppliedSkins(string appliedSkinsFile, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetAppliedSkinsAsync(string appliedSkinsFile, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Skin> GetAvailableSkins(string skinsFolder, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Skin>> GetAvailableSkinsAsync(string skinsFolder, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<GameInfo> GetGameInfo(string gameInfoFileName, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<GameInfo>> GetGameInfoAsync(string gameInfoFileName, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SkinType> GetSkinTypes(string subTypesFile, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<SkinType>> GetSkinTypesAsync(string subTypesFile, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public void RestoreBackup(string sourceLocation, string destinationLocation, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public Task RestoreBackupAsync(string sourceLocation, string destinationLocation, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public void SaveAppliedSkins(IEnumerable<string> appliedSkins, string appliedSkinsFileName, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public Task SaveAppliedSkinsAsync(IEnumerable<string> appliedSkins, string appliedSkinsFileName, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public void SaveGameInfo(IEnumerable<GameInfo> gameInfo, string gameInfoFileName, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public Task SaveGameInfoAsync(IEnumerable<GameInfo> gameInfo, string gameInfoFileName, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public void SaveSkinTypes(IEnumerable<SkinType> skinTypes, string skinTypeSubTypesFile, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public Task SaveSkinTypesAsync(IEnumerable<SkinType> skinTypes, string skinTypeSubTypesFile, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public void StartGame(string fileLocation, IMessenger messenger)
        {
            throw new NotImplementedException();
        }

        public Task StartGameAsync(string fileLocation, IMessenger messenger)
        {
            throw new NotImplementedException();
        }
    }
}
