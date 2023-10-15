using System.Collections.Generic;

namespace SkinManager.Models
{
    public class GameInfo
    {
        public string GameName { get; set; } = string.Empty;
        public string SkinsLocation { get; set; } = string.Empty;
        public string GameLocation { get; set; } = string.Empty;
        public string GameExecutable { get; set; } = string.Empty;
        public List<string> AppliedSkins { get; set; } = new List<string>();
    }
}
