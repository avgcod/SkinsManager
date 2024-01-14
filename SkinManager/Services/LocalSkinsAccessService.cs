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
    private readonly IMessenger _theMessenger = theMessenger;

    public async Task<IEnumerable<Skin>> GetAvailableSkinsAsync(string skinsFolder)
    {
        try
        {
            List<Skin> skins = [];
            if (await DirectoryExistsAsync(skinsFolder))
            {
                DirectoryInfo rootDirectory = new(skinsFolder);
                await Task.Run(() =>
                {
                    foreach (DirectoryInfo skinTypeDirectory in rootDirectory.GetDirectories())
                    {
                        foreach (DirectoryInfo subTypeDirectory in skinTypeDirectory.GetDirectories())
                        {
                            if (subTypeDirectory.Name != "Originals")
                            {
                                foreach (DirectoryInfo skinDirectory in subTypeDirectory.GetDirectories())
                                {
                                    Skin tempSkin = new()
                                    {
                                        Name = skinDirectory.Name,
                                        CreationDate = DateOnly.FromDateTime(skinDirectory.CreationTime),
                                        LastUpdatedDate = DateOnly.FromDateTime(skinDirectory.LastWriteTime),
                                        Location = skinDirectory.FullName,
                                        SkinType = new SkinType() { Name = subTypeDirectory.Parent?.Name ?? skinTypeDirectory.Name },
                                        SubType = subTypeDirectory.Name
                                    };

                                    if (skinDirectory.Name.Contains("_by_"))
                                    {
                                        tempSkin.Name = skinDirectory.Name.Split("_by_")[1];
                                        int authorStartIndex = skinDirectory.Name.IndexOf("_by_");
                                        tempSkin.Description = $"{skinDirectory.Name} {subTypeDirectory.Name} skin {skinDirectory.Name.Remove(authorStartIndex)}.";
                                    }
                                    else
                                    {
                                        tempSkin.Description = $"{skinDirectory.Name} {subTypeDirectory.Name} skin {skinDirectory.Name}.";
                                    }

                                    string screenshotsFolder = Path.Combine(subTypeDirectory.FullName, "Screenshots");
                                    if (Directory.Exists(screenshotsFolder))
                                    {
                                        tempSkin.Screenshots.AddRange(new DirectoryInfo(screenshotsFolder).GetFiles().Select(x => x.FullName));
                                    }

                                    skins.Add(tempSkin);
                                }
                            }
                        }
                    }
                });
            }
            return skins;
        }
        catch (Exception ex)
        {
            _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            return new List<Skin>();
        }
    }

    /// <summary>
    /// Checks if a directory exists.
    /// </summary>
    /// <param name="directoryName">Directory name.</param>
    /// <returns>If the directory exists.</returns>
    private async Task<bool> DirectoryExistsAsync(string directoryName)
    {
        try
        {
            return await Task.Run(() => Directory.Exists(directoryName));
        }
        catch (Exception ex)
        {
            _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            return false;
        }
    }
}
