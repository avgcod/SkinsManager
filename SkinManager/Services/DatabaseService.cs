using LinqToDB;
using LinqToDB.Data;
using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public class DatabaseService
    {
        private readonly DataOptions dOptions;

        public DatabaseService(string connectionString)
        {
            dOptions = new DataOptions().UseSQLite(connectionString);
        }

        public async Task<bool> CreateGameInfo(GameInfo gameInfo)
        {
            using (DataConnection dataConnection = new DataConnection(dOptions))
            {
                return await dataConnection.InsertAsync<GameInfo>(gameInfo) == 1;
            }
        }

        public async Task<IEnumerable<GameInfo>> ReadAllGameInfo()
        {
            using (DataConnection dataConnection = new DataConnection(dOptions))
            {
                return await dataConnection.GetTable<GameInfo>().ToListAsync();
            }
        }

        public async Task<GameInfo> ReadGameInfo(int gameID)
        {
            using (DataConnection dataConnection = new DataConnection(dOptions))
            {
                return await dataConnection.GetTable<GameInfo>().Where(x => x.GameID == gameID).SingleAsync();
            }
        }

        public async Task<GameInfo> ReadGameInfo(string gameName)
        {
            using (DataConnection dataConnection = new DataConnection(dOptions))
            {
                return await dataConnection.GetTable<GameInfo>().Where(x => x.GameName == gameName).SingleAsync();
            }
        }

        public async Task<bool> UpdateGameInfo(GameInfo gameInfo)
        {
            using (DataConnection dataConnection = new DataConnection(dOptions))
            {
                return await dataConnection.UpdateAsync<GameInfo>(gameInfo) == 1;
            }
        }

        public async Task<bool> DeleteGameInfo(GameInfo gameInfo)
        {
            using (DataConnection dataConnection = new DataConnection(dOptions))
            {
                return await dataConnection.DeleteAsync<GameInfo>(gameInfo) == 1;
            }
        }
    }
}
