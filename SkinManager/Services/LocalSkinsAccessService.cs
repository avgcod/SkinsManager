using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SkinManager.Services;

/// <summary>
/// This class provides access to files on the local file system.
/// </summary>
public class LocalSkinsAccessService(IMessenger theMessenger) : ISkinsAccessService
{
    private readonly IMessenger _theMessenger = theMessenger;

    public IEnumerable<Skin> GetAvailableSkins(string skinsFolder)
    {
        try
        {
            List<Skin> skins = [];
            if (Directory.Exists(skinsFolder))
            {
                DirectoryInfo rootDirectory = new (skinsFolder);
                foreach (DirectoryInfo skinTypeDirectory in rootDirectory.GetDirectories())
                {
                    foreach (DirectoryInfo subTypeDirectory in skinTypeDirectory.GetDirectories())
                    {
                        if (subTypeDirectory.Name != "Originals")
                        {
                            foreach (DirectoryInfo skinDirectory in subTypeDirectory.GetDirectories())
                            {
                                skins.Add(new Skin(new SkinType(subTypeDirectory.Parent?.Name ?? subTypeDirectory.Name,
                                    new List<string>()),

                                    subTypeDirectory.Name,
                                    skinDirectory.Name,
                                    skinDirectory.FullName,
                                    string.Empty,
                                    string.Empty,
                                    DateOnly.FromDateTime(skinDirectory.CreationTime),
                                    DateOnly.FromDateTime(skinDirectory.CreationTime)));
                            }
                        }
                    }
                }
            }
            return skins;
        }
        catch (Exception ex)
        {
            _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            return new List<Skin>();
        }
    }

    public async Task<IEnumerable<Skin>> GetAvailableSkinsAsync(string skinsFolder)
    {
        try
        {
            List<Skin> skins = [];
            if (await DirectoryExistsAsync(skinsFolder))
            {
                DirectoryInfo rootDirectory = new (skinsFolder);
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
                                    skins.Add(new Skin(new SkinType(subTypeDirectory.Parent?.Name ?? subTypeDirectory.Name,
                                        new List<string>()),

                                        subTypeDirectory.Name,
                                        skinDirectory.Name,
                                        skinDirectory.FullName,
                                        string.Empty,
                                        string.Empty,
                                        DateOnly.FromDateTime(skinDirectory.CreationTime),
                                        DateOnly.FromDateTime(skinDirectory.CreationTime)));
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

    public IEnumerable<string> GetSkinScreenshots(Skin currentSkin)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetSkinScreenshotsAsync(Skin currentSkin)
    {
        throw new NotImplementedException();
    }
}
