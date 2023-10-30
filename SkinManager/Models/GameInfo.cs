using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace SkinManager.Models
{
    public partial class GameInfo : ObservableObject
    {
        [ObservableProperty]
        public string _gameName = string.Empty;
        [ObservableProperty]
        public string _skinsLocation = string.Empty;
        [ObservableProperty]
        public string _gameLocation = string.Empty;
        [ObservableProperty]
        public string _gameExecutable = string.Empty;
        [ObservableProperty]
        public List<string> _appliedSkins  = new List<string>();
    }
}
