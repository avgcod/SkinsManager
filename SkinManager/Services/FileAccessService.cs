using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace SkinManager.Services
{
    /// <summary>
    /// This class facilitates local file system access.
    /// </summary>
    public static class FileAccessService
    {
        private static IMessenger _theMessenger = default!;

        public static void Initialize(IMessenger messenger)
        {
            _theMessenger = messenger;
        }
        public static async Task<bool> ApplySkinAsync(string skinDirectoryName, string gameDirectoryName)
        {
            try
            {
                await Task.Run(() =>
                {
                    foreach (DirectoryInfo folder in new DirectoryInfo(skinDirectoryName).GetDirectories("*.*",
                                 SearchOption.AllDirectories))
                    {
                        foreach (FileInfo theFile in folder.GetFiles())
                        {
                            string destinationPath = Path.Combine(gameDirectoryName, folder.Name, theFile.Name);
                            File.Copy(theFile.FullName, destinationPath, true);
                        }
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return false;
            }
        }
        private static async Task<bool> CreateFolderAsync(string directoryName)
        {
            if (!await DirectoryExistsAsync(directoryName))
            {
                try
                {
                    await Task.Run(() => Directory.CreateDirectory(directoryName));
                }
                catch (Exception ex)
                {
                    _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                    return false;
                }
            }

            return true;
        }
        public static async Task<bool> CreateStructureAsync(IEnumerable<SkinType> skinTypes, string skinsFolderName)
        {
            string path = string.Empty;
            try
            {
                await Task.Run(async () =>
                {
                    foreach (SkinType currentType in skinTypes)
                    {
                        path = Path.Combine(skinsFolderName, currentType.Name);
                        await CreateFolderAsync(path);

                        foreach (string subType in currentType.SubTypes)
                        {
                            path = Path.Combine(skinsFolderName, currentType.Name, subType);
                            await CreateFolderAsync(path);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _theMessenger.Send(new FatalErrorMessage(ex.GetType().Name, ex.Message));
                return false;
            }


            return true;
        }
        public static async Task<bool> CreateBackUpAsync(string skinDirectoryName, string backUpDirectoryName,
            string gameDirectoryName)
        {
            try
            {
                DirectoryInfo skinDirectory = new(skinDirectoryName);
                DirectoryInfo[] subDirs =
                    await Task.Run(() => skinDirectory.GetDirectories("*.*", SearchOption.AllDirectories));

                await Task.Run(async () =>
                {
                    foreach (DirectoryInfo currentFolder in subDirs)
                    {
                        string newBackUpDirectoryName = Path.Combine(backUpDirectoryName, currentFolder.Name);
                        if (await CreateFolderAsync(newBackUpDirectoryName))
                        {
                            foreach (FileInfo theFile in currentFolder.GetFiles())
                            {
                                string backupFileName = Path.Combine(newBackUpDirectoryName, theFile.Name);
                                string originalFileName =
                                    Path.Combine(gameDirectoryName, currentFolder.Name, theFile.Name);
                                if (File.Exists(originalFileName))
                                {
                                    File.Copy(originalFileName, backupFileName, false);
                                }
                                else
                                {
                                    string filesToDelete = "FilesToDelete.txt";
                                    FileStream originalFileStream;
                                    if (await FileExistsAsync(filesToDelete))
                                    {
                                        originalFileStream = File.OpenWrite(originalFileName);
                                    }
                                    else
                                    {
                                        originalFileStream = File.Create(backupFileName);
                                    }

                                    await using StreamWriter writer = new StreamWriter(originalFileStream);
                                    await writer.WriteLineAsync(originalFileName);
                                    writer.Close();
                                }
                            }
                        }
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return false;
            }
        }
        public static async Task<bool> RestoreBackupAsync(string backUpDirectoryName, string gameDirectoryName)
        {
            try
            {
                DirectoryInfo backupDirectory = new(backUpDirectoryName);
                IEnumerable<DirectoryInfo> subDirs = await Task.Run(() => backupDirectory
                    .GetDirectories("*.*", SearchOption.AllDirectories));

                await Task.Run(async () =>
                {
                    foreach (DirectoryInfo currentFolder in subDirs)
                    {
                        foreach (FileInfo theFile in currentFolder.GetFiles())
                        {
                            string gameFileName = Path.Combine(gameDirectoryName, currentFolder.Name, theFile.Name);
                            if (await FileExistsAsync(gameFileName))
                            {
                                File.Copy(theFile.FullName, gameFileName, true);
                            }
                        }
                    }

                    string filesToDelete = "FilesToDelete.txt";
                    if (await FileExistsAsync(filesToDelete))
                    {
                        using StreamReader reader = new StreamReader(filesToDelete);
                        while (!reader.EndOfStream)
                        {
                            File.Delete((await reader.ReadLineAsync())!);
                        }

                        File.Delete(filesToDelete);
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return false;
            }
        }
        private static async Task<bool> FileExistsAsync(string fileName)
        {
            try
            {
                return await Task.Run(() => File.Exists(fileName));
            }
            catch (Exception ex)
            {
                _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return false;
            }
        }
        private static async Task<bool> DirectoryExistsAsync(string directoryName)
        {
            try
            {
                return await Task.Run(() => Directory.Exists(directoryName));
            }
            catch (Exception ex)
            {
                _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return false;
            }
        }
        public static async Task StartGameAsync(string fileLocation)
        {
            if (await FileExistsAsync(fileLocation))
            {
                ProcessStartInfo psInfo = new()
                {
                    FileName = Path.Combine(fileLocation),
                    Verb = "runas",
                    UseShellExecute = true
                };

                try
                {
                    await Task.Run(() => Process.Start(psInfo));
                }
                catch (Exception ex)
                {
                    _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                }
            }
        }
        public static async Task<bool> ExtractSkinAsync(string archivePath, string destinationLocation)
        {
            if (await FileExistsAsync(archivePath))
            {
                try
                {
                    Directory.CreateDirectory(destinationLocation);
                    await Task.Run(() => ZipFile.ExtractToDirectory(archivePath, destinationLocation));
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public static async Task<GameInfo> LoadGameInfoAsync(string gameInfoFileName)
        {
            try
            {
                if (File.Exists(gameInfoFileName))
                {
                    await using Stream fileStream = File.OpenRead(gameInfoFileName);
                    return await JsonSerializer.DeserializeAsync<GameInfo>(fileStream) switch
                    {
                        { } gameInfo => gameInfo,
                        _ => throw new FileLoadException("The GameInfo file was found but was unable to be loaded.")
                    };
                }
                else
                {
                    throw new FileLoadException("The GameInfo file was not found.");
                }
            }
            catch (Exception ex)
            {
                _theMessenger.Send(new FatalErrorMessage(ex.GetType().Name, ex.Message));
                throw new FileLoadException(ex.Message);
            }
        }
        public static async Task<IEnumerable<Skin>> LoadCachedWebSkinsAsync(string cachedWebSkinsFileName)
        {
            try
            {
                if (File.Exists(cachedWebSkinsFileName))
                {
                    await using Stream fileStream = File.OpenRead(cachedWebSkinsFileName);
                    return await JsonSerializer.DeserializeAsync<IEnumerable<Skin>>(fileStream) switch
                    {
                        { } webSkins => [..webSkins],
                        _ => []
                    };
                }
                else
                {
                    return [];
                }
            }
            catch (Exception ex)
            {
                _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return [];
            }
        }
        public static async Task SaveGameInfoAsync(GameInfo gameInfo, string gameInfoFileName)
        {
            try
            {
                await using Stream fileStream = File.Open(gameInfoFileName, FileMode.Create);
                await JsonSerializer.SerializeAsync(fileStream, gameInfo,
                    new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }
        public static async Task SaveSkinsAsync(IEnumerable<Skin> webSkins, string skinsFileName)
        {
            try
            {
                await using Stream fileStream = File.Open(skinsFileName, FileMode.Create);
                await JsonSerializer.SerializeAsync(fileStream, webSkins,
                    new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }
        public static async Task<bool> SaveScreenshotsAsync(string path, IEnumerable<string> screenshots)
        {
            try
            {
                Directory.CreateDirectory(path);
                int screenshotNumber = 1;
                foreach (var screenshot in screenshots)
                {
                    Bitmap? screenshotBitmap = screenshot.Contains("http") switch
                    {
                        true => await ImageHelperService.LoadFromWeb(new Uri(screenshot)),
                        _ => ImageHelperService.LoadFromResource(new Uri(screenshot))
                    };

                    if (screenshotBitmap is not null)
                    {
                        screenshotBitmap.Save(Path.Combine(path, $"Screenshot {screenshotNumber}.{
                            screenshot.Split(".").Last()}"));
                    }

                    screenshotNumber++;
                }

                return true;
            }
            catch (Exception ex)
            {
                _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return false;
            }
        }
        public static async Task<bool> FolderHasFilesAsync(string folderPath)
        {
            if (await DirectoryExistsAsync(folderPath))
            {
                return new DirectoryInfo(folderPath).EnumerateFiles(".", SearchOption.AllDirectories).Any();
            }
            else
            {
                return false;
            }
        }
    }
}