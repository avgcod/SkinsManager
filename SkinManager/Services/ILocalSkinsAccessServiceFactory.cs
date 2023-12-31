using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public interface ILocalSkinsAccessServiceFactory
    {
        /// <summary>
        /// Creates an instance of an ISkinsAccessService
        /// </summary>
        /// <returns></returns>
        ISkinsAccessService Create();
    }
}
