using System;
using System.Collections.Generic;

namespace SkinManager.Models;

/*
public class Skin
{
    public string SkinType { get; init; }
    public string SubType { get; init; }
    public string Name { get; init; }
    public string Location { get; init; }
    public string Author { get; init; }
    public string Description { get; init; }
    public bool IsOriginal => Location.ToLower().Contains("originals", StringComparison.OrdinalIgnoreCase);
    public DateOnly CreationDate { get; init; }
    public DateOnly LastUpdatedDate { get; init; }
    public List<string> Screenshots { get; init; }
    public bool IsWebSkin { get; init; }


    private Skin(string skinType, string subType, string name, string location, string author, string description,
        DateOnly creationDate, DateOnly lastUpdatedDate, bool isWebSkin, IEnumerable<string> screenshots)
    {
        SkinType = skinType;
        SubType = subType;
        Name = name;
        Location = location;
        Author = author;
        Description = description;
        CreationDate = creationDate;
        LastUpdatedDate = lastUpdatedDate;
        IsWebSkin = isWebSkin;
        Screenshots = [..screenshots];
    }

    public static Skin Create(string skinType, string subType, string name, string location, string author,
        string description,
        DateOnly creationDate, DateOnly lastUpdatedDate, bool isWebSkin, IEnumerable<string> screenshots)
        => new Skin(skinType, subType, name, location, author, description, creationDate, lastUpdatedDate, isWebSkin, screenshots);

    public override string ToString()
    {
        return $"This is skin {Name} located at {Location}.";
    }
}
*/
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
    bool IsWebSkin)
{
    public static Skin Create(string skinType, string subType, string name, IEnumerable<string> locations, string author,
        string description,
        DateOnly creationDate, DateOnly lastUpdatedDate, bool isWebSkin, IEnumerable<string> screenshots)
        => new Skin(skinType, subType, name, [..locations], author, description, creationDate, lastUpdatedDate, [..screenshots], isWebSkin);

    public override string ToString()
    {
        
        if (!IsWebSkin)
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