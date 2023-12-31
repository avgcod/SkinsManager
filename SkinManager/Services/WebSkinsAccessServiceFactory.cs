using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public class WebSkinsAccessServiceFactory(Func<WebSkinsAccessService> createWebSkinsAccessService) : IWebSkinsAccessServiceFactory
    {

        private readonly Func<WebSkinsAccessService> _createWebSkinsAccessService = createWebSkinsAccessService;

        /// <summary>
        /// Creates an instance of a WebSkinsAccessService and returns it.
        /// </summary>
        /// <returns>A new WebSkinsAccessService instance.</returns>
        public ISkinsAccessService Create()
        {
            ISkinsAccessService webSkinsAccessService = _createWebSkinsAccessService();
            return webSkinsAccessService;
        }
    }
}
