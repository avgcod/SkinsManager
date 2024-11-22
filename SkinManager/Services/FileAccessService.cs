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
    public class FileAccessService(IMessenger theMessenger)
    {
        private readonly IMessenger _theMessenger = theMessenger;

        /// <summary>
        /// Copies all files in a skin folder, including subfolders, to the game folder.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="skinDirectoryName">Directory the skin is located in.</param>
        /// <param name="gameDirectoryName">Directory the game is located in.</param>
        public void ApplySkin(string skinDirectoryName, string gameDirectoryName)
        {
            try
            {
                foreach (DirectoryInfo folder in new DirectoryInfo(skinDirectoryName).GetDirectories("*.*", SearchOption.AllDirectories))
                {
                    foreach (FileInfo theFile in folder.GetFiles())
                    {
                        File.Copy(theFile.FullName, Path.Combine(gameDirectoryName, folder.FullName.Replace(skinDirectoryName, string.Empty),
                            theFile.Name), true);
                    }
                }
            }
            catch (Exception ex)
            {
                _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }

        /// <summary>
        /// Copies all files in a skin folder, including subfolders, to the game folder.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="skinDirectoryName">Directory the skin is located in.</param>
        /// <param name="gameDirectoryName">Directory the game is located in.</param>
        public async Task ApplySkinAsync(string skinDirectoryName, string gameDirectoryName)
        {
            try
            {
                await Task.Run(() =>
                {
                    foreach (DirectoryInfo folder in new DirectoryInfo(skinDirectoryName).GetDirectories("*.*", SearchOption.AllDirectories))
                    {
                        foreach (FileInfo theFile in folder.GetFiles())
                        {
                            File.Copy(theFile.FullName, Path.Combine(gameDirectoryName, folder.FullName.Replace(skinDirectoryName, string.Empty),
                                theFile.Name), true);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }

        #region Create Methods
        /// <summary>
        /// Creates a folder.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="directoryName">Directory to create.</param>
        private void CreateFolder(string directoryName)
        {
            if (!Directory.Exists(directoryName))
            {
                try
                {
                    Directory.CreateDirectory(directoryName);
                }
                catch (Exception ex)
                {
                    _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                }
            }
        }

        /// <summary>
        /// Creates a folder.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="directoryName">Directory to create.</param>
        private async Task CreateFolderAsync(string directoryName)
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
                }
            }
        }

        /// <summary>
        /// Creates folder structure based on a collection of SkinType.
        /// Send a OperationErrorMessage if an error occurs.
        /// Send a DirectoryNotEmptyMessage if the folder is not empty.
        /// </summary>
        /// <param name="skinTypes">Collection of SkinType.</param>
        /// <param name="skinsFolderName">Directory for the structure to be created in.</param>
        public void CreateStructure(IEnumerable<SkinType> skinTypes, string skinsFolderName)
        {
            if (IsEmptyDirectory(skinsFolderName))
            {
                try
                {
                    foreach (SkinType currentType in skinTypes)
                    {
                        CreateFolder(Path.Combine(skinsFolderName, currentType.Name));

                        foreach (string subType in currentType.SubTypes)
                        {
                            CreateFolder(Path.Combine(skinsFolderName, currentType.Name, subType));
                        }
                    }
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
        /// Creates folder structure based on a collection of SkinType.
        /// Send a OperationErrorMessage if an error occurs.
        /// Send a DirectoryNotEmptyMessage if the folder is not empty.
        /// </summary>
        /// <param name="skinTypes">Collection of SkinType.</param>
        /// <param name="skinsFolderName">Directory for the structure to be created in.</param>
        public async Task CreateStructureAsync(IEnumerable<SkinType> skinTypes, string skinsFolderName)
        {
            if (await IsEmptyDirectoryAsync(skinsFolderName))
            {
                try
                {
                    await Task.Run(() =>
                    {
                        foreach (SkinType currentType in skinTypes)
                        {
                            CreateFolder(Path.Combine(skinsFolderName, currentType.Name));

                            foreach (string subType in currentType.SubTypes)
                            {
                                CreateFolder(Path.Combine(skinsFolderName, currentType.Name, subType));
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
        public void CreateBackUp(string skinDirectoryName, string backUpDirectoryName, string gameDirectoryName)
        {
            try
            {
                DirectoryInfo skinDirectory = new(skinDirectoryName);
                DirectoryInfo[] subDirs = new DirectoryInfo(skinDirectoryName).GetDirectories("*.*", SearchOption.AllDirectories);

                foreach (DirectoryInfo currentFolder in subDirs)
                {
                    CreateFolder(backUpDirectoryName + currentFolder.FullName.Replace(skinDirectory.FullName, string.Empty));
                    foreach (FileInfo theFile in currentFolder.GetFiles())
                    {
                        File.Copy(gameDirectoryName + currentFolder.FullName.Replace(skinDirectory.FullName, string.Empty) + Path.DirectorySeparatorChar + theFile.Name,
                            backUpDirectoryName + currentFolder.FullName.Replace(skinDirectory.FullName, string.Empty) + Path.DirectorySeparatorChar + theFile.Name, false);
                    }
                }
            }
            catch (Exception ex)
            {
                _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }
        
        /// <summary>
        /// Copies all files in a game directory, including subdirectories, that are in the skin folder, and subdirectories, to the back-up folder.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="skinDirectoryName">Directory of the skin.</param>
        /// <param name="backUpDirectoryName">Directory to store the backup.</param>
        /// <param name="gameDirectoryName">Directory of the game.</param>
        public async Task CreateBackUpAsync(string skinDirectoryName, string backUpDirectoryName, string gameDirectoryName)
        {
            try
            {
                DirectoryInfo skinDirectory = new(skinDirectoryName);
                DirectoryInfo[] subDirs = await Task.Run(() => skinDirectory.GetDirectories("*.*", SearchOption.AllDirectories));

                await Task.Run(async () =>
                {
                    foreach (DirectoryInfo currentFolder in subDirs)
                    {
                        await CreateFolderAsync(backUpDirectoryName + currentFolder.FullName.Replace(skinDirectory.FullName, string.Empty));
                        foreach (FileInfo theFile in currentFolder.GetFiles())
                        {
                            File.Copy(gameDirectoryName + currentFolder.FullName.Replace(skinDirectory.FullName, string.Empty) + Path.DirectorySeparatorChar + theFile.Name,
                                    backUpDirectoryName + currentFolder.FullName.Replace(skinDirectory.FullName, string.Empty) + Path.DirectorySeparatorChar + theFile.Name, false);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }
        #endregion

        /// <summary>
        /// Restores backed up game files to the game installation location.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="skinDirectoryName">The back-up location.</param>
        /// <param name="gameDirectoryName">The game installation location.</param>
        public bool RestoreBackup(string skinDirectoryName, string gameDirectoryName)
        {
            try
            {
                DirectoryInfo skinDirectory = new(skinDirectoryName);
                DirectoryInfo[] subDirs = new DirectoryInfo(skinDirectoryName).GetDirectories("*.*", SearchOption.AllDirectories);

                foreach (DirectoryInfo currentFolder in subDirs)
                {
                    foreach (FileInfo theFile in currentFolder.GetFiles())
                    {
                        string gameFileName = skinDirectoryName + currentFolder.FullName.Replace(skinDirectory.FullName, string.Empty) +
                            Path.DirectorySeparatorChar + theFile.Name;
                        if (File.Exists(gameFileName))
                        {
                            File.Copy(skinDirectoryName + currentFolder.FullName.Replace(skinDirectory.FullName, string.Empty) + Path.DirectorySeparatorChar + theFile.Name,
                                gameFileName, true);
                        }
                    }
                }

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
        /// <param name="skinDirectoryName">The back-up location.</param>
        /// <param name="gameDirectoryName">The game installation location.</param>
        public async Task<bool> RestoreBackupAsync(string skinDirectoryName, string gameDirectoryName)
        {
            try
            {
                DirectoryInfo skinDirectory = new(skinDirectoryName);
                DirectoryInfo[] subDirs = await Task.Run(() => skinDirectory.GetDirectories("*.*", SearchOption.AllDirectories));

                await Task.Run(async () =>
                {
                    foreach (DirectoryInfo currentFolder in subDirs)
                    {
                        foreach (FileInfo theFile in currentFolder.GetFiles())
                        {
                            string gameFileName = gameDirectoryName + currentFolder.FullName.Replace(skinDirectory.FullName, string.Empty) +
                            Path.DirectorySeparatorChar + theFile.Name;
                            if (await FileExistsAsync(gameFileName))
                            {
                                File.Copy(skinDirectoryName + currentFolder.FullName.Replace(skinDirectory.FullName, string.Empty) + Path.DirectorySeparatorChar + theFile.Name,
                                    gameFileName, true);
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
        private async Task<bool> FileExistsAsync(string fileName)
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
        private async Task<bool> DirectoryExistsAsync(string directoryName)
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
        public void StartGame(string fileLocation)
        {
            if (File.Exists(fileLocation))
            {
                ProcessStartInfo psInfo = new()
                {
                    FileName = Path.Combine(fileLocation),
                    Verb = "runas",
                    UseShellExecute = true
                };

                try
                {
                    Process.Start(psInfo);
                }
                catch (Exception ex)
                {
                    _theMessenger.Send(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                }
            }
        }

        /// <summary>
        /// Starts the game.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="fileLocation">Game executable location.</param>
        public async Task StartGameAsync(string fileLocation)
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

        public async Task<bool> ExtractSkin(string archivePath, string destinationLocation)
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
        private static bool IsEmptyDirectory(string directoryPath)
        {
            return !Directory.EnumerateFileSystemEntries(directoryPath).Any();
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
        public async Task<GameInfo> LoadGameInfo(string gameInfoFileName)
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
        
        public async Task<IEnumerable<Skin>> LoadWebSkins(string webSkinsFileName)
        {
            try
            {
                if (File.Exists(webSkinsFileName))
                {
                    await using Stream fileStream = File.OpenRead(webSkinsFileName);
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
        public async Task SaveGameInfo(GameInfo gameInfo, string gameInfoFileName)
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

        public async Task SaveWebSkins(IEnumerable<Skin> webSkins, string webSkinsFileName)
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

        public async Task SaveScreenshot(string path, IEnumerable<string> screenshots)
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
    }
}
