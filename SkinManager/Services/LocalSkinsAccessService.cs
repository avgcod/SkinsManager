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
public class LocalSkinsAccessService : ISkinsAccessService
{
    private readonly IMessenger _theMessenger;

    public LocalSkinsAccessService(IMessenger theMessenger)
    {
        _theMessenger = theMessenger;
    }

    /// <summary>
    /// Gets a list of available skins.
    /// </summary>
    /// <param name="skinsFolder">Location of skins.</param>
    /// <returns>List of available skins.</returns>
    public IEnumerable<Skin> GetAvailableSkins(string skinsFolder)
    {
        try
        {
            List<Skin> skins = new List<Skin>();
            if (Directory.Exists(skinsFolder))
            {
                DirectoryInfo rootDirectory = new DirectoryInfo(skinsFolder);
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
                                    skinDirectory.CreationTime,
                                    skinDirectory.CreationTime));
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

    /// <summary>
    /// Gets a list of available skins.
    /// </summary>
    /// <param name="skinsFolder">Location of skins.</param>
    /// <returns>List of available skins.</returns>
    public async Task<IEnumerable<Skin>> GetAvailableSkinsAsync(string skinsFolder)
    {
        try
        {
            List<Skin> skins = new List<Skin>();
            if (await DirectoryExistsAsync(skinsFolder))
            {
                DirectoryInfo rootDirectory = new DirectoryInfo(skinsFolder);
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
                                        skinDirectory.CreationTime,
                                        skinDirectory.CreationTime));
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

    /// <summary>
    /// Gets screenshots for a skin.
    /// </summary>
    /// <param name="currentSkin">the skins to get the screenshots for.</param>
    /// <returns>Collection of screenshot locations</returns>
    public IEnumerable<string> GetSkinScreenshots(Skin currentSkin)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentSkin"></param>
    /// <returns>Collection of screenshot locations.</returns>
    public Task<IEnumerable<string>> GetSkinScreenshotsAsync(Skin currentSkin)
    {
        throw new NotImplementedException();
    }
}
