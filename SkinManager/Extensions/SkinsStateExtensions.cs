using System;
using SkinManager.Types;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using LanguageExt;

namespace SkinManager.Extensions;

public static class SkinsStateExtensions{

    public static SkinsState AddAppliedSkin(this SkinsState currentState, LocalSkin appliedSkin){
        if(currentState.AppliedSkins.Count == 0) return currentState with {  AppliedSkins = currentState.AppliedSkins.Add(appliedSkin) };
        return currentState.AppliedSkins.Any(currentAppliedSkin =>
            currentAppliedSkin.SkinType == appliedSkin.SkinType &&
            currentAppliedSkin.SkinSubType == appliedSkin.SkinSubType)
            ? currentState with{
                AppliedSkins = currentState.AppliedSkins.Where(currentAppliedSkin =>
                    currentAppliedSkin.SkinType != appliedSkin.SkinType &&
                    currentAppliedSkin.SkinSubType != appliedSkin.SkinSubType).Concat([appliedSkin]).ToImmutableList()
            }
            : currentState with{ AppliedSkins = currentState.AppliedSkins.Add(appliedSkin) };
    }

    public static SkinsState AddAppliedSkins(this SkinsState currentState, IEnumerable<LocalSkin> appliedSkins){
        if(currentState.AppliedSkins.Count == 0) return currentState with {  AppliedSkins = appliedSkins.ToImmutableList() };
        return currentState with{
            AppliedSkins = currentState.AppliedSkins.Aggregate(new List<LocalSkin>(), (newAppliedSkins, currentAppliedSkin)
                => appliedSkins.Any(newAppliedSkin => currentAppliedSkin.SkinType == newAppliedSkin.SkinType &&
                                                      currentAppliedSkin.SkinSubType == newAppliedSkin.SkinSubType)
                        ? newAppliedSkins
                        : [..newAppliedSkins, currentAppliedSkin]
                ).ToImmutableList() };
    }
    public static SkinsState RemoveAppliedSkin(this SkinsState currentState, LocalSkin appliedSkin)
        => currentState with { AppliedSkins = currentState.AppliedSkins.Where(currentAppliedSkin => currentAppliedSkin.SkinType != appliedSkin.SkinType && currentAppliedSkin.SkinSubType != appliedSkin.SkinSubType).ToImmutableList()};
    
    public static SkinsState AddLocalSkin(this SkinsState currentState, LocalSkin localSkin){
        if(currentState.LocalSkins.Count == 0) return currentState with { LocalSkins = currentState.LocalSkins.Add(localSkin) };
        
        return currentState.LocalSkins.Any(currentLocalSkin => currentLocalSkin.SkinName == localSkin.SkinName) 
            ? currentState 
            : currentState with {LocalSkins = currentState.LocalSkins.Add(localSkin)};
    }
    public static SkinsState AddLocalSkins(this SkinsState currentState, IEnumerable<LocalSkin> localSkins){
        if(currentState.LocalSkins.Count == 0) return currentState with { LocalSkins = localSkins.ToImmutableList() };
        
        var newLocalSkins = currentState.LocalSkins.Aggregate(new List<LocalSkin>(),
            (currentSkins, currentSkin) =>
                currentState.WebSkins.Any(currenLocalSkin => currenLocalSkin.SkinName == currentSkin.SkinName) ? currentSkins : [..currentSkins, currentSkin]);
        return currentState with{ LocalSkins = currentState.LocalSkins.Union(newLocalSkins).ToImmutableList() };
    }
    
    public static SkinsState ReplaceLocalSkins(this SkinsState currentState, IEnumerable<LocalSkin> localSkins)
    => currentState with  { LocalSkins = localSkins.ToImmutableList() };
    
    public static SkinsState ReplaceWebSkins(this SkinsState currentState, IEnumerable<WebSkin> webSkins)
        => currentState with  { WebSkins = webSkins.ToImmutableList() };

    public static SkinsState AddWebSkins(this SkinsState currentState, IEnumerable<WebSkin> webSkins){
        if(currentState.WebSkins.Count == 0) return currentState with { WebSkins = webSkins.ToImmutableList() };
        
            var newWebSkins = currentState.WebSkins.Aggregate(new List<WebSkin>(),
                (currentSkins, currentSkin) =>
                    currentState.WebSkins.Any(currentWebSkin => currentWebSkin.SkinName == currentSkin.SkinName) ? currentSkins : [..currentSkins, currentSkin]);

            return currentState with{ WebSkins = currentState.WebSkins.AddRange(newWebSkins.ToImmutableList()) };
    }
    public static SkinsState AddSkins(this SkinsState currentState, IEnumerable<WebSkin> newSkins, IEnumerable<LocalSkin> localSkins) => currentState.AddWebSkins(newSkins).AddLocalSkins(localSkins);

    private static SkinsState RemoveWebSkin(this SkinsState currentState, WebSkin webSkin) => currentState with {WebSkins = currentState.WebSkins.Remove(webSkin)};

    public static SkinsState ChangeWebSkinToLocalSkin(this SkinsState currentState, WebSkin currentSkin, string newLocalSkinPath) 
        => currentState.RemoveWebSkin(currentSkin).AddLocalSkin(currentSkin.ToLocalSkin(newLocalSkinPath));
    public static Option<LocalSkin> GetAppliedSkin(this SkinsState currentState, string skinType, string skinSubType) 
        => currentState.AppliedSkins.SingleOrDefault(currentAppliedSkin => currentAppliedSkin.SkinType == skinType && currentAppliedSkin.SkinSubType == skinSubType) is { } appliedSkin
            ? appliedSkin
            : Option<LocalSkin>.None;

