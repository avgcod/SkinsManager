using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public interface ISkinsAccessService
    {
        /// <summary>
        /// Gets the available skins in a provided location.
        /// </summary>
        /// <param name="skinsLocation">The location to look for skins at.</param>
        /// <returns>A collection of found skins.</returns>
        IEnumerable<Skin> GetAvailableSkins(string skinsLocation);
        
        /// <summary>
        /// Get the location of screenshots for a provided skin.
        /// </summary>
        /// <param name="currentSkin">The skin to get screen shots of.</param>
        /// <returns>Collection of found screen shot full qualified names.</returns>
        IEnumerable<string> GetSkinScreenshots(Skin currentSkin);

        /// <summary>
        /// Get the location of screenshots for a provided skin.
        /// </summary>
        /// <param name="currentSkin">The skin to get screen shots of.</param>
        /// <returns>Collection of found screen shot full qualified names.</returns>
        Task<IEnumerable<string>> GetSkinScreenshotsAsync(Skin currentSkin);
        /// <summary>
        /// Gets the available skins in a provided location.
        /// </summary>
        /// <param name="skinsLocation">The location to look for skins at.</param>
        /// <returns>A collection of found skins.</returns>
        Task<IEnumerable<Skin>> GetAvailableSkinsAsync(string skinsDirectoryName);

    }
}
