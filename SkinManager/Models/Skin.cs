using System;
using System.Collections.Generic;

namespace SkinManager.Models
{
    public class Skin
    {
        public SkinType SkinType { get; set; } = new();
        public string SubType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsOriginal => Location.ToLower().Contains("originals", StringComparison.OrdinalIgnoreCase);
        public DateOnly CreationDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public DateOnly LastUpdatedDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
        public List<string> Screenshots { get; set; } = [];
        public bool IsWebSkin = false;
    }
}
