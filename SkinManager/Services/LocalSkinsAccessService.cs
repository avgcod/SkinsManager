using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SkinManager.Services;

/// <summary>
/// This class provides access to files on the local file system.
/// </summary>
public class LocalSkinsAccessService(IMessenger theMessenger)
{
    
    public async Task<IEnumerable<Skin>> GetAvailableSkinsAsync(string skinsFolder, IEnumerable<SkinType> skinTypes)
    {
        try
        {
            if (await DirectoryExistsAsync(skinsFolder))
            {
                return await GetSkins(skinsFolder);
            }
            else
            {
                return [];
            }
        }
        catch (Exception ex)
        {
            theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            return [];
        }
    }

    private async Task<IEnumerable<Skin>> GetSkins(string skinsFolder)
    {
        List<Skin> skins = [];
        DirectoryInfo rootDirectory = new(skinsFolder);

        foreach (DirectoryInfo skinTypeDirectory in rootDirectory.GetDirectories())
        {
            foreach (DirectoryInfo subTypeDirectory in skinTypeDirectory.GetDirectories()
                         .Where(x => x.Name != "Originals"))
            {
                foreach (DirectoryInfo skinDirectory in subTypeDirectory.GetDirectories())
                {
                    string screenshotsFolder = Path.Combine(subTypeDirectory.FullName, "Screenshots");
                    
                    skins.Add(await CreateTempSkin(skinDirectory, subTypeDirectory,
                        screenshotsFolder, skinTypeDirectory.Name, subTypeDirectory.Name));
                }
            }
        }

        return skins;
    }

    private async Task<Skin> CreateTempSkin(DirectoryInfo skinDirectory, DirectoryInfo subTypeDirectory, string screenshotsFolder, string skinType, string subType)
    {
        List<string> screenshots = [..await GetScreenShots(screenshotsFolder)];
        
        (string Name, string Description, string Author) skinInfo =
            GetSkinNameAndDescription(skinDirectory.Name, subTypeDirectory.Name);

        DateOnly creationDate = DateOnly.FromDateTime(skinDirectory.CreationTime);
        DateOnly lastUpdatedDate = DateOnly.FromDateTime(skinDirectory.LastWriteTime);

        return Skin.Create(skinType, subType, skinInfo.Name, [skinDirectory.FullName], skinInfo.Author,
            skinInfo.Description, creationDate, lastUpdatedDate, screenshots);
    }

    private async Task<IEnumerable<string>> GetScreenShots(string screenshotsFolder)
    {
        if (await DirectoryExistsAsync(screenshotsFolder))
        {
            return new DirectoryInfo(screenshotsFolder).GetFiles().Select(x => x.FullName);
        }
        else
        {
            return [];
        }
    }

    private static (string name, string description, string author) GetSkinNameAndDescription(string skinDirectoryName, string subTypeDirectoryName)
    {
        if (skinDirectoryName.Contains("_by_", StringComparison.Ordinal))
        {
            int authorStartIndex = skinDirectoryName.IndexOf("_by_", StringComparison.Ordinal);
            
            string name = skinDirectoryName.Split("_by_")[1];
            string description =
                $"{skinDirectoryName} {subTypeDirectoryName} skin {skinDirectoryName.Remove(authorStartIndex)}.";
            string author = skinDirectoryName.Split("_by_").Last();
            
            return (name, description, author);
        }
        else
        {
            string description = $"{skinDirectoryName} {subTypeDirectoryName} skin {skinDirectoryName}.";
            return (skinDirectoryName, description, "Unknown");
        }
    }
    
    private async Task<bool> DirectoryExistsAsync(string directoryName)
    {
        try
        {
            return await Task.Run(() => Directory.Exists(directoryName));
        }
        catch (Exception ex)
        {
            theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            return false;
        }
    }
}