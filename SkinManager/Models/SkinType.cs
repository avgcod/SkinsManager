using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkinManager.Models;

/*
public class SkinType
{
    public string Name { get; init; }
    public List<string> SubTypes { get; init; }
        
    private SkinType(string name, List<string> subTypes)
    {
        Name = name;
        SubTypes = subTypes;
    }

    public static SkinType Create(string name, List<string> subTypes)
        => new SkinType(name, subTypes);

    public override string ToString()
    {
        return $"This is skin type {Name}.";
    }

}
*/

public record SkinType(string Name, List<string> SubTypes, DateOnly LastOnlineCheck)
{
    public static SkinType Create(string name, List<string> subTypes, DateOnly lastOnlineCheck)
        => new SkinType(name, subTypes, lastOnlineCheck);

    public override string ToString()
    {
        StringBuilder fullString = new StringBuilder();
        
        fullString.Append($"This is skin type {Name} and it was last checked on {LastOnlineCheck}. The subtypes are ");
        
        foreach (string subType in SubTypes)
        {
            fullString.Append($"{subType}, ");
        }
        
        return fullString.ToString();
    }
}