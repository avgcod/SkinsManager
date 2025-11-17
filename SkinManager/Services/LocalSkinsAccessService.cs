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
    public static Fin<IEnumerable<LocalSkin>> GetAvailableSkinsAsync(string skinsFolder)
    {
        try{
            if(Directory.Exists(skinsFolder))
             return Fin.Succ(GetSkins(skinsFolder));
            return Fin.Fail<IEnumerable<LocalSkin>>($"Unable to find skins folder : {skinsFolder}.");
        }
        catch (Exception ex){
            return Fin.Fail<IEnumerable<LocalSkin>>(ex);
        }
    }

    private static IEnumerable<LocalSkin> GetSkins(string skinsFolder)
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
                    string screenshotsFolder = Path.Combine(skinDirectory.FullName, "Screenshots");
                    
                    skins.Add(CreateTempSkin(skinDirectory, screenshotsFolder));
                }
            }
        }

        return skins;
    }

    private static LocalSkin CreateTempSkin(DirectoryInfo skinDirectory, string screenshotsFolder)
    {
        string skinName = skinDirectory.Name.Split("_by_").First();
        string skinType = skinDirectory!.Parent!.Parent!.Name;
        string skinSubType = skinDirectory.Parent.Name;
        string author = skinDirectory.Name.Contains("_by_",StringComparison.OrdinalIgnoreCase) ? skinDirectory.Name.Split("_by_").Last() : string.Empty;
        return new LocalSkin(skinName, skinType, skinSubType, skinDirectory.FullName, author,
            [..GetScreenShots(screenshotsFolder)]);
    }

    private static IEnumerable<string> GetScreenShots(string screenshotsFolder){
        if(Directory.Exists(screenshotsFolder))
        {
            return new DirectoryInfo(screenshotsFolder).GetFiles().Select(x => x.FullName);
        }
        else
        {
            return [];
        }
    }
}