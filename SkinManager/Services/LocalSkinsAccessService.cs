using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using SkinManager.Types;

namespace SkinManager.Services;

public static class LocalSkinsAccessService{
    public static async Task<Fin<ImmutableList<LocalSkin>>> GetAvailableSkinsAsync(string skinsFolder)
    {
        try{
            Fin<ImmutableList<LocalSkin>> skins = Fin<ImmutableList<LocalSkin>>.Fail("");
            if((await Task.Run(() => Directory.Exists(skinsFolder))))
             skins = await GetSkins(skinsFolder);
            return skins;
        }
        catch (Exception ex){
            return Fin<ImmutableList<LocalSkin>>.Fail(ex);
        }
    }

    private static async Task<ImmutableList<LocalSkin>> GetSkins(string skinsFolder)
    {
        List<LocalSkin> skins = [];
        DirectoryInfo rootDirectory = new(skinsFolder);

        foreach (DirectoryInfo skinTypeDirectory in rootDirectory.GetDirectories())
        {
            foreach (DirectoryInfo subTypeDirectory in skinTypeDirectory.GetDirectories()
                         .Where(x => x.Name != "Originals"))
            {
                foreach (DirectoryInfo skinDirectory in subTypeDirectory.GetDirectories())
                {
                    string screenshotsFolder = Path.Combine(subTypeDirectory.FullName, "Screenshots");
                    
                    skins.Add(await CreateTempSkin(skinDirectory, screenshotsFolder));
                }
            }
        }

        return skins.ToImmutableList();
    }

    private static async Task<LocalSkin> CreateTempSkin(DirectoryInfo skinDirectory, string screenshotsFolder)
    {
        string skinName = skinDirectory.Name;
        string skinType = skinDirectory!.Parent!.Parent!.Name;
        string skinSubType = skinDirectory.Parent.Name;
        string author = skinDirectory.Name.Split("_by_").LastOrDefault() ?? string.Empty;
        return new LocalSkin(skinName, skinType, skinSubType, skinDirectory.FullName, author,
            [..await GetScreenShots(screenshotsFolder)]);
    }

    private static async Task<ImmutableList<string>> GetScreenShots(string screenshotsFolder){
        bool folderExists = false;
        (await DirectoryExistsAsync(screenshotsFolder)).IfSucc(exists => folderExists = exists);
        if (folderExists)
        {
            return new DirectoryInfo(screenshotsFolder).GetFiles().Select(x => x.FullName).ToImmutableList();
        }
        else
        {
            return [];
        }
    }
    
    private static async Task<Fin<bool>> DirectoryExistsAsync(string directoryName)
    {
        try
        {
            return await Task.Run(() => Directory.Exists(directoryName));
        }
        catch (Exception ex)
        {
            return Fin<bool>.Fail(ex);
        }
    }
}