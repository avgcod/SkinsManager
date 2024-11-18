using SkinManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public interface ISkinsAccessService
    {
        void AddAppliedSkin(string appliedSkinName);

        void AddGame(GameInfo currentGame);

        /// <summary>
        /// Downloads the specified skin and returns the contents as a stream.
        /// </summary>
        /// <param name="skinToDownload">The skin to download.</param>
        /// <param name="skinsFolder">Where to save the skin.</param>
        /// <param name="downloadLinkNumber">Which skin location download link to use.</param>
        /// <returns>If the operations succeeded.</returns>
        Task<bool> DownloadSkin(Skin skinToDownload, string skinsFolder, int downloadLinkNumber);

        /// <summary>
        /// Gets the available skins in a provided location.
        /// </summary>
        /// <returns>A collection of found skins.</returns>
        Task<IEnumerable<Skin>> GetAvailableSkinsAsync();
        IEnumerable<string> GetAvailableSkinNames(string selectedSkinTypeName, string selectedSkinSubTypeName);
        IEnumerable<SkinType> GetAvailableSkinTypes();
        Dictionary<string, List<Skin>> GetCachedWebSkins();
        string GetGameLocation();
        IEnumerable<string> GetGameNames();
        IEnumerable<GameInfo> GetGamesCollection();
        string GetGameExecutableLocation();
        IEnumerable<KnownGameInfo> GetKnownGames();
        Task<IEnumerable<Skin>> GetWebSkinsForSpecificSkinType(SkinType skinType);
        string GetAppliedSkinNameFromLocation(string selectedSkinTypeName, string selectedSkinSubTypeName);
        IEnumerable<string> GetOriginalSkinNames();
        string GetSkinsLocation();
        IEnumerable<SkinType> GetSkinTypesForWeb();

        Task LoadGamesInformation();
        Task SaveGamesInformation();
        void RemoveAppliedSkin(string removedSkinName);
        bool SelectedGameIsKnown(string gameName);

        /// <summary>
        /// Sets the current game to manage skins for.
        /// </summary>
        /// <param name="currentGameName">The name of the game to manage skins for.</param>
        void SetCurrentGame(string currentGameName);

        /// <summary>
        /// Sets the current skin source.
        /// </summary>
        /// <param name="skinsSource">The current skins source.</param>
        void SetSkinsSource(SkinsSource skinsSource);
    }
}
