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

namespace SkinManager.Services
{
    public class FileAccessService : IFileAccessService
    {
        private readonly IMessenger _theMessenger;

        public FileAccessService(IMessenger theMessenger)
        {
            _theMessenger = theMessenger;
        }

        /// <summary>
        /// Copies all files in a skin folder, including subfolders, to the game folder.
        /// </summary>
        /// <param name="skinDirectoryName">Directory the skin is located in.</param>
        /// <param name="gameDirectoryName">Directory the game is located in.</param>
        /// <param name="_theMessenger">_theMessenger to use if there is an error.</param>
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
        /// </summary>
        /// <param name="skinDirectoryName">Directory the skin is located in.</param>
        /// <param name="gameDirectoryName">Directory the game is located in.</param>
        /// <param name="_theMessenger">_theMessenger to use if there is an error.</param>
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
        /// </summary>
        /// <param name="directoryName">Directory to create.</param>
        /// <param name="_theMessenger">_theMessenger to use if there is an error.</param>
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
        /// </summary>
        /// <param name="directoryName">Directory to create.</param>
        /// <param name="_theMessenger">_theMessenger to use if there is an error.</param>
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
        /// </summary>
        /// <param name="skinTypes">Collection of SkinType.</param>
        /// <param name="skinsFolderName">Directory for the structure to be created in.</param>
        public void CreateStructure(IEnumerable<SkinType> skinTypes, string skinsFolderName)
        {
            if (IsEmptyDirectory(skinsFolderName))
            {
                if (!skinTypes.Any())
                {
                    skinTypes = PopulateDefaultStructure();
                }
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
        /// </summary>
        /// <param name="skinTypes">Collection of SkinType.</param>
        /// <param name="skinsFolderName">Directory for the structure to be created in.</param>
        public async Task CreateStructureAsync(IEnumerable<SkinType> skinTypes, string skinsFolderName)
        {
            if (await IsEmptyDirectoryAsync(skinsFolderName))
            {
                if (!skinTypes.Any())
                {
                    skinTypes = PopulateDefaultStructure();
                }
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
        /// Copies all files in a game directory, including sub directories, that are in the skin folder, and subdirectories, tp the back up folder.
        /// </summary>
        /// <param name="skinDirectoryName">Directory of the skin.</param>
        /// <param name="backUpDirectoryName">Directory to store the backup.</param>
        /// <param name="gameDirectoryName">Directory of the game.</param>
        /// <param name="_theMessenger">_theMessenger to use if there is an error.</param>
        public void CreateBackUp(string skinDirectoryName, string backUpDirectoryName, string gameDirectoryName)
        {
            try
            {
                DirectoryInfo skinDirectory = new DirectoryInfo(skinDirectoryName);
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
        /// Copies all files in a game directory, including sub directories, that are in the skin folder, and subdirectories, tp the back up folder.
        /// </summary>
        /// <param name="skinDirectoryName">Directory of the skin.</param>
        /// <param name="backUpDirectoryName">Directory to store the backup.</param>
        /// <param name="gameDirectoryName">Directory of the game.</param>
        /// <param name="_theMessenger">_theMessenger to use if there is an error.</param>
        public async Task CreateBackUpAsync(string skinDirectoryName, string backUpDirectoryName, string gameDirectoryName)
        {
            try
            {
                DirectoryInfo skinDirectory = new DirectoryInfo(skinDirectoryName);
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
        /// This method retores backed up game files to the game installation.
        /// </summary>
        /// <param name="sourceLocation">The back up location.</param>
        /// <param name="destinationLocation">The game installation location.</param>
        public void RestoreBackup(string skinDirectoryName, string gameDirectoryName)
        {
            try
            {
                DirectoryInfo skinDirectory = new DirectoryInfo(skinDirectoryName);
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
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }

        /// <summary>
        /// This method retores backed up game files to the game installation.
        /// </summary>
        /// <param name="sourceLocation">The back up location.</param>
        /// <param name="destinationLocation">The game installation location.</param>
        public async Task RestoreBackupAsync(string skinDirectoryName, string gameDirectoryName)
        {
            try
            {
                DirectoryInfo skinDirectory = new DirectoryInfo(skinDirectoryName);
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
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }

        }
        #endregion

        #region Get Methods
        /// <summary>
        /// Gets a list of applied skins.
        /// </summary>
        /// <param name="appliedSkinsFile">Applied skins file.</param>
        /// <returns>List of found applied skins locations.</returns>
        public IEnumerable<string> GetAppliedSkins(string appliedSkinsFile)
        {
            /*
             * List<Skin> appliedSkins = new List<Skin>();
            if (File.Exists(appliedSkinsFile))
            {
                using (XmlReader theReader = XmlReader.Create(appliedSkinsFile))
                {
                    Skin appliedSkin;
                    SkinType skinType = new SkinType(string.Empty, new List<string>());
                    string skinName = string.Empty;
                    string skinSubType = string.Empty;

                    while (theReader.ReadToFollowing("AppliedSkin"))
                    {
                        theReader.ReadToFollowing("Name");
                        skinName = theReader.ReadElementContentAsString("Name", "");

                        theReader.ReadToFollowing("Type");
                        skinType = new SkinType(theReader.ReadElementContentAsString("Type", ""), new List<string>());

                        theReader.ReadToFollowing("SubType");
                        skinSubType = theReader.ReadElementContentAsString("SubType", "");

                        appliedSkin = new Skin(skinType, skinSubType, skinName, string.Empty, string.Empty, string.Empty,
                            DateTime.Now, DateTime.Now);

                        appliedSkins.Add(appliedSkin);
                    }
                }
            }
             */
            try
            {
                if (File.Exists(appliedSkinsFile))
                {
                    return JsonSerializer.Deserialize<List<string>>(File.OpenRead(appliedSkinsFile)) ?? new List<string>();
                }
                else
                {
                    return new List<string>();
                }
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return new List<string>();
            }


        }

        /// <summary>
        /// Gets a list of applied skins.
        /// </summary>
        /// <param name="appliedSkinsFile">Applied skins file.</param>
        /// <returns>List of found applied skins locations.</returns>
        public async Task<IEnumerable<string>> GetAppliedSkinsAsync(string appliedSkinsFile)
        {
            try
            {
                List<string> appliedSkins = new List<string>();
                if (await FileExistsAsync(appliedSkinsFile))
                {
                    Stream reader = File.OpenRead(appliedSkinsFile);
                    appliedSkins = await JsonSerializer.DeserializeAsync<List<string>>(reader) ?? new List<string>();
                    reader.Close();
                }
                return new List<string>();
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return new List<string>();
            }
        }

        /// <summary>
        /// Gets a list of skin types.
        /// </summary>
        /// <param name="subTypesFile">Skin types file.</param>
        /// <returns>List of skin types.</returns>
        public IEnumerable<SkinType> GetSkinTypes(string skinTypesFile)
        {
            /*
             * List<SkinType> skinTypes = new List<SkinType>();

            if (File.Exists(subTypesFile))
            {
                using (XmlReader theReader = XmlReader.Create(subTypesFile))
                {
                    SkinType skinType;
                    List<string> subTypes;
                    string skinTypeName;

                    while (theReader.ReadToFollowing("SkinType"))
                    {
                        theReader.ReadToFollowing("Name");
                        skinTypeName = theReader.ReadElementContentAsString("Name", "");

                        subTypes = new List<string>();
                        theReader.ReadToFollowing("SubTypeName");
                        do
                        {
                            subTypes.Add(theReader.ReadElementContentAsString("SubTypeName", ""));

                        } while (theReader.ReadToNextSibling("SubTypeName"));

                        skinType = new SkinType(skinTypeName, subTypes);

                        skinTypes.Add(skinType);
                    }
                }
            }
            else
            {
                skinTypes = PopulateSkins().ToList();
            }

            return skinTypes;
             */
            try
            {
                if (File.Exists(skinTypesFile))
                {
                    return JsonSerializer.Deserialize<List<SkinType>>(File.OpenRead(skinTypesFile)) ?? new List<SkinType>();
                }
                else
                {
                    return new List<SkinType>();
                }
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return new List<SkinType>();
            }


        }

        /// <summary>
        /// Gets a list of skin types.
        /// </summary>
        /// <param name="subTypesFile">Skin types file.</param>
        /// <returns>List of skin types.</returns>
        public async Task<IEnumerable<SkinType>> GetSkinTypesAsync(string subTypesFile)
        {
            try
            {
                List<SkinType> skinTypes = new List<SkinType>();
                if (await FileExistsAsync(subTypesFile))
                {
                    Stream reader = File.OpenRead(subTypesFile);
                    skinTypes = await JsonSerializer.DeserializeAsync<List<SkinType>>(reader) ?? new List<SkinType>();
                    reader.Close();
                }
                return skinTypes;
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return new List<SkinType>();
            }
        }
        #endregion

        #region Save Methods
        /// <summary>
        /// Saves applied skins to an XML file.
        /// </summary>
        /// <param name="appliedSkins">Applied skins locations.</param>
        /// <param name="appliedSkinsFileName">File to save to.</param>
        /// <exception cref="Exception">Error saving file.</exception>
        public void SaveAppliedSkins(IEnumerable<string> appliedSkins, string appliedSkinsFileName)
        {
            /*
             * try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                using (XmlWriter writer = XmlWriter.Create(appliedSkinsFileName, settings))
                {
                    writer.WriteStartElement("AppliedSkins");

                    foreach (Skin theSkin in appliedSkins)
                    {
                        writer.WriteStartElement("AppliedSkin");

                        writer.WriteStartElement("Name");
                        writer.WriteString(theSkin.Name);
                        writer.WriteEndElement();

                        writer.WriteStartElement("Type");
                        writer.WriteString(theSkin.SkinType.Name);
                        writer.WriteEndElement();

                        writer.WriteStartElement("SubType");
                        writer.WriteString(theSkin.SubType);
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
             */

            try
            {
                JsonSerializer.Serialize(File.OpenWrite(appliedSkinsFileName), appliedSkins, new JsonSerializerOptions() { WriteIndented = true });
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }

        }
        /// <summary>
        /// Saves applied skins to a JSON file.
        /// </summary>
        /// <param name="appliedSkins">Applied skins locations.</param>
        /// <param name="appliedSkinsFileName">File to save to.</param>
        /// <exception cref="Exception">Error saving file.</exception>
        public async Task SaveAppliedSkinsAsync(IEnumerable<string> appliedSkins, string appliedSkinsFileName)
        {
            try
            {
                using Stream writer = File.OpenWrite(appliedSkinsFileName);
                await JsonSerializer.SerializeAsync<IEnumerable<string>>(writer, appliedSkins, new JsonSerializerOptions() { WriteIndented = true });
                writer.Close();
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }
        /// <summary>
        /// Saves a collection skin types to an XML file.
        /// </summary>
        /// <param name="skinTypes">Skin types.</param>
        /// <param name="skinTypesFile">File to save skin types to.</param>
        /// <exception cref="Exception">Error saving file.</exception>
        public void SaveSkinTypes(IEnumerable<SkinType> skinTypes, string skinTypesFile)
        {
            /*
             * try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                using (XmlWriter writer = XmlWriter.Create(skinTypesFile, settings))
                {
                    writer.WriteStartElement("SkinTypes");

                    foreach (SkinType skinType in skinTypes)
                    {
                        writer.WriteStartElement("SkinType");

                        writer.WriteStartElement("Name");
                        writer.WriteString(skinType.Name);
                        writer.WriteEndElement();

                        writer.WriteStartElement("SubTypes");
                        foreach (string subType in skinType.SubTypes)
                        {
                            writer.WriteStartElement("SubTypeName");
                            writer.WriteString(subType);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();

                    writer.Close();
                    return true;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
             */

            try
            {
                JsonSerializer.Serialize(File.OpenWrite(skinTypesFile), skinTypes, new JsonSerializerOptions() { WriteIndented = true });
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }

        }
        /// <summary>
        /// Saves skin types to a JSON file.
        /// </summary>
        /// <param name="skinTypes">Skin types.</param>
        /// <param name="skinTypesFile">File to save skin types to.</param>
        /// <exception cref="Exception">Error saving file.</exception>
        public async Task SaveSkinTypesAsync(IEnumerable<SkinType> skinTypes, string skinTypesFile)
        {
            try
            {
                using Stream writer = File.OpenWrite(skinTypesFile);
                await JsonSerializer.SerializeAsync(writer, skinTypes, new JsonSerializerOptions() { WriteIndented = true });
                writer.Close();
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }
        #endregion

        #region Exists Methods
        /// <summary>
        /// Checks if a file exists.
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
        /// </summary>
        /// <param name="fileLocation">Game executable location.</param>
        public void StartGame(string fileLocation)
        {
            if (File.Exists(fileLocation))
            {
                ProcessStartInfo psInfo = new ProcessStartInfo()
                {
                    FileName = Path.Combine(fileLocation),
                    Verb = "runas",
                    UseShellExecute = true
                };

                Process.Start(psInfo);
            }
        }

        /// <summary>
        /// Starts the game asynchronously.
        /// </summary>
        /// <param name="fileLocation">Game executable location.</param>
        public async Task StartGameAsync(string fileLocation)
        {
            if (await FileExistsAsync(fileLocation))
            {
                ProcessStartInfo psInfo = new ProcessStartInfo()
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
        private bool IsEmptyDirectory(string directoryPath)
        {
            return !Directory.EnumerateFileSystemEntries(directoryPath).Any();
        }

        private async Task<bool> IsEmptyDirectoryAsync(string directoryPath)
        {
            return await Task.Run(() => !Directory.EnumerateFileSystemEntries(directoryPath).Any());
        }
        #endregion

        /// <summary>
        /// Generates a list of default skin types.
        /// </summary>
        /// <returns>The default list of skin types</returns>
        private IEnumerable<SkinType> PopulateDefaultStructure()
        {
            List<SkinType> skinTypes = new List<SkinType>();
            skinTypes.Add(new SkinType("Area", new List<string>() { "Forest" }));
            skinTypes.Add(new SkinType("Armor", new List<string>() { "Arm" }));
            skinTypes.Add(new SkinType("Box", new List<string>() { "All" }));
            skinTypes.Add(new SkinType("Class", new List<string>() { "Warrior" }));
            skinTypes.Add(new SkinType("Effect", new List<string>() { "Fire" }));
            skinTypes.Add(new SkinType("Enemy", new List<string>() { "Grunt" }));
            skinTypes.Add(new SkinType("Helper", new List<string>() { "Guardian" }));
            skinTypes.Add(new SkinType("Shield", new List<string>() { "Broad" }));
            skinTypes.Add(new SkinType("UI", new List<string>() { "HUD" }));
            skinTypes.Add(new SkinType("Weapon", new List<string>() { "Sword" }));
            return skinTypes;
        }
    }
}
