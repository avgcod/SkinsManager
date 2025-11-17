using System;
using System.Collections.Immutable;
using LanguageExt;

namespace SkinManager.Types;

public enum SkinsSource { Ephinea, Local, UniversPS }
public sealed record SkinSiteInformation(string SkinType, string SkinSubType, string Name, string Address);
public sealed record SkinTypeSiteInformation(string Name, string Address);

public sealed record GameInfo(
    string SkinsLocation,
    string GameLocation,
    string GameExecutable){
    public static GameInfo Empty() => new(string.Empty, string.Empty, string.Empty);
}

public sealed record AppSettings(string GameInfoFile, string AppliedSkinsFile, string CachedSkinsFile){
    public static AppSettings Empty() => new(string.Empty, string.Empty, string.Empty);
}

public abstract record Skin;

public sealed record WebSkin(string SkinName, string SkinType, string SkinSubType, string Address, string Author, ImmutableList<string> DownloadLinks, ImmutableList<string> ScreenshotLinks, SkinsSource Source) : Skin{
    public string Description => $"{SkinType} {SkinSubType} skin {SkinName}.";
    public override string ToString() => $"{Enum.GetName<SkinsSource>(Source)} Skin - {SkinName} - {Author}.";
}

public sealed record LocalSkin(string SkinName, string SkinType, string SkinSubType, string SkinLocation, string Author, ImmutableList<string> ScreenshotFileNames) : Skin{
    public string Description => $"{SkinType} {SkinSubType} skin {SkinName}.";
    public override string ToString() => $"Local Skin- {SkinName} - {Author}.";
}

public sealed record AddressBook(SkinsSource Source, string Main, string Base, string BaseScreenshots);

public sealed record SkinsState(
    ImmutableList<Either<WebSkin, LocalSkin>> GameSkins,
    ImmutableDictionary<(string, string), LocalSkin> AppliedSkins,
    ImmutableList<AddressBook> AddressBooks){
    public static SkinsState Empty() => new (ImmutableList<Either<WebSkin, LocalSkin>>.Empty, ImmutableDictionary<(string, string), LocalSkin>.Empty, ImmutableList<AddressBook>.Empty);
}