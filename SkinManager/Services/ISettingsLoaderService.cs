using SkinManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public interface ISettingsLoaderService
    {
        IEnumerable<GameInfo> GetGameInfo(string gameInfoFileName);
        IEnumerable<KnownGameInfo> GetKnowGamesInfo(string knownGameInfoFileName);
        void SaveGameInfo(IEnumerable<GameInfo> gameInfo, string gameInfoFileName);
        void SaveKnownGamesList(IEnumerable<KnownGameInfo> knownGamesList, string fileName);
        Dictionary<string, List<Skin>> GetWebSkins(IEnumerable<string> gameNames);
        void SaveWebSkinsList(Dictionary<string, List<Skin>> webSkins);
    }
}