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
    public class WebSkinsAccessService : ISkinsAccessService
    {
        private readonly IMessenger _theMessenger;

        public WebSkinsAccessService(IMessenger theMessenger)
        {
            _theMessenger = theMessenger;
        }

        public IEnumerable<Skin> GetAvailableSkins(string skinsLocation)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Skin>> GetAvailableSkinsAsync(string skinsDirectoryName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetSkinScreenshots(Skin currentSkin)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetSkinScreenshotsAsync(Skin currentSkin)
        {
            throw new NotImplementedException();
        }
    }
}
