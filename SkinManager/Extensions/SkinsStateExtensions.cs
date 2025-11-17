using SkinManager.Types;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using LanguageExt;
using LanguageExt.Traits;

namespace SkinManager.Extensions;

public static class SkinsStateExtensions{

    public static SkinsState AddAppliedSkin(this SkinsState currentState, LocalSkin appliedSkin) 
        => currentState with { AppliedSkins = currentState.AppliedSkins
            .Remove((appliedSkin.SkinType, appliedSkin.SkinSubType))
            .Add((appliedSkin.SkinType, appliedSkin.SkinSubType), appliedSkin)};
    public static SkinsState AddAppliedSkins(this SkinsState currentState, IEnumerable<LocalSkin> appliedSkins)
        => currentState with { AppliedSkins = currentState.AppliedSkins.AddRange(appliedSkins.Select(currentSkin 
            => new KeyValuePair<(string, string), LocalSkin>((currentSkin.SkinType, currentSkin.SkinSubType), currentSkin)))};
    public static SkinsState RemoveAppliedSkin(this SkinsState currentState, LocalSkin appliedSkin)
        => currentState with { AppliedSkins = currentState.AppliedSkins.Remove((appliedSkin.SkinType, appliedSkin.SkinSubType))};
    public static SkinsState AddLocalSkins(this SkinsState currentState, IEnumerable<LocalSkin> localSkins)
        => currentState with { GameSkins = currentState.GameSkins.Union(localSkins.Select(currentSkin => new Either<WebSkin, LocalSkin>.Right(currentSkin))).ToImmutableList() };

    public static SkinsState AddWebSkins(this SkinsState currentState, IEnumerable<WebSkin> webSkins){
            var localSkinNames = currentState.GameSkins.Rights().Select(currentLocalSkin => currentLocalSkin.SkinName);
            var newWebSkins = webSkins.Aggregate(new List<Either<WebSkin, LocalSkin>.Left>(),
                (currentSkins, currentSkin) =>
                    localSkinNames.Contains(currentSkin.SkinName.RemoveSpecialCharacters()) ? currentSkins : [..currentSkins, new Either<WebSkin,LocalSkin>.Left(currentSkin)]);

            return currentState with{ GameSkins = currentState.GameSkins.AddRange(newWebSkins.ToImmutableList()) };
    }
    public static SkinsState AddSkins(this SkinsState currentState, IEnumerable<Either<WebSkin, LocalSkin>> newSkins){
        var split = newSkins.Partition();
        var localSkinNames = split.Item2.Select(currentLocalSkin => currentLocalSkin.SkinName);
        var newWebSkins = split.Item1.Aggregate(new List<Either<WebSkin, LocalSkin>>(),
            (currentSkins, currentSkin) =>
                localSkinNames.Contains(currentSkin.SkinName.RemoveSpecialCharacters()) ? currentSkins : [..currentSkins, new Either<WebSkin,LocalSkin>.Left(currentSkin)]);
        var combinedLists = newWebSkins.Concat(split.Item2.Select(currentLocalSkin => new Either<WebSkin,LocalSkin>.Right(currentLocalSkin)));
        return currentState with{ GameSkins = combinedLists.ToImmutableList() };
    }

