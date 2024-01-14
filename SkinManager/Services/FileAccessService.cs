using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SkinManager.Services
{
    /// <summary>
    /// This class facilitates local file system access.
    /// </summary>
    public class FileAccessService(IMessenger theMessenger) : IFileAccessService
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
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
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
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
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
                    _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
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
                    _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
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
                    _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                }
            }
            else
            {
                _theMessenger.Send<DirectoryNotEmptyMessage>(new DirectoryNotEmptyMessage(skinsFolderName));
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
                    _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                }
            }
            else
            {
                _theMessenger.Send<DirectoryNotEmptyMessage>(new DirectoryNotEmptyMessage(skinsFolderName));
            }

        }
        
        /// <summary>
        /// Copies all files in a game directory, including sub directories, that are in the skin folder, and subdirectories, to the back up folder.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="skinDirectoryName">Directory of the skin.</param>
        /// <param name="backUpDirectoryName">Directory to store the backup.</param>
        /// <param name="gameDirectoryName">Directory of the game.</param>
        public void CreateBackUp(string skinDirectoryName, string backUpDirectoryName, string gameDirectoryName)
        {
            try
            {
                DirectoryInfo skinDirectory = new (skinDirectoryName);
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
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }
        /// <summary>
        /// Copies all files in a game directory, including sub directories, that are in the skin folder, and subdirectories, to the back up folder.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="skinDirectoryName">Directory of the skin.</param>
        /// <param name="backUpDirectoryName">Directory to store the backup.</param>
        /// <param name="gameDirectoryName">Directory of the game.</param>
        public async Task CreateBackUpAsync(string skinDirectoryName, string backUpDirectoryName, string gameDirectoryName)
        {
            try
            {
                DirectoryInfo skinDirectory = new (skinDirectoryName);
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
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }
        #endregion

        #region Restore Methods
        /// <summary>
        /// Restores backed up game files to the game installation location.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="skinDirectoryName">The back up location.</param>
        /// <param name="gameDirectoryName">The game installation location.</param>
        public bool RestoreBackup(string skinDirectoryName, string gameDirectoryName)
        {
            try
            {
                DirectoryInfo skinDirectory = new (skinDirectoryName);
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
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return false;
            }
        }

        /// <summary>
        /// Restores backed up game files to the game installation location.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="skinDirectoryName">The back up location.</param>
        /// <param name="gameDirectoryName">The game installation location.</param>
        public async Task<bool> RestoreBackupAsync(string skinDirectoryName, string gameDirectoryName)
        {
            try
            {
                DirectoryInfo skinDirectory = new (skinDirectoryName);
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
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return false;
            }

        }
        #endregion

        #region Get Methods

        public Dictionary<string, List<Skin>> GetCachedWebSkins(IEnumerable<string> gameNames)
        {
            Dictionary<string, List<Skin>> gameWebSkins = [];
            try
            {
                foreach (string gameName in gameNames)
                {
                    string fileName = gameName + " Skins.xml";
                    if (File.Exists(fileName))
                    {
                        using Stream fileStream = File.OpenRead(fileName);
                        XmlSerializer theSerializer = new(typeof(List<Skin>));
                        List<Skin> foundSkins = theSerializer.Deserialize(fileStream) as List<Skin> ?? [];
                        gameWebSkins.Add(gameName, foundSkins);
                    }
                }
                return gameWebSkins;
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return [];
            }
        }

        /// <summary>
        /// Gets a collection of applied skins locations.
        /// Send a OperationErrorMessage if an error occurs and returns an empty collection.
        /// </summary>
        /// <param name="appliedSkinsFile">Applied skins file.</param>
        /// <returns>Collection of found applied skins locations.</returns>
        //public IEnumerable<string> GetAppliedSkins(string appliedSkinsFile)
        //{
        //    /*
        //     * List<Skin> appliedSkins = new List<Skin>();
        //    if (File.Exists(appliedSkinsFile))
        //    {
        //        using (XmlReader theReader = XmlReader.Create(appliedSkinsFile))
        //        {
        //            Skin appliedSkin;
        //            SkinType skinType = new SkinType(string.Empty, new List<string>());
        //            string skinName = string.Empty;
        //            string skinSubType = string.Empty;

        //            while (theReader.ReadToFollowing("AppliedSkin"))
        //            {
        //                theReader.ReadToFollowing("Name");
        //                skinName = theReader.ReadElementContentAsString("Name", "");

        //                theReader.ReadToFollowing("Type");
        //                skinType = new SkinType(theReader.ReadElementContentAsString("Type", ""), new List<string>());

        //                theReader.ReadToFollowing("SubType");
        //                skinSubType = theReader.ReadElementContentAsString("SubType", "");

        //                appliedSkin = new Skin(skinType, skinSubType, skinName, string.Empty, string.Empty, string.Empty,
        //                    DateTime.Now, DateTime.Now);

        //                appliedSkins.Add(appliedSkin);
        //            }
        //        }
        //    }
        //     */
        //    try
        //    {
        //        if (File.Exists(appliedSkinsFile))
        //        {
        //            using Stream fileStream = File.OpenRead("testXML.xml");
        //            gameInfo = (List<KnownGameInfo>)theSerializer.Deserialize(fileStream) ?? [];
        //            return JsonSerializer.Deserialize<List<string>>(File.OpenRead(appliedSkinsFile)) ?? [];
        //        }
        //        else
        //        {
        //            return new List<string>();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
        //        return new List<string>();
        //    }


        //}

        /// <summary>
        /// Gets a collecttion of applied skins locations.
        /// Send a OperationErrorMessage if an error occurs and returns an empty collection.
        /// </summary>
        /// <param name="appliedSkinsFile">Applied skins file.</param>
        /// <returns>Collection of found applied skins locations.</returns>
        //public async Task<IEnumerable<string>> GetAppliedSkinsAsync(string appliedSkinsFile)
        //{
        //    try
        //    {
        //        List<string> appliedSkins = [];
        //        if (await FileExistsAsync(appliedSkinsFile))
        //        {
        //            using Stream reader = File.OpenRead(appliedSkinsFile);
        //            appliedSkins = await JsonSerializer.DeserializeAsync<List<string>>(reader) ?? [];
        //            reader.Close();
        //        }
        //        return new List<string>();
        //    }
        //    catch (Exception ex)
        //    {
        //        _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
        //        return new List<string>();
        //    }
        //}

        /// <summary>
        /// Gets a collection of SkinType.
        /// Send a OperationErrorMessage if an error occurs and returns an empty collection.
        /// </summary>
        /// <param name="subTypesFile">Skin types file.</param>
        /// <returns>Collection of SkinType.</returns>
        //public IEnumerable<SkinType> GetSkinTypes(string skinTypesFile)
        //{
        //    /*
        //     * List<SkinType> skinTypes = new List<SkinType>();

        //    if (File.Exists(subTypesFile))
        //    {
        //        using (XmlReader theReader = XmlReader.Create(subTypesFile))
        //        {
        //            SkinType skinType;
        //            List<string> subTypes;
        //            string skinTypeName;

        //            while (theReader.ReadToFollowing("SkinType"))
        //            {
        //                theReader.ReadToFollowing("Name");
        //                skinTypeName = theReader.ReadElementContentAsString("Name", "");

        //                subTypes = new List<string>();
        //                theReader.ReadToFollowing("SubTypeName");
        //                do
        //                {
        //                    subTypes.Add(theReader.ReadElementContentAsString("SubTypeName", ""));

        //                } while (theReader.ReadToNextSibling("SubTypeName"));

        //                skinType = new SkinType(skinTypeName, subTypes);

        //                skinTypes.Add(skinType);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        skinTypes = PopulateSkins().ToList();
        //    }

        //    return skinTypes;
        //     */
        //    try
        //    {
        //        if (File.Exists(skinTypesFile))
        //        {
        //            return JsonSerializer.Deserialize<List<SkinType>>(File.OpenRead(skinTypesFile)) ?? [];
        //        }
        //        else
        //        {
        //            return new List<SkinType>();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
        //        return new List<SkinType>();
        //    }


        //}

        /// <summary>
        /// Gets a collection of SkinType.
        /// Send a OperationErrorMessage if an error occurs and returns an empty collection.
        /// </summary>
        /// <param name="subTypesFile">Skin types file.</param>
        /// <returns>Collection of skin types.</returns>
        //public async Task<IEnumerable<SkinType>> GetSkinTypesAsync(string subTypesFile)
        //{
        //    try
        //    {
        //        List<SkinType> skinTypes = [];
        //        if (await FileExistsAsync(subTypesFile))
        //        {
        //            using Stream reader = File.OpenRead(subTypesFile);
        //            skinTypes = await JsonSerializer.DeserializeAsync<List<SkinType>>(reader) ?? [];
        //            reader.Close();
        //        }
        //        return skinTypes;
        //    }
        //    catch (Exception ex)
        //    {
        //        _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
        //        return new List<SkinType>();
        //    }
        //}
        #endregion

        #region Save Methods
        /// <summary>
        /// Saves applied skins to a JSON file.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="appliedSkins">Collection of applied skins locations.</param>
        /// <param name="appliedSkinsFileName">File to save to.</param>
        //public void SaveAppliedSkins(IEnumerable<string> appliedSkins, string appliedSkinsFileName)
        //{
        //    /*
        //     * try
        //    {
        //        XmlWriterSettings settings = new XmlWriterSettings();
        //        settings.Indent = true;
        //        using (XmlWriter writer = XmlWriter.Create(appliedSkinsFileName, settings))
        //        {
        //            writer.WriteStartElement("AppliedSkins");

        //            foreach (Skin theSkin in appliedSkins)
        //            {
        //                writer.WriteStartElement("AppliedSkin");

        //                writer.WriteStartElement("Name");
        //                writer.WriteString(theSkin.Name);
        //                writer.WriteEndElement();

        //                writer.WriteStartElement("Type");
        //                writer.WriteString(theSkin.SkinType.Name);
        //                writer.WriteEndElement();

        //                writer.WriteStartElement("SubType");
        //                writer.WriteString(theSkin.SubType);
        //                writer.WriteEndElement();

        //                writer.WriteEndElement();
        //            }
        //            writer.WriteEndElement();
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //     */

        //    try
        //    {
        //        JsonSerializer.Serialize(File.OpenWrite(appliedSkinsFileName), appliedSkins, options);
        //    }
        //    catch (Exception ex)
        //    {
        //        _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
        //    }

        //}

        /// <summary>
        /// Saves applied skins to a JSON file.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="appliedSkins">Applied skins locations.</param>
        /// <param name="appliedSkinsFileName">File to save to.</param>
        //public async Task SaveAppliedSkinsAsync(IEnumerable<string> appliedSkins, string appliedSkinsFileName)
        //{
        //    try
        //    {
        //        using Stream writer = File.OpenWrite(appliedSkinsFileName);
        //        await JsonSerializer.SerializeAsync(writer, appliedSkins, options);
        //        writer.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
        //    }
        //}

        /// <summary>
        /// Saves a collection SkinTypes to a JSON file.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="skinTypes">Collection of skin types.</param>
        /// <param name="skinTypesFile">File to save to.</param>
        //public void SaveSkinTypes(IEnumerable<SkinType> skinTypes, string skinTypesFile)
        //{
        //    /*
        //     * try
        //    {
        //        XmlWriterSettings settings = new XmlWriterSettings();
        //        settings.Indent = true;
        //        using (XmlWriter writer = XmlWriter.Create(skinTypesFile, settings))
        //        {
        //            writer.WriteStartElement("SkinTypes");

        //            foreach (SkinType skinType in skinTypes)
        //            {
        //                writer.WriteStartElement("SkinType");

        //                writer.WriteStartElement("Name");
        //                writer.WriteString(skinType.Name);
        //                writer.WriteEndElement();

        //                writer.WriteStartElement("SubTypes");
        //                foreach (string subType in skinType.SubTypes)
        //                {
        //                    writer.WriteStartElement("SubTypeName");
        //                    writer.WriteString(subType);
        //                    writer.WriteEndElement();
        //                }
        //                writer.WriteEndElement();

        //                writer.WriteEndElement();
        //            }

        //            writer.WriteEndElement();

        //            writer.Close();
        //            return true;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //     */

        //    try
        //    {
        //        JsonSerializer.Serialize(File.OpenWrite(skinTypesFile), skinTypes, options);
        //    }
        //    catch (Exception ex)
        //    {
        //        _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
        //    }

        //}

        /// <summary>
        /// Saves a collection of SkinType to a JSON file.
        /// Send a OperationErrorMessage if an error occurs.
        /// </summary>
        /// <param name="skinTypes">Collection of SkinType.</param>
        /// <param name="skinTypesFile">File to save to.</param>
        //public async Task SaveSkinTypesAsync(IEnumerable<SkinType> skinTypes, string skinTypesFile)
        //{
        //    try
        //    {
        //        using Stream writer = File.OpenWrite(skinTypesFile);
        //        await JsonSerializer.SerializeAsync(writer, skinTypes, options);
        //        writer.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
        //    }
        //}
        #endregion

        #region Exists Methods
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
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
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
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return false;
            }
        }
        #endregion

        #region Start Game Methods
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
                    _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
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
                    _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                }
            }
        }
        #endregion

        #region IsEmpty Methods
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
        #endregion
    }
}
