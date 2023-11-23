using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public class LocalSkinsAccessServiceFactory : ILocalSkinsAccessServiceFactory
    {
        private readonly Func<LocalSkinsAccessService> _createLocalSkinsAccessService;
        public LocalSkinsAccessServiceFactory(Func<LocalSkinsAccessService> createLocalSkinsAccessService)
        {

            _createLocalSkinsAccessService = createLocalSkinsAccessService;

        }
        public ISkinsAccessService Create()
        {
            ISkinsAccessService localSkinsAccessService = _createLocalSkinsAccessService();
            return localSkinsAccessService;
        }
    }
}
