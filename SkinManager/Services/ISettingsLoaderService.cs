using SkinManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public interface ISettingsLoaderService
    {
        IEnumerable<GameInfo> GetGameInfo(string gameInfoFileName);
        Task<IEnumerable<GameInfo>> GetGameInfoAsync(string gameInfoFileName);
        IEnumerable<KnownGameInfo> GetKnowGamesInfo(string knownGameInfoFileName);
        Task<IEnumerable<KnownGameInfo>> GetKnowGamesInfoAsync(string knownGameInfoFileName);
        void SaveGameInfo(IEnumerable<GameInfo> gameInfo, string gameInfoFileName);
        Task SaveGameInfoAsync(IEnumerable<GameInfo> gameInfo, string gameInfoFileName);
        Task SaveKnownGamesListAsync(IEnumerable<KnownGameInfo> knownGamesList, string fileName);
    }
}