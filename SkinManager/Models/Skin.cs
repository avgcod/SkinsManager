using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace SkinManager.Models
{
    public partial class Skin : ObservableObject
    {
        [ObservableProperty]
        public SkinType _skinType  = new();
        [ObservableProperty]
        public string _subType  = string.Empty;
        [ObservableProperty]
        public string _name  = string.Empty;
        [ObservableProperty]
        public string _location  = string.Empty;
        [ObservableProperty]
        public string _author  = string.Empty;
        [ObservableProperty]
        public string _description  = string.Empty;
        public bool IsOriginal => Location.ToLower().Contains("originals", StringComparison.OrdinalIgnoreCase);
        [ObservableProperty]
        public DateOnly _creationDate  = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        public DateOnly _lastUpdatedDate  = DateOnly.FromDateTime(DateTime.Now);
        [ObservableProperty]
        public List<string> _screenshots  = [];
        public bool IsWebSkin = false;
    }
}
