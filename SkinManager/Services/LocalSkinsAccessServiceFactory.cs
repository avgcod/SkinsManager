using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public class LocalSkinsAccessServiceFactory(Func<LocalSkinsAccessService> createLocalSkinsAccessService) : ILocalSkinsAccessServiceFactory
    {
        private readonly Func<LocalSkinsAccessService> _createLocalSkinsAccessService = createLocalSkinsAccessService;

        /// <summary>
        /// Creates an instance of a LocalSkinsAccessService and returns it.
        /// </summary>
        /// <returns>A new LocalSkinsAccessService instance.</returns>
        public ISkinsAccessService Create()
        {
            ISkinsAccessService localSkinsAccessService = _createLocalSkinsAccessService();
            return localSkinsAccessService;
        }
    }
}
