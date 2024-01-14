using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    /// <summary>
    /// This class facilitates loading and saving settings information to the local file system.
    /// </summary>
    public class SettingsLoaderService(IMessenger theMessenger) : ISettingsLoaderService
    {
        private readonly IMessenger _theMessenger = theMessenger;

        /// <summary>
        /// Loads a collection of GameInfo from a file.
        /// </summary>
        /// <param name="gameInfoFileName">File to load.</param>
        /// <returns>Collection of GameInfo.</returns>
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
            try
            {
                if (File.Exists(gameInfoFileName))
                {
                    using Stream fileStream = File.OpenRead(gameInfoFileName);
                    XmlSerializer theSerializer = new(typeof(List<GameInfo>));
                    return theSerializer.Deserialize(File.OpenRead(gameInfoFileName)) as List<GameInfo> ?? [];
                }
                else
                {
                    return [];
                }
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return [];
            }
        }
        /// <summary>
        /// Saves a collection of GameInfo to a file.
        /// </summary>
        /// <param name="gameInfo">Collection of GameInfo to save.</param>
        /// <param name="gameInfoFileName">File to save to.</param>
        public void SaveGameInfo(IEnumerable<GameInfo> gameInfo, string gameInfoFileName)
        {
            try
            {
                XmlSerializer theSerializer = new(gameInfo.GetType());
                using TextWriter writer = new StreamWriter(gameInfoFileName);
                theSerializer.Serialize(writer, gameInfo);
                writer.Close();
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }

        }
        /// <summary>
        /// Loads the KnownGameInfo to prepopulate skin type and web skins resource.
        /// </summary>
        /// <param name="knownGameInfoFileName">File of the known game info.</param>
        /// <returns>Collection of KnownGameInfo.</returns>
        public IEnumerable<KnownGameInfo> GetKnowGamesInfo(string knownGameInfoFileName)
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
            try
            {
                if (File.Exists(knownGameInfoFileName))
                {
                    using Stream fileStream = File.OpenRead(knownGameInfoFileName);
                    XmlSerializer theSerializer = new(typeof(List<KnownGameInfo>));
                    return theSerializer.Deserialize(File.OpenRead(knownGameInfoFileName)) as List<KnownGameInfo> ?? [];
                }
                else
                {
                    return [];
                }
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return [];
            }
        }
        /// <summary>
        /// Saves a collection of KnownGameInfo to a file.
        /// </summary>
        /// <param name="knownGamesList">The collection of KnownGameInfo.</param>
        /// <param name="fileName">File to save to.</param>
        public void SaveKnownGamesList(IEnumerable<KnownGameInfo> knownGamesList, string fileName)
        {
            try
            {
                XmlSerializer theSerializer = new(knownGamesList.GetType());
                using TextWriter writer = new StreamWriter(fileName);
                theSerializer.Serialize(writer, knownGamesList);
                writer.Close();
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }

        public Dictionary<string,List<Skin>> GetWebSkins(IEnumerable<string> gameNames)
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
                        List<Skin> foundSkins =  theSerializer.Deserialize(fileStream) as List<Skin> ?? [];
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

        public void SaveWebSkinsList(Dictionary<string, List<Skin>> webSkins)
        {
            try
            {
                foreach (string currentGameName in webSkins.Keys)
                {
                    XmlSerializer theSerializer = new(webSkins[currentGameName].GetType());
                    using TextWriter writer = new StreamWriter($"{currentGameName} Skins.xml");
                    theSerializer.Serialize(writer, webSkins[currentGameName]);
                    writer.Close();
                }
                
            }
            catch (Exception ex)
            {
                _theMessenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }
        }
        /// <summary>
        /// Checks if a file exists.
        /// </summary>
        /// <param name="fileName">File name to check.</param>
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

    }
}
