using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    /// <summary>
    /// This class provides access to files on the local file system.
    /// </summary>
    public class LocalSkinsAccessService : ISkinsAccessService
    {
        /// <summary>
        /// Applies a skin.
        /// </summary>
        /// <param name="sourceLocation">Where the skin is located.</param>
        /// <param name="destination">The game disrectory.</param>
        /// <exception cref="Exception">Error during application.</exception>
        public void ApplySkin(string sourceLocation, string destination, IMessenger messenger)
        {
            try
            {
                foreach (DirectoryInfo folder in new DirectoryInfo(sourceLocation).GetDirectories("*.*", SearchOption.AllDirectories))
                {
                    foreach (FileInfo theFile in folder.GetFiles())
                    {
                        File.Copy(theFile.FullName, Path.Combine(destination, folder.FullName.Replace(sourceLocation, string.Empty),
                            theFile.Name), true);
                    }
                }
            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }

        /// <summary>
        /// Applies the skin to the game.
        /// </summary>
        /// <param name="sourceLocation">Skin location.</param>
        /// <param name="destination">Game location to apply the skin to.</param>
        /// <exception cref="Exception">Error applying the skin.</exception>
        public async Task ApplySkinAsync(string sourceLocation, string destination, IMessenger messenger)
        {
            try
            {
                await Task.Run(() =>
                {
                    foreach (DirectoryInfo folder in new DirectoryInfo(sourceLocation).GetDirectories("*.*", SearchOption.AllDirectories))
                    {
                        foreach (FileInfo theFile in folder.GetFiles())
                        {
                            File.Copy(theFile.FullName, Path.Combine(destination, folder.FullName.Replace(sourceLocation, string.Empty),
                                theFile.Name), true);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }

        #region CreateMethods

        /// <summary>
        /// Creates a folder.
        /// </summary>
        /// <param name="location">Folder location to create.</param>
        /// <exception cref="Exception">Error during creation.</exception>
        public void CreateFolder(string location, IMessenger messenger)
        {
            try
            {
                Directory.CreateDirectory(location);
            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }

        }
        /// <summary>
        /// Creates a backup of the game file sthat will be overwritten by the skin.
        /// </summary>
        /// <param name="sourceLocation">Skin location.</param>
        /// <param name="backUpLocation">Where to save the backups.</param>
        /// <param name="installationLocation">Game location.</param>
        /// <exception cref="Exception">Error during backup.</exception>
        public void CreateBackUp(string sourceLocation, string backUpLocation, string installationLocation, IMessenger messenger)
        {
            try
            {
                DirectoryInfo[] subDirs = new DirectoryInfo(sourceLocation).GetDirectories("*.*", SearchOption.AllDirectories);

                foreach (DirectoryInfo currentFolder in subDirs)
                {
                    CreateFolder(Path.Combine(backUpLocation, currentFolder.FullName.Replace(currentFolder.FullName, string.Empty)), messenger);
                    foreach (FileInfo theFile in currentFolder.GetFiles())
                    {
                        File.Copy(Path.Combine(installationLocation, currentFolder.FullName.Replace(currentFolder.FullName, string.Empty), theFile.Name),
                                Path.Combine(backUpLocation, currentFolder.FullName.Replace(currentFolder.FullName, string.Empty), theFile.Name), true);
                    }

                    /*
                     *                     foreach (FileInfo theFile in currentFolder.GetFiles())
                    {
                        if (currentFolder.Name.ToLower().Contains("data") && File.Exists(installationLocation + Path.DirectorySeparatorChar + theFile.Name))
                        {
                            tempDestination = backUpLocation + Path.DirectorySeparatorChar + currentFolder.Name;
                            if (!Directory.Exists(tempDestination))
                            {
                                CreateFolder(tempDestination);
                            }
                            File.Copy(Path.Combine(installationLocation, theFile.Name),
                                Path.Combine(backUpLocation + Path.DirectorySeparatorChar + currentFolder.Name, theFile.Name), true);
                        }
                    }
                     */

                }
            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }
        /// <summary>
        /// Creates folder structure for skins.
        /// </summary>
        /// <param name="skinTypes">Types of skins.</param>
        /// <param name="skinsFolder">Folder for skins.</param>
        public void CreateStructure(IEnumerable<SkinType> skinTypes, string skinsFolder, IMessenger messenger)
        {
            if (IsEmptyDirectory(skinsFolder))
            {
                if (!skinTypes.Any())
                {
                    skinTypes = PopulateDefaultStructure();
                }
                try
                {
                    foreach (SkinType currentType in skinTypes)
                    {
                        CreateFolder(Path.Combine(skinsFolder, currentType.Name), messenger);

                        foreach (string subType in currentType.SubTypes)
                        {
                            CreateFolder(Path.Combine(skinsFolder, currentType.Name, subType), messenger);
                        }
                    }
                }
                catch (Exception ex)
                {
                    messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                }
            }
            else
            {
                messenger.Send<DirectoryNotEmptyMessage>(new DirectoryNotEmptyMessage(skinsFolder));
            }


        }
        /// <summary>
        /// Creates folder structure for skins.
        /// </summary>
        /// <param name="skinTypes">Types of skins.</param>
        /// <param name="skinsFolder">Folder for skins.</param> 
        public async Task CreateStructureAsync(IEnumerable<SkinType> skinTypes, string skinsFolder, IMessenger messenger)
        {
            if (await IsEmptyDirectoryAsync(skinsFolder))
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
                            CreateFolder(Path.Combine(skinsFolder, currentType.Name), messenger);

                            foreach (string subType in currentType.SubTypes)
                            {
                                CreateFolder(Path.Combine(skinsFolder, currentType.Name, subType), messenger);
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                }
            }
            else
            {
                messenger.Send<DirectoryNotEmptyMessage>(new DirectoryNotEmptyMessage(skinsFolder));
            }

        }
        /// <summary>
        /// Creates a folder
        /// </summary>
        /// <param name="location">Folder location to create.</param>
        /// <exception cref="Exception">Error during creation.</exception>
        public async Task CreateFolderAsync(string location, IMessenger messenger)
        {
            if (!await DirectoryExistsAsync(location, messenger))
            {
                try
                {
                    await Task.Run(() => Directory.CreateDirectory(location));
                }
                catch (Exception ex)
                {
                    messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                }
            }
        }
        /// <summary>
        /// Creates a backup of the game file sthat will be overwritten by the skin.
        /// </summary>
        /// <param name="sourceLocation">Skin location.</param>
        /// <param name="backUpLocation">Where to save the backups.</param>
        /// <param name="installationLocation">Game location.</param>
        /// <exception cref="Exception">Error during backup.</exception>
        public async Task CreateBackUpAsync(string sourceLocation, string backUpLocation, string installationLocation, IMessenger messenger)
        {
            try
            {
                DirectoryInfo[] subDirs = await Task.Run(() => new DirectoryInfo(sourceLocation).GetDirectories("*.*", SearchOption.AllDirectories));

                await Task.Run(async () =>
                {
                    foreach (DirectoryInfo currentFolder in subDirs)
                    {
                        await CreateFolderAsync(Path.Combine(backUpLocation, currentFolder.FullName.Replace(currentFolder.FullName, string.Empty)), messenger);
                        foreach (FileInfo theFile in currentFolder.GetFiles())
                        {
                            File.Copy(Path.Combine(installationLocation, currentFolder.FullName.Replace(currentFolder.FullName, string.Empty), theFile.Name),
                                    Path.Combine(backUpLocation, currentFolder.FullName.Replace(currentFolder.FullName, string.Empty), theFile.Name), false);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }

        #endregion

        /// <summary>
        /// This method retores backed up game files to the game installation.
        /// </summary>
        /// <param name="sourceLocation">The back up location.</param>
        /// <param name="destinationLocation">The game installation location.</param>
        public void RestoreBackup(string sourceLocation, string destinationLocation, IMessenger messenger)
        {
            try
            {
                if (Directory.Exists(sourceLocation) && Directory.Exists(destinationLocation))
                {
                    foreach (DirectoryInfo currentDirectory in new DirectoryInfo(sourceLocation).GetDirectories("*.*", SearchOption.AllDirectories))
                    {
                        foreach (FileInfo theFile in currentDirectory.GetFiles())
                        {
                            File.Copy(Path.Combine(sourceLocation, currentDirectory.FullName.Replace(currentDirectory.FullName, string.Empty), theFile.Name),
                                Path.Combine(destinationLocation, currentDirectory.FullName.Replace(currentDirectory.FullName, string.Empty), theFile.Name), true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }

        /// <summary>
        /// This method retores backed up game files to the game installation.
        /// </summary>
        /// <param name="sourceLocation">The back up location.</param>
        /// <param name="destinationLocation">The game installation location.</param>
        public async Task RestoreBackupAsync(string sourceLocation, string destinationLocation, IMessenger messenger)
        {
            try
            {
                if (await DirectoryExistsAsync(sourceLocation, messenger) && await DirectoryExistsAsync(destinationLocation, messenger))
                {
                    await Task.Run(() =>
                    {
                        foreach (DirectoryInfo currentDirectory in new DirectoryInfo(sourceLocation).GetDirectories("*.*", SearchOption.AllDirectories))
                        {
                            foreach (FileInfo theFile in currentDirectory.GetFiles())
                            {
                                File.Copy(Path.Combine(sourceLocation, currentDirectory.FullName.Replace(currentDirectory.FullName, string.Empty), theFile.Name),
                                        Path.Combine(destinationLocation, currentDirectory.FullName.Replace(currentDirectory.FullName, string.Empty), theFile.Name), true);
                            }
                        }

                    });
                }

            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }

        }

        #region GetMethods
        /// <summary>
        /// Gets a list of applied skins.
        /// </summary>
        /// <param name="appliedSkinsFile">Applied skins file.</param>
        /// <returns>List of found applied skins locations.</returns>
        public IEnumerable<string> GetAppliedSkins(string appliedSkinsFile, IMessenger messenger)
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
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return new List<string>();
            }


        }
        /// <summary>
        /// Gets a list of available skins.
        /// </summary>
        /// <param name="skinsFolder">Location of skins.</param>
        /// <returns>List of available skins.</returns>
        public IEnumerable<Skin> GetAvailableSkins(string skinsFolder, IMessenger messenger)
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
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return new List<Skin>();
            }
        }
        /// <summary>
        /// Gets a list of skin types.
        /// </summary>
        /// <param name="subTypesFile">Skin types file.</param>
        /// <returns>List of skin types.</returns>
        public IEnumerable<SkinType> GetSkinTypes(string skinTypesFile, IMessenger messenger)
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
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return new List<SkinType>();
            }


        }

        /// <summary>
        /// Gets a list of applied skins.
        /// </summary>
        /// <param name="appliedSkinsFile">Applied skins file.</param>
        /// <returns>List of found applied skins locations.</returns>
        public async Task<IEnumerable<string>> GetAppliedSkinsAsync(string appliedSkinsFile, IMessenger messenger)
        {
            try
            {
                List<string> appliedSkins = new List<string>();
                if (await FileExistsAsync(appliedSkinsFile, messenger))
                {
                    Stream reader = File.OpenRead(appliedSkinsFile);
                    appliedSkins = await JsonSerializer.DeserializeAsync<List<string>>(reader) ?? new List<string>();
                    reader.Close();
                }
                return new List<string>();
            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return new List<string>();
            }
        }
        /// <summary>
        /// Gets a list of available skins.
        /// </summary>
        /// <param name="skinsFolder">Location of skins.</param>
        /// <returns>List of available skins.</returns>
        public async Task<IEnumerable<Skin>> GetAvailableSkinsAsync(string skinsFolder, IMessenger messenger)
        {
            try
            {
                List<Skin> skins = new List<Skin>();
                if (await DirectoryExistsAsync(skinsFolder, messenger))
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
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return new List<Skin>();
            }
        }
        /// <summary>
        /// Gets a list of skin types.
        /// </summary>
        /// <param name="subTypesFile">Skin types file.</param>
        /// <returns>List of skin types.</returns>
        public async Task<IEnumerable<SkinType>> GetSkinTypesAsync(string subTypesFile, IMessenger messenger)
        {
            try
            {
                List<SkinType> skinTypes = new List<SkinType>();
                if (await FileExistsAsync(subTypesFile, messenger))
                {
                    Stream reader = File.OpenRead(subTypesFile);
                    skinTypes = await JsonSerializer.DeserializeAsync<List<SkinType>>(reader) ?? new List<SkinType>();
                    reader.Close();
                }
                return skinTypes;
            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return new List<SkinType>();
            }
        }
        #endregion

        #region SaveMethods
        /// <summary>
        /// Saves skin types to an XML file.
        /// </summary>
        /// <param name="skinTypes">Skin types.</param>
        /// <param name="skinTypesFile">File to save skin types to.</param>
        /// <exception cref="Exception">Error saving file.</exception>
        public void SaveSkinTypes(IEnumerable<SkinType> skinTypes, string skinTypesFile, IMessenger messenger)
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
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }

        }
        /// <summary>
        /// Saves applied skins to an XML file.
        /// </summary>
        /// <param name="appliedSkins">Applied skins locations.</param>
        /// <param name="appliedSkinsFileName">File to save to.</param>
        /// <exception cref="Exception">Error saving file.</exception>
        public void SaveAppliedSkins(IEnumerable<string> appliedSkins, string appliedSkinsFileName, IMessenger messenger)
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
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }

        }
        /// <summary>
        /// Saves the current directories to an XML file.
        /// </summary>
        /// <param name="gameInfo">List of GameInfo to save.</param>
        /// <param name="gameInfoFileName">Name of the XML file.</param>
        /// <exception cref="Exception">Error saving.</exception>

        /// <summary>
        /// Saves applied skins to a JSON file.
        /// </summary>
        /// <param name="appliedSkins">Applied skins locations.</param>
        /// <param name="appliedSkinsFileName">File to save to.</param>
        /// <exception cref="Exception">Error saving file.</exception>
        public async Task SaveAppliedSkinsAsync(IEnumerable<string> appliedSkins, string appliedSkinsFileName, IMessenger messenger)
        {
            try
            {
                using Stream writer = File.OpenWrite(appliedSkinsFileName);
                await JsonSerializer.SerializeAsync<IEnumerable<string>>(writer, appliedSkins, new JsonSerializerOptions() { WriteIndented = true });
                writer.Close();
            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }
        /// <summary>
        /// Saves skin types to a JSON file.
        /// </summary>
        /// <param name="skinTypes">Skin types.</param>
        /// <param name="skinTypesFile">File to save skin types to.</param>
        /// <exception cref="Exception">Error saving file.</exception>
        public async Task SaveSkinTypesAsync(IEnumerable<SkinType> skinTypes, string skinTypesFile, IMessenger messenger)
        {
            try
            {
                using Stream writer = File.OpenWrite(skinTypesFile);
                await JsonSerializer.SerializeAsync(writer, skinTypes, new JsonSerializerOptions() { WriteIndented = true });
                writer.Close();
            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }

        /// <summary>
        /// Saves PSOBB skin types to a JSON file.
        /// </summary>
        /// <param name="skinTypesFile">File to save skin types to.</param>
        /// <exception cref="Exception">Error saving file.</exception>
        public static async Task SavePSOBBSkinTypesAsync(string skinTypesFile, IMessenger messenger)
        {
            try
            {
                using Stream writer = File.OpenWrite(skinTypesFile);
                await JsonSerializer.SerializeAsync(writer, PopulateDefaultPSOBBSkinTypes(), new JsonSerializerOptions() { WriteIndented = true });
                writer.Close();
            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }

        public static async Task SaveKnownGamesListAsync(IEnumerable<KnownGameInfo> knownGamesList, string fileName, IMessenger messenger)
        {
            try
            {
                using Stream writer = File.OpenWrite(fileName);
                await JsonSerializer.SerializeAsync(writer, knownGamesList, new JsonSerializerOptions() { WriteIndented = true });
                writer.Close();
            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }

        #endregion

        /// <summary>
        /// Generates a list of default skin types for Phantasy Star Online Blue Burst.
        /// </summary>
        /// <returns>The default list of skin types</returns>
        private static IEnumerable<SkinType> PopulateDefaultPSOBBSkinTypes()
        {
            List<SkinType> defaultSkinTypes = new List<SkinType>();
            defaultSkinTypes.Add(new SkinType("Area", new List<string>()
                        {
                            "Forest", "Caves", "Mines", "Ruins",
                            "Jungle", "Mountain", "Seaside", "CCA",
                            "Seabed", "Crater", "Desert", "Lobby",
                            "City", "Temple", "Spaceship", "Towers"
                        }));

            defaultSkinTypes.Add(new SkinType("Enemy", new List<string>()
                        {
                            "Booma", "Gobooma", "Gigobooma", "Booma Family",
                            "Rappy", "Al Rappy", "Rappy Family", "Monest",
                            "Mothmant", "Savage Wolf", "Barbarouse Wolf", "Wolf Family",
                            "Hildebear", "Hildeblue", "Hidelt", "Hildetorr",
                            "Dargon", "Sil Dragon", "Evil Shark", "Pal Shark",
                            "Guil Shark", "Shark Family", "Poison Lily", "Nar Lily",
                            "Grass Assassin", "Nano Dragon", "Pan Arms", "Hiddom",
                            "Migium", "DelRoLe", "DelRalLie", "Gilchic",
                            "Dubchic", "Canadine", "Canane", "Sinow Beat",
                            "Sinow Gold", "Garanz", "Vol Opt", "Dimenian",
                            "La Dimenian", "So Dimenian", "Dimenian Family", "Claw",
                            "Bulclaw", "Bulk", "Delsaber", "Dark Belra",
                            "Chaos Sorcerer", "Dark Gunner", "Chaos Bringer", "Dark Falz",
                            "Merillia", "Meritas", "Mericarol", "Ul Gibbon",
                            "Zol Gibbon", "Gibbles", "Gee", "Gi Gue",
                            "Sinow Berril", "Sinow Spigell", "Gol Dragon", "Gal Gryphon",
                            "Domolm", "Dolmdari", "Recon", "Reconbox",
                            "Sinow Zoa", "Sinow Zele", "Morfos", "Deldepth",
                            "Delbiter", "Epsilon", "Olga Flow", "Sand Rappy",
                            "Del Rappy", "Girtablublu", "Goran", "Pyro Goran",
                            "Goran Detonator", "Merissa A", "Merissa AA", "Zu",
                            "Pazuzu", "Satellite Lizard", "Yowie"

                        }));

            defaultSkinTypes.Add(new SkinType("Class", new List<string>()
                        {
                            "Humar", "Hucast", "Hunewearl", "Hucaseal",
                            "Fomar", "Fomarl", "Fonewearl", "Ramar",
                            "Racast", "Ramarl", "Racaseal"
                        }));
            defaultSkinTypes.Add(new SkinType("UI", new List<string>()
                        {
                            "HUD", "Title", "Audio","Box"
                        }));
            defaultSkinTypes.Add(new SkinType("Equipment", new List<string>()
                        {
                            "Armor", "Shield", "Weapon","Mag"
                        }));
            defaultSkinTypes.Add(new SkinType("Effect", new List<string>()
                        {
                            "Attack", "Buff", "Healing","Other"
                        }));
            return defaultSkinTypes;
        }

        /// <summary>
        /// Generates a list of default skin types.
        /// </summary>
        /// <returns>The default list of skin types</returns>
        private static IEnumerable<SkinType> PopulateDefaultStructure()
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

        /// <summary>
        /// Checks if a file exists.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <returns>If the file exists.</returns>
        private static async Task<bool> FileExistsAsync(string fileName, IMessenger messenger)
        {
            try
            {
                return await Task.Run(() => File.Exists(fileName));
            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return false;
            }

        }

        /// <summary>
        /// Checks if a directory exists.
        /// </summary>
        /// <param name="directoryName">Directory name.</param>
        /// <returns>If the directory exists.</returns>
        private static async Task<bool> DirectoryExistsAsync(string directoryName, IMessenger messenger)
        {
            try
            {
                return await Task.Run(() => Directory.Exists(directoryName));
            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return false;
            }
        }

        /// <summary>
        /// Starts the game.
        /// </summary>
        /// <param name="fileLocation">Game executable location.</param>
        public void StartGame(string fileLocation, IMessenger messenger)
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
        public async Task StartGameAsync(string fileLocation, IMessenger messenger)
        {
            if (await FileExistsAsync(fileLocation, messenger))
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
                    messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                }
            }
        }

        private bool IsEmptyDirectory(string directoryPath)
        {
            return !Directory.EnumerateFileSystemEntries(directoryPath).Any();
        }

        private async Task<bool> IsEmptyDirectoryAsync(string directoryPath)
        {
            return await Task.Run(() => !Directory.EnumerateFileSystemEntries(directoryPath).Any());
        }
    }
}