    public static SkinsState ChangeWebSkinToLocalSkin(this SkinsState currentState, WebSkin currentSkin, string newLocalSkinPath) 
        => currentState with { GameSkins = currentState.GameSkins.Remove(currentSkin).Add(currentSkin.ToLocalSkin(newLocalSkinPath))};
    public static string GetAppliedSkinName(this SkinsState currentState, string selectedSkinTypeName, string selectedSkinSubTypeName) 
        => currentState.AppliedSkins.TryGetValue((selectedSkinTypeName, selectedSkinSubTypeName), out LocalSkin? appliedSkin)
            ? appliedSkin.SkinName
            : "None";
    public static IEnumerable<string> GetSkinTypes(this SkinsState currentState) 
        => currentState.GameSkins.Lefts().Select(currentWebSkin => currentWebSkin.SkinType)
            .Concat(currentState.GameSkins.Rights().Select(currentLocalSkin => currentLocalSkin.SkinType)).Distinct();
    public static IEnumerable<string> GetSkinSubTypes(this SkinsState currentState,string selectedSkinTypeName) 
        => currentState.GameSkins.Lefts().Where(currentWebSkin => currentWebSkin.SkinType  == selectedSkinTypeName)
            .Select(currentWebSkin => currentWebSkin.SkinSubType)
            .Concat(currentState.GameSkins.Rights().Where(currentLocalSkin => currentLocalSkin.SkinType == selectedSkinTypeName)
                .Select(currentLocalSkin => currentLocalSkin.SkinSubType)).Distinct();
    public static IEnumerable<string> GetAvailableSkinNames(this SkinsState currentState,string skinType, string skinSubType) 
        => currentState.GameSkins.Lefts()
            .Where(x => x.SkinType == skinType &&
                        x.SkinSubType == skinSubType)
            .Select(x => x.SkinName)
            .Concat(currentState.GameSkins.Rights()
                .Where(x => x.SkinType == skinType && x.SkinSubType == skinSubType)
                .Select(x => x.SkinName));

    public static IEnumerable<Skin> GetAvailableSkins(this SkinsState currentState, string skinType, string skinSubType,
        bool showWebSkins) =>
        showWebSkins ? GetAllAvailableSkins(currentState, skinType, skinSubType) : GetAllAvailableLocalSkins(currentState, skinType, skinSubType);
    
    private static IEnumerable<Skin> GetAllAvailableSkins(SkinsState currentState, string skinType, string skinSubType) 
        => Enumerable.Empty<Skin>().Concat(currentState.GameSkins.Lefts().Where(currentWebSkin => currentWebSkin.SkinType == skinType && currentWebSkin.SkinSubType == skinSubType))
            .Concat(currentState.GameSkins.Rights().Where(currentLocalSkin => currentLocalSkin.SkinType == skinType && currentLocalSkin.SkinSubType == skinSubType));

    private static IEnumerable<Skin>
        GetAllAvailableLocalSkins(SkinsState currentState, string skinType, string skinSubType) =>
        currentState.GameSkins.Rights().Where(currentLocalSkin =>
            currentLocalSkin.SkinType == skinType && currentLocalSkin.SkinSubType == skinSubType);
    
    public static bool GetIsWebSkinSelected(this SkinsState currentState, Option<Skin> selectedSkinOption) 
        => selectedSkinOption.Fold(false, (matchFound, selectedSkin) => 
            selectedSkin switch{
                WebSkin selectedWebSkin => currentState.GameSkins.Lefts()
                    .Any(currentWebSkin => currentWebSkin.SkinName == selectedWebSkin.SkinName),
                LocalSkin selectedLocalSkin => currentState.GameSkins.Rights()
                    .Any(currentLocalSkin => currentLocalSkin.SkinName == selectedLocalSkin.SkinName),
                _ => false
            }
        );
    public static IEnumerable<string> GetSkinScreenshots(this SkinsState currentState,string skinName) 
        => currentState.GameSkins.Lefts().FirstOrDefault(currentSkin => currentSkin.SkinName == skinName)?.ScreenshotLinks
           ?? currentState.GameSkins.Rights().First(currentSkin => currentSkin.SkinName == skinName).ScreenshotFileNames;
    public static string GetBackupLocation(this SkinsState currentState, string selectedSkinName, string skinsLocation)
        => currentState.GameSkins.Rights().FirstOrDefault(x => x.SkinName == selectedSkinName) is{ } selectedSkin
            ? Path.Combine(skinsLocation, selectedSkin.SkinType, "Originals",
                selectedSkin.SkinSubType)
            : string.Empty;
    
}