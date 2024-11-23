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

        /// <summary>
        /// Copies all files in a skin folder, including subfolders, to the game folder.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="skinDirectoryName">Directory the skin is located in.</param>
        /// <param name="gameDirectoryName">Directory the game is located in.</param>
        public static async Task<bool> ApplySkinAsync(string skinDirectoryName, string gameDirectoryName)
        {
            try
            {
                await Task.Run(() =>
                {
                    foreach (DirectoryInfo folder in new DirectoryInfo(skinDirectoryName).GetDirectories("*.*", SearchOption.AllDirectories))
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

        /// <summary>
        /// Creates a folder.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="directoryName">Directory to create.</param>
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

        /// <summary>
        /// Creates folder structure based on a collection of SkinType.
        /// Send a OperationErrorMessage if an error occurs.
        /// Send a DirectoryNotEmptyMessage if the folder is not empty.
        /// </summary>
        /// <param name="skinTypes">Collection of SkinType.</param>
        /// <param name="skinsFolderName">Directory for the structure to be created in.</param>
        public static async Task CreateStructureAsync(IEnumerable<SkinType> skinTypes, string skinsFolderName)
        {
            if (await IsEmptyDirectoryAsync(skinsFolderName))
            {
                try
                {
                    await Task.Run(async () =>
                    {
                        foreach (SkinType currentType in skinTypes)
                        {
                            await CreateFolderAsync(Path.Combine(skinsFolderName, currentType.Name));

                            foreach (string subType in currentType.SubTypes)
                            {
                                await CreateFolderAsync(Path.Combine(skinsFolderName, currentType.Name, subType));
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                }
            }
            else
            {
                _theMessenger.Send(new DirectoryNotEmptyMessage(skinsFolderName));
            }
        }
        
        /// <summary>
        /// Copies all files in a game directory, including subdirectories, that are in the skin folder, and subdirectories, to the back-up folder.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="skinDirectoryName">Directory of the skin.</param>
        /// <param name="backUpDirectoryName">Directory to store the backup.</param>
        /// <param name="gameDirectoryName">Directory of the game.</param>
        public static async Task<bool> CreateBackUpAsync(string skinDirectoryName, string backUpDirectoryName, string gameDirectoryName)
        {
            try
            {
                DirectoryInfo skinDirectory = new(skinDirectoryName);
                DirectoryInfo[] subDirs = await Task.Run(() => skinDirectory.GetDirectories("*.*", SearchOption.AllDirectories));

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
                                string originalFileName = Path.Combine(gameDirectoryName,currentFolder.Name, theFile.Name);
                                if (File.Exists(originalFileName))
                                {
                                    File.Copy(originalFileName, backupFileName, false);
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

        /// <summary>
        /// Restores backed up game files to the game installation location.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="backUpDirectoryName">The back-up location.</param>
        /// <param name="gameDirectoryName">The game installation location.</param>
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
                });

                return true;
            }
            catch (Exception ex)
            {
                _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return false;
            }
        }

        /// <summary>
        /// Checks if a file exists.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <returns>If the file exists.</returns>
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

        /// <summary>
        /// Checks if a directory exists.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="directoryName">Directory name.</param>
        /// <returns>If the directory exists.</returns>
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

        /// <summary>
        /// Starts the game.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="fileLocation">Game executable location.</param>
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

        /// <summary>
        /// Checks if a directory is empty.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns>if the directory is empty.</returns>
        private static async Task<bool> IsEmptyDirectoryAsync(string directoryPath)
        {
            return await Task.Run(() => !Directory.EnumerateFileSystemEntries(directoryPath).Any());
        }

        /// <summary>
        /// Loads a collection of GameInfo from a file.
        /// </summary>
        /// <param name="gameInfoFileName">File to load.</param>
        /// <returns>Collection of GameInfo.</returns>
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
                        _ =>[]
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
        
        /// <summary>
        /// Saves a collection of GameInfo to a file.
        /// </summary>
        /// <param name="gameInfo">Collection of GameInfo to save.</param>
        /// <param name="gameInfoFileName">File to save to.</param>
        public static async Task SaveGameInfoAsync(GameInfo gameInfo, string gameInfoFileName)
        {
            try
            {
                await using Stream fileStream = File.Open(gameInfoFileName, FileMode.Create);
                await JsonSerializer.SerializeAsync(fileStream,gameInfo, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }

        public static async Task SaveWebSkinsAsync(IEnumerable<Skin> webSkins, string webSkinsFileName)
        {
            try
            {
                await using Stream fileStream = File.Open(webSkinsFileName, FileMode.Create);
                await JsonSerializer.SerializeAsync(fileStream,webSkins, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }

        public static async Task SaveScreenshotsAsync(string path, IEnumerable<string> screenshots)
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
            }
            catch (Exception ex)
            {
                _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
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
