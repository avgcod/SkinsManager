using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public class WebSkinsAccessServiceFactory : IWebSkinsAccessServiceFactory
    {

        private readonly Func<WebSkinsAccessService> _createWebSkinsAccessService;
        public WebSkinsAccessServiceFactory(Func<WebSkinsAccessService> createWebSkinsAccessService)
        {

            _createWebSkinsAccessService = createWebSkinsAccessService;

        }

        public ISkinsAccessService Create()
        {
            ISkinsAccessService webSkinsAccessService = _createWebSkinsAccessService();
            return webSkinsAccessService;
        }
    }
}
