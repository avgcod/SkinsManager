using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SkinManager.Models
{
    public partial class SkinType : ObservableObject
    {
        [ObservableProperty]
        public string _name  = string.Empty;
        [ObservableProperty]
        public List<string> _subTypes  = [];
        public DateOnly LastOnlineCheck { get; set; } = DateOnly.FromDateTime(DateTime.Now.AddDays(-7));
    }
}