    public static Skin GetSkinFromDisplaySkin(this SkinsState currentState, DisplaySkin currentSkin){
        Skin? foundSkin = currentState.WebSkins.FirstOrDefault(currentWebSkin
            => currentWebSkin.SkinName == currentSkin.SkinName
               && currentWebSkin.SkinType == currentSkin.SkinType
               && currentWebSkin.SkinSubType == currentSkin.SkinSubType);
        
        return foundSkin ?? currentState.LocalSkins.First(currentLocalSkin
                   => currentLocalSkin.SkinName == currentSkin.SkinName
                      && currentLocalSkin.SkinType == currentSkin.SkinType
                      && currentLocalSkin.SkinSubType == currentSkin.SkinSubType);
    }

    public static IEnumerable<Skin> SearchForSkins(this SkinsState currentState, string searchText) =>
        currentState.WebSkins.Where(currentWebSkin
                => currentWebSkin.SkinName.Contains(searchText, StringComparison.OrdinalIgnoreCase) || currentWebSkin.SkinType.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                   currentWebSkin.SkinSubType.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .Concat<Skin>(currentState.LocalSkins.Where(currentLocalSkin
                => currentLocalSkin.SkinName.Contains(searchText, StringComparison.OrdinalIgnoreCase) || currentLocalSkin.SkinType.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                   currentLocalSkin.SkinSubType.Contains(searchText, StringComparison.OrdinalIgnoreCase)));

    public static IEnumerable<string> GetSkinTypes(this SkinsState currentState) 
        => currentState.WebSkins.Select(currentWebSkin => currentWebSkin.SkinType)
            .Concat(currentState.LocalSkins.Select(currentLocalSkin => currentLocalSkin.SkinType)).Distinct();
    public static IEnumerable<string> GetSkinSubTypes(this SkinsState currentState,string selectedSkinTypeName) 
        => currentState.WebSkins.Where(currentWebSkin => currentWebSkin.SkinType  == selectedSkinTypeName)
            .Select(currentWebSkin => currentWebSkin.SkinSubType)
            .Concat(currentState.LocalSkins.Where(currentLocalSkin => currentLocalSkin.SkinType == selectedSkinTypeName)
                .Select(currentLocalSkin => currentLocalSkin.SkinSubType)).Distinct();
    public static IEnumerable<string> GetAvailableSkinNames(this SkinsState currentState,string skinType, string skinSubType) 
        => currentState.WebSkins
            .Where(x => x.SkinType == skinType &&
                        x.SkinSubType == skinSubType)
            .Select(x => x.SkinName)
            .Concat(currentState.LocalSkins
                .Where(x => x.SkinType == skinType && x.SkinSubType == skinSubType)
                .Select(x => x.SkinName));

    public static IEnumerable<Skin> GetAvailableSkins(this SkinsState currentState, string skinType, string skinSubType,
        bool showWebSkins) =>
        showWebSkins ? GetAllAvailableSkins(currentState, skinType, skinSubType) : GetAllAvailableLocalSkins(currentState, skinType, skinSubType);
    
    private static IEnumerable<Skin> GetAllAvailableSkins(SkinsState currentState, string skinType, string skinSubType) =>
        currentState.WebSkins.Where(currentWebSkin => currentWebSkin.SkinType == skinType &&  currentWebSkin.SkinSubType == skinSubType)
            .Concat<Skin>(currentState.LocalSkins.Where(currentLocalSkin => currentLocalSkin.SkinType == skinType &&  currentLocalSkin.SkinSubType == skinSubType));

    private static IEnumerable<Skin>
        GetAllAvailableLocalSkins(SkinsState currentState, string skinType, string skinSubType) =>
        currentState.LocalSkins.Where(currentLocalSkin =>
            currentLocalSkin.SkinType == skinType && currentLocalSkin.SkinSubType == skinSubType);
    
    public static bool GetIsWebSkinSelected(this SkinsState currentState, Option<Skin> selectedSkinOption) 
        => selectedSkinOption.Match(currentSkin => currentSkin is WebSkin ? true : false, () => false);
    public static bool GetIsWebSkinSelected(this SkinsState currentState, Option<DisplaySkin> selectedSkinOption) 
        => selectedSkinOption.Match(currentSkin => currentState.GetSkinFromDisplaySkin(currentSkin) switch{
            WebSkin => true,
            LocalSkin => false
        }, () => false);
    public static IEnumerable<string> GetSkinScreenshots(this SkinsState currentState,string skinName) 
        => currentState.WebSkins.FirstOrDefault(currentSkin => currentSkin.SkinName == skinName)?.ScreenshotLinks
           ?? currentState.LocalSkins.First(currentSkin => currentSkin.SkinName == skinName).ScreenshotFileNames;
    public static string GetBackupLocation(this SkinsState currentState, string selectedSkinName, string skinsLocation)
        => currentState.LocalSkins.FirstOrDefault(x => x.SkinName == selectedSkinName) is{ } selectedSkin
            ? Path.Combine(skinsLocation, selectedSkin.SkinType, "Originals",
                selectedSkin.SkinSubType)
            : string.Empty;
    
}