using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SkinManager.Models
{
    public class SkinType
    {
        public string Name { get; set; } = string.Empty;
        public List<string> SubTypes { get; set; } = [];
        public DateOnly LastOnlineCheck { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
    }
}
