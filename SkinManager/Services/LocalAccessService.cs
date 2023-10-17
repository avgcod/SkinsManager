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
    public class LocalAccessService : ISkinsAccessService
    {
        /// <summary>
        /// Applies a skin.
        /// </summary>
        /// <param name="sourceLocation">Where the skin is located.</param>
        /// <param name="destination">The game disrectory.</param>
        /// <returns>If the operation succeeded.</returns>
        /// <exception cref="Exception">Error during application.</exception>
        public bool ApplySkin(string sourceLocation, string destination)
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
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #region CreateMethods
        /// <summary>
        /// Creates a folder.
        /// </summary>
        /// <param name="location">Folder location to create.</param>
        /// <returns>If the operation succeeded.</returns>
        /// <exception cref="Exception">Error during creation.</exception>
        public bool CreateFolder(string location)
        {
            if (!Directory.Exists(location))
            {
                try
                {
                    Directory.CreateDirectory(location);
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                return false;
            }

        }
        /// <summary>
        /// Creates a backup of the game file sthat will be overwritten by the skin.
        /// </summary>
        /// <param name="sourceLocation">Skin location.</param>
        /// <param name="backUpLocation">Where to save the backups.</param>
        /// <param name="installationLocation">Game location.</param>
        /// <returns>If the operation succeded</returns>
        /// <exception cref="Exception">Error during backup.</exception>
        public bool CreateBackUp(string sourceLocation, string backUpLocation, string installationLocation)
        {
            try
            {
                DirectoryInfo[] subDirs = new DirectoryInfo(sourceLocation).GetDirectories("*.*", SearchOption.AllDirectories);

                foreach (DirectoryInfo currentFolder in subDirs)
                {
                    CreateFolder(Path.Combine(backUpLocation, currentFolder.FullName.Replace(currentFolder.FullName, string.Empty)));
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
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// Creates folder structure for skins.
        /// </summary>
        /// <param name="skinTypes">Types of skins.</param>
        /// <param name="skinsFolder">Folder for skins.</param>
        /// <returns>If the operation succeeded.</returns>        
        public bool CreateStructure(IEnumerable<SkinType> skinTypes, string skinsFolder)
        {
            if (!skinTypes.Any())
            {
                skinTypes = PopulateDefaultStructure();
            }
            foreach (SkinType currentType in skinTypes)
            {
                CreateFolder(Path.Combine(skinsFolder, currentType.Name));

                foreach (string subType in currentType.SubTypes)
                {
                    CreateFolder(Path.Combine(skinsFolder, currentType.Name, subType));
                }
            }
            return true;

        }
        /// <summary>
        /// Creates folder structure for skins.
        /// </summary>
        /// <param name="skinTypes">Types of skins.</param>
        /// <param name="skinsFolder">Folder for skins.</param>
        /// <returns>If the operation succeeded.</returns>        
        public async Task<bool> CreateStructureAsync(IEnumerable<SkinType> skinTypes, string skinsFolder)
        {
            if (!skinTypes.Any())
            {
                skinTypes = PopulateDefaultStructure();
            }
            bool succeeded = false;
            await Task.Run(() =>
            {
                foreach (SkinType currentType in skinTypes)
                {
                    CreateFolder(Path.Combine(skinsFolder, currentType.Name));

                    foreach (string subType in currentType.SubTypes)
                    {
                        CreateFolder(Path.Combine(skinsFolder, currentType.Name, subType));
                    }
                }
                succeeded = true;
            });

            return succeeded;

        }
        /// <summary>
        /// Creates a folder
        /// </summary>
        /// <param name="location">Folder location to create.</param>
        /// <returns>If the operation succeeded.</returns>
        /// <exception cref="Exception">Error during creation.</exception>
        public async Task<bool> CreateFolderAsync(string location)
        {
            if (!await DirectoryExistsAsync(location))
            {
                try
                {
                    await Task.Run(() => Directory.CreateDirectory(location));
                    return true;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Creates a backup of the game file sthat will be overwritten by the skin.
        /// </summary>
        /// <param name="sourceLocation">Skin location.</param>
        /// <param name="backUpLocation">Where to save the backups.</param>
        /// <param name="installationLocation">Game location.</param>
        /// <returns>If the operation succeded</returns>
        /// <exception cref="Exception">Error during backup.</exception>
        public async Task<bool> CreateBackUpAsync(string sourceLocation, string backUpLocation, string installationLocation)
        {
            try
            {
                DirectoryInfo[] subDirs = await Task.Run(() => new DirectoryInfo(sourceLocation).GetDirectories("*.*", SearchOption.AllDirectories));

                await Task.Run(() =>
                {
                    foreach (DirectoryInfo currentFolder in subDirs)
                    {
                        CreateFolder(Path.Combine(backUpLocation, currentFolder.FullName.Replace(currentFolder.FullName, string.Empty)));
                        foreach (FileInfo theFile in currentFolder.GetFiles())
                        {
                            File.Copy(Path.Combine(installationLocation, currentFolder.FullName.Replace(currentFolder.FullName, string.Empty), theFile.Name),
                                    Path.Combine(backUpLocation, currentFolder.FullName.Replace(currentFolder.FullName, string.Empty), theFile.Name), false);
                        }
                    }
                });
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// This method retores backed up game files to the game installation.
        /// </summary>
        /// <param name="sourceLocation">The back up location.</param>
        /// <param name="destinationLocation">The game installation location.</param>
        /// <returns></returns>
        public bool RestoreBackup(string sourceLocation, string destinationLocation)
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
                return true;
            }

            return false;
        }

        /// <summary>
        /// This method retores backed up game files to the game installation.
        /// </summary>
        /// <param name="sourceLocation">The back up location.</param>
        /// <param name="destinationLocation">The game installation location.</param>
        /// <returns></returns>public async Task<bool> RestoreBackupAsync(string sourceLocation, string destinationLocation)
        public async Task<bool> RestoreBackupAsync(string sourceLocation, string destinationLocation)
        {
            try
            {
                if (await DirectoryExistsAsync(sourceLocation) && await DirectoryExistsAsync(destinationLocation))
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
                        return true;

                    });
                }
                
                return false;

            }
            catch (Exception ex)
            {
                return false;
            }

        }
        #endregion

        #region GetMethods
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

            if (File.Exists(appliedSkinsFile))
            {
                return JsonSerializer.Deserialize<List<string>>(File.OpenRead(appliedSkinsFile)) ?? new List<string>();
            }
            return new List<string>();
        }
        /// <summary>
        /// Gets a list of available skins.
        /// </summary>
        /// <param name="skinsFolder">Location of skins.</param>
        /// <returns>List of available skins.</returns>
        public IEnumerable<Skin> GetAvailableSkins(string skinsFolder)
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
        /// <summary>
        /// Gets a list of skin types.
        /// </summary>
        /// <param name="subTypesFile">Skin types file.</param>
        /// <returns>List of skin types.</returns>
        public IEnumerable<SkinType> GetSkinTypes(string subTypesFile)
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

            List<SkinType> skinTypes = new List<SkinType>();
            if (File.Exists(subTypesFile))
            {
                skinTypes = JsonSerializer.Deserialize<List<SkinType>>(File.OpenRead(subTypesFile)) ?? new List<SkinType>();
            }
            return skinTypes;

        }
        /// <summary>
        /// Gets the game information.
        /// </summary>
        /// <param name="gameInfoFileName">File location of the game info.</param>
        /// <returns>List of game information.</returns>
        public IEnumerable<GameInfo> GetGameInfo(string gameInfoFileName)
        {
            /*
             * List<string> directories = new List<string>();
            if (File.Exists(directoriesFile))
            {
                using (XmlReader theReader = XmlReader.Create(directoriesFile))
                {
                    string directoryLocation = string.Empty;

                    theReader.ReadToFollowing("SkinsLocation");
                    directoryLocation = theReader.ReadElementContentAsString("SkinsLocation", "");
                    directories.Add(directoryLocation);

                    theReader.ReadToFollowing("InstallLocation");
                    directoryLocation = theReader.ReadElementContentAsString("InstallLocation", "");
                    directories.Add(directoryLocation);
                }
            }
            return directories;
             */

            if (File.Exists(gameInfoFileName))
            {
                return JsonSerializer.Deserialize<IEnumerable<GameInfo>>(File.OpenRead(gameInfoFileName)) ?? new List<GameInfo>();
            }
            else
            {
                return new List<GameInfo>();
            }

        }
        /// <summary>
        /// Gets the game information.
        /// </summary>
        /// <param name="gameInfoFileName">File location of the game info.</param>
        /// <returns>List of game information.</returns>
        public async Task<IEnumerable<GameInfo>> GetGameInfoAsync(string gameInfoFileName)
        {
            List<GameInfo> theGameInfo = new List<GameInfo>();
            if (File.Exists(gameInfoFileName))
            {
                Stream reader = File.OpenRead(gameInfoFileName);
                theGameInfo = await JsonSerializer.DeserializeAsync<List<GameInfo>>(reader) ?? new List<GameInfo>();
                reader.Close();
            }
            return theGameInfo;
        }
        /// <summary>
        /// Gets a list of applied skins.
        /// </summary>
        /// <param name="appliedSkinsFile">Applied skins file.</param>
        /// <returns>List of found applied skins locations.</returns>
        public async Task<IEnumerable<string>> GetAppliedSkinsAsync(string appliedSkinsFile)
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
        /// <summary>
        /// Gets a list of available skins.
        /// </summary>
        /// <param name="skinsFolder">Location of skins.</param>
        /// <returns>List of available skins.</returns>
        public async Task<IEnumerable<Skin>> GetAvailableSkinsAsync(string skinsFolder)
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
        /// <summary>
        /// Gets a list of skin types.
        /// </summary>
        /// <param name="subTypesFile">Skin types file.</param>
        /// <returns>List of skin types.</returns>
        public async Task<IEnumerable<SkinType>> GetSkinTypesAsync(string subTypesFile)
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
        #endregion

        #region SaveMethods
        /// <summary>
        /// Saves skin types to an XML file.
        /// </summary>
        /// <param name="skinTypes">Skin types.</param>
        /// <param name="skinTypesFile">File to save skin types to.</param>
        /// <returns>If the operation succeeded.</returns>
        /// <exception cref="Exception">Error saving file.</exception>
        public bool SaveSkinTypes(IEnumerable<SkinType> skinTypes, string skinTypesFile)
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

            JsonSerializer.Serialize(File.OpenWrite(skinTypesFile), skinTypes, new JsonSerializerOptions() { WriteIndented = true });

            return true;

        }
        /// <summary>
        /// Saves applied skins to an XML file.
        /// </summary>
        /// <param name="appliedSkins">Applied skins locations.</param>
        /// <param name="appliedSkinsFileName">File to save to.</param>
        /// <returns>If the operation succeded.</returns>
        /// <exception cref="Exception">Error saving file.</exception>
        public bool SaveAppliedSkins(IEnumerable<string> appliedSkins, string appliedSkinsFileName)
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

            JsonSerializer.Serialize(File.OpenWrite(appliedSkinsFileName), appliedSkins, new JsonSerializerOptions() { WriteIndented = true });

            return true;

        }
        /// <summary>
        /// Saves the current directories to an XML file.
        /// </summary>
        /// <param name="gameInfo">List of GameInfo to save.</param>
        /// <param name="gameInfoFileName">Name of the XML file.</param>
        /// <returns>If the operation succeeded.</returns>
        /// <exception cref="Exception">Error saving.</exception>
        public bool SaveGameInfo(IEnumerable<GameInfo> gameInfo, string gameInfoFileName)
        {
            JsonSerializer.Serialize<IEnumerable<GameInfo>>(File.OpenWrite(gameInfoFileName), gameInfo, new JsonSerializerOptions() { WriteIndented = true });

            return true;

        }
        /// <summary>
        /// Saves the current directories to a JSON file.
        /// </summary>
        /// <param name="gameInfo">List of gameInfo to save.</param>
        /// <param name="gameInfoFileName">Name of the XML file.</param>
        /// <returns>If the operation succeeded.</returns>
        /// <exception cref="Exception">Error saving.</exception>
        public async Task<bool> SaveGameInfoAsync(IEnumerable<GameInfo> gameInfo, string gameInfoFileName)
        {
            using Stream writer = File.OpenWrite(gameInfoFileName);
            await JsonSerializer.SerializeAsync<IEnumerable<GameInfo>>(writer, gameInfo, new JsonSerializerOptions() { WriteIndented = true });
            writer.Close();

            return true;
        }
        /// <summary>
        /// Saves applied skins to a JSON file.
        /// </summary>
        /// <param name="appliedSkins">Applied skins locations.</param>
        /// <param name="appliedSkinsFileName">File to save to.</param>
        /// <returns>If the operation succeded.</returns>
        /// <exception cref="Exception">Error saving file.</exception>
        public async Task<bool> SaveAppliedSkinsAsync(IEnumerable<string> appliedSkins, string appliedSkinsFileName)
        {
            using Stream writer = File.OpenWrite(appliedSkinsFileName);
            await JsonSerializer.SerializeAsync<IEnumerable<string>>(writer, appliedSkins, new JsonSerializerOptions() { WriteIndented = true });
            writer.Close();

            return true;
        }
        /// <summary>
        /// Saves skin types to a JSON file.
        /// </summary>
        /// <param name="skinTypes">Skin types.</param>
        /// <param name="skinTypesFile">File to save skin types to.</param>
        /// <returns>If the operation succeeded.</returns>
        /// <exception cref="Exception">Error saving file.</exception>
        public async Task<bool> SaveSkinTypesAsync(IEnumerable<SkinType> skinTypes, string skinTypesFile)
        {
            using Stream writer = File.OpenWrite(skinTypesFile);
            await JsonSerializer.SerializeAsync(writer, skinTypes, new JsonSerializerOptions() { WriteIndented = true });
            writer.Close();

            return true;
        }
        #endregion

        /// <summary>
        /// Generates a list of default skin types for Phantasy Star Online Blue Burst.
        /// </summary>
        /// <returns>The default list of skin types</returns>
        private static IEnumerable<SkinType> PopulateDefaultPSOBBSkins()
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
        private Task<bool> FileExistsAsync(string fileName)
        {
            return Task.Run(() => File.Exists(fileName));
        }

        /// <summary>
        /// Checks if a directory exists.
        /// </summary>
        /// <param name="directoryName">Directory name.</param>
        /// <returns>If the directory exists.</returns>
        private Task<bool> DirectoryExistsAsync(string directoryName)
        {
            return Task.Run(() => Directory.Exists(directoryName));
        }

        /// <summary>
        /// Applies the skin to the game.
        /// </summary>
        /// <param name="sourceLocation">Skin location.</param>
        /// <param name="destination">Game location to apply the skin to.</param>
        /// <returns>If the operation succeded.</returns>
        /// <exception cref="Exception">Error applying the skin.</exception>
        public async Task<bool> ApplySkinAsync(string sourceLocation, string destination)
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
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

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

                await Task.Run(() => Process.Start(psInfo));
            }
        }
    }
}
