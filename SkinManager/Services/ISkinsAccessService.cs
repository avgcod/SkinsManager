using SkinManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public interface ISkinsAccessService
    {
        Task<IEnumerable<Skin>> GetAvailableSkinsForSpecificTypeAsync(SkinType skinType);
        Task<IEnumerable<Skin>> GetAvailableSkinsAsync();
    }
}
