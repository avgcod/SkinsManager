using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SkinManager.Models;

namespace SkinManager.Extensions;

public static class SkinExtensions
{
    public static bool IsOriginal(this Skin theSkin) => theSkin.Locations.Count == 1 &&
        theSkin.Locations[0].ToLower().Contains("originals", StringComparison.OrdinalIgnoreCase);
    
    public static bool IsWebSkin(this Skin theSkin) => theSkin.Source != SkinsSource.Local;
}