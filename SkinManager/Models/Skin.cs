using System;
using System.Collections.Generic;
using SkinManager.Extensions;

namespace SkinManager.Models;
public record Skin(
    string SkinType,
    string SubType,
    string Name,
    List<string> Locations,
    string Author,
    string Description,
    DateOnly CreationDate,
    DateOnly LastUpdatedDate,
    List<string> Screenshots,
    SkinsSource Source)
{
    public static Skin Create(string skinType, string subType, string name, IEnumerable<string> locations, string author,
        string description,
        DateOnly creationDate, DateOnly lastUpdatedDate, IEnumerable<string> screenshots, SkinsSource source)
        => new Skin(skinType, subType, name, [..locations], author, description, creationDate, lastUpdatedDate, [..screenshots], source);

    public override string ToString()
    {
        
        if (!this.IsWebSkin())
        {
            return $"This is skin {Name} located at {Locations[0]}.";
        }
        else
        {
            if (Locations.Count > 1)
            {
                return $"This is skin {Name} and the download links are {string.Join(", ", Locations)}.";
            }
            else
            {
                return $"This is skin {Name} and the download link is {Locations[0]}.";
            }
        }
    }
}