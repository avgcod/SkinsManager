using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkinManager.Models;

public record SkinType(string Name, List<string> SubTypes, Dictionary<SkinsSource, DateOnly> LastOnlineChecks)
{
    public static SkinType Create(string name, List<string> subTypes,  Dictionary<SkinsSource, DateOnly> lastOnlineChecks)
        => new SkinType(name, subTypes, lastOnlineChecks);

    public override string ToString()
    {
        StringBuilder fullString = new StringBuilder();
        
        fullString.Append($"This is skin type {Name} and it was last checked on {LastOnlineChecks}. The subtypes are ");
        
        foreach (string subType in SubTypes)
        {
            fullString.Append($"{subType}, ");
        }
        
        return fullString.ToString();
    }
}