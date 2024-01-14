using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinManager.Models
{
    public partial class KnownGameInfo : ObservableObject
    {
        [ObservableProperty]
        public string _gameName  = string.Empty;
        [ObservableProperty]
        public string _skinsSiteName  = string.Empty;
        [ObservableProperty]
        public string _skinsSiteAddress  = string.Empty;
        [ObservableProperty]
        public List<SkinType> _skinTypes  = [];
    }
}
