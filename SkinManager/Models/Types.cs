using System.Collections.Immutable;

namespace SkinManager.Types;

public enum SkinsSource { Ephinea, Local, UniversPS }
public sealed record SkinSiteInformation(string SkinType, string SkinSubType, string Name, string Address);
public sealed record SkinTypeSiteInformation(string Name, string Address);

public sealed record GameInfo(
    string SkinsLocation,
    string GameLocation,
    string GameExecutable);

public sealed record Locations(string GameInfoFile, string AppliedSkinsFile, string CachedSkinsFile);

public sealed record WebSkin(string SkinName, string SkinType, string SkinSubType, string Address, string Author, ImmutableList<string> DowloadLinks, ImmutableList<string> ScreenshotLinks){
    public string Description => $"{SkinType} {SkinSubType} skin {SkinName}.";
}

public sealed record LocalSkin(string SkinName, string SkinType, string SkinSubType, string SkinLocation, string Author, ImmutableList<string> ScreenshotFileNames){
    public string Description => $"{SkinType} {SkinSubType} skin {SkinName}.";
}

public sealed record AddressBook(string Main, string Base, string BaseScreenshots);