using System.Collections.Generic;

namespace SkinManager.Models
{
    public class KnownGameInfo
    {
        public string GameName { get; set; } = string.Empty;
        public string SkinsSiteName { get; set; } = string.Empty;
        public string SkinsSiteAddress { get; set; } = string.Empty;
        public List<SkinType> SkinTypes { get; set; } = [];
    }
}
