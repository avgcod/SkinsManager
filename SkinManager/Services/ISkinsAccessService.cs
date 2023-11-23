using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public interface ISkinsAccessService
    {
        IEnumerable<Skin> GetAvailableSkins(string skinsLocation);
        IEnumerable<string> GetSkinScreenshots(Skin currentSkin);
        Task<IEnumerable<string>> GetSkinScreenshotsAsync(Skin currentSkin);
        
        Task<IEnumerable<Skin>> GetAvailableSkinsAsync(string skinsDirectoryName);

    }
}
