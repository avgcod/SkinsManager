using Avalonia.Media.Imaging;
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
        Task<IEnumerable<Skin>> GetAvailableSkinsAsync();

        /// <summary>
        /// Sets the current game to manage skins for.
        /// </summary>
        /// <param name="currentGame">The game to manage skins for.</param>
        void SetCurrentGame(string currentGameName);

        /// <summary>
        /// Sets the current skin source.
        /// </summary>
        /// <param name="skinsSource">The current skins source.</param>
        void SetSkinsSource(SkinsSource skinsSource);

        /// <summary>
        ///Downloads the specified skin and returns the contents as a stream.
        /// </summary>
        /// <param name="skinToDownload">the skin to download.</param>
        /// <param name="skinsFolder">Where to save the skin.</param>
        /// <returns>If the operations succeeded.</returns>
        Task<bool> DownloadSkin(Skin skinToDownload, string skinsFolder);

        Dictionary<string, List<Skin>> GetCachedWebSkins();
        Task<IEnumerable<Skin>> GetWebSkinsForSpecificSkinType(string skinTypeName);
        IEnumerable<KnownGameInfo> GetKnownGames();
        bool SelectedGameIsKnown(string gameName);
        void Initialize(string knownGamesInfoFile, string gamesInfoFile);

        IEnumerable<string> GetGameNames();
        IEnumerable<GameInfo> GetGamesCollection();
        IEnumerable<SkinType> GetAvailableSkinTypes();
        void AddAppliedSkin(string appliedSkinName);
        void RemoveAppliedSkin(string removedSkinName);
        string GetAppliedSkinNameFromLocation(string selectedSkinTypeName, string selectedSkinSubypeName);
        IEnumerable<string> GetAvailableSkinNames(string selectedSkinTypeName, string selectedSkinSubypeName);
        IEnumerable<string> GetOriginalSkinNames();
        string GetSkinsLocation();
        string GetGameLocation();
        string GetGameExecutableLocation();
        IEnumerable<SkinType> GetSkinTypesForWeb();

    }
}
