﻿using CommunityToolkit.Mvvm.Messaging;
using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    /// <summary>
    /// This class loads settings information from the local file system.
    /// </summary>
    public static class SettingsLoaderService
    {
        /// <summary>
        /// Loads a collection of GameInfo from a JSON file.
        /// </summary>
        /// <param name="gameInfoFileName">JSON file to load.</param>
        /// <returns>Collection of GameInfo.</returns>
        public static IEnumerable<GameInfo> GetGameInfo(string gameInfoFileName, IMessenger messenger)
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
                    return JsonSerializer.Deserialize<IEnumerable<GameInfo>>(File.OpenRead(gameInfoFileName)) ?? new List<GameInfo>();
                }
                else
                {
                    return new List<GameInfo>();
                }
            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return new List<GameInfo>();
            }
        }
        /// <summary>
        /// Loads a collection of GameInfo from a JSON file.
        /// </summary>
        /// <param name="gameInfoFileName">JSON file to load.</param>
        /// <returns>Collection of GameInfo.</returns>
        public static async Task<IEnumerable<GameInfo>> GetGameInfoAsync(string gameInfoFileName, IMessenger messenger)
        {
            try
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
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return new List<GameInfo>();
            }

        }
        /// <summary>
        /// Saves a collection of GameInfo to a JSON file.
        /// </summary>
        /// <param name="gameInfo">Collection of GameInfo to save.</param>
        /// <param name="gameInfoFileName">JSON file to save to.</param>
        public static void SaveGameInfo(IEnumerable<GameInfo> gameInfo, string gameInfoFileName, IMessenger messenger)
        {
            try
            {
                JsonSerializer.Serialize(File.OpenWrite(gameInfoFileName), gameInfo, new JsonSerializerOptions() { WriteIndented = true });
            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }

        }
        /// <summary>
        /// Saves a collection of GameInfo to a JSON file.
        /// </summary>
        /// <param name="gameInfo">Collection of GameInfo to save.</param>
        /// <param name="gameInfoFileName">JSON file to save to.</param>
        public static async Task SaveGameInfoAsync(IEnumerable<GameInfo> gameInfo, string gameInfoFileName, IMessenger messenger)
        {
            try
            {
                using Stream writer = File.OpenWrite(gameInfoFileName);
                await JsonSerializer.SerializeAsync(writer, gameInfo, new JsonSerializerOptions() { WriteIndented = true });
                writer.Close();
            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
            }

        }
        /// <summary>
        /// Gets the known game information to prepopulate skin type and web skins resource.
        /// </summary>
        /// <param name="gameInfoFileName">JSON file of the known game info.</param>
        /// <returns>Collection of known game information.</returns>
        public static IEnumerable<KnownGameInfo> GetKnowGamesInfo(string knownGameInfoFileName, IMessenger messenger)
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
                    return JsonSerializer.Deserialize<IEnumerable<KnownGameInfo>>(File.OpenRead(knownGameInfoFileName)) ?? new List<KnownGameInfo>();
                }
                else
                {
                    return new List<KnownGameInfo>();
                }
            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return new List<KnownGameInfo>();
            }
        }
        /// <summary>
        /// Gets the known game information to prepopulate skin type and web skins resource.
        /// </summary>
        /// <param name="gameInfoFileName">JSON file of the known game info.</param>
        /// <returns>Collection of known game information.</returns>
        public static async Task<IEnumerable<KnownGameInfo>> GetKnowGamesInfoAsync(string knownGameInfoFileName, IMessenger messenger)
        {
            try
            {
                List<KnownGameInfo> theKnownGameInfo = new List<KnownGameInfo>();
                if (await FileExistsAsync(knownGameInfoFileName, messenger))
                {
                    Stream reader = File.OpenRead(knownGameInfoFileName);
                    theKnownGameInfo = await JsonSerializer.DeserializeAsync<List<KnownGameInfo>>(reader) ?? new List<KnownGameInfo>();
                    reader.Close();
                }
                return theKnownGameInfo;
            }
            catch (Exception ex)
            {
                messenger.Send<OperationErrorMessage>(new OperationErrorMessage(ex.GetType().Name, ex.Message));
                return new List<KnownGameInfo>();
            }

        }
        /// <summary>
        /// Saves a collection of KnownGameInfo to a file.
        /// </summary>
        /// <param name="knownGamesList">the collection of KnownGameInfo.</param>
        /// <param name="fileName">JSON file to save to.</param>
        /// <param name="messenger">Messenger to use if there is an error.</param>
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
    }
}