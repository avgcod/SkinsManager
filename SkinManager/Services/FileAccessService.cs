using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using LanguageExt;
using SharpCompress.Common;
using SharpCompress.Readers;
using static LanguageExt.Prelude;

namespace SkinManager.Services;

public static class FileAccessService{
    private static readonly JsonSerializerOptions Options = new(){ WriteIndented = true };

    public static async Task<Fin<bool>> ApplySkinAsync(string skinDirectoryName, string gameDirectoryName){
        try{
            foreach (DirectoryInfo currentFolder in new DirectoryInfo(skinDirectoryName).GetDirectories("*.*",
                         SearchOption.AllDirectories).Where(theDirectory => theDirectory.Name != "Screenshots")){
                string dirPath = string.Empty;
                DirectoryInfo? enumeratingParent = currentFolder.Parent;
                while (enumeratingParent is not null && enumeratingParent.FullName != skinDirectoryName){
                    dirPath =  Path.Combine(dirPath,enumeratingParent.Name);
                    enumeratingParent = enumeratingParent.Parent;
                }
                foreach (FileInfo theFile in currentFolder.GetFiles()){
                    string destinationPath = Path.Combine(gameDirectoryName, dirPath,currentFolder.Name, theFile.Name);
                    File.Copy(theFile.FullName, destinationPath, true);
                }
            }

            return true;
        }
        catch (Exception ex){
            return Fin.Fail<bool>(ex);
        }
    }

    private static Fin<bool> CreateFolder(string directoryName){
        if (!Directory.Exists(directoryName)){
            try{
                Directory.CreateDirectory(directoryName);
            }
            catch (Exception ex){
                return Fin.Fail<bool>(ex);
            }
        }

        return true;
    }

    public static async Task<Fin<bool>> CreateBackUpAsync(string skinDirectoryName, string backUpDirectoryName,
        string gameDirectoryName){
        try{
            DirectoryInfo skinDirectory = new(skinDirectoryName);
            DirectoryInfo[] subDirs = skinDirectory.GetDirectories("*.*", SearchOption.AllDirectories);

            foreach (DirectoryInfo currentFolder in subDirs.Where(theFolder => theFolder.Name != "Screenshots")){
                string dirPath = string.Empty;
                DirectoryInfo? enumeratingParent = currentFolder.Parent;
                while (enumeratingParent is not null && enumeratingParent.Name != skinDirectory.Name){
                    dirPath =  Path.Combine(dirPath,enumeratingParent.Name);
                    enumeratingParent = enumeratingParent.Parent;
                }
                string newBackUpDirectoryName = Path.Combine(backUpDirectoryName, dirPath,currentFolder.Name);
                if (CreateFolder(newBackUpDirectoryName)){
                    foreach (FileInfo theFile in currentFolder.GetFiles()){
                        string backupFileName = Path.Combine(newBackUpDirectoryName, theFile.Name);
                        string originalFileName =
                            Path.Combine(gameDirectoryName, dirPath, currentFolder.Name, theFile.Name);
                        if (File.Exists(originalFileName) && !File.Exists(backupFileName)){
                            File.Copy(originalFileName, backupFileName, false);
                        }
                        else{
                            FileStream originalFileStream = File.Exists("FilesToDelete.txt")
                                ? File.OpenWrite("FilesToDelete.txt")
                                : File.Create("FilesToDelete.txt");

                            await using StreamWriter writer = new StreamWriter(originalFileStream);
                            await writer.WriteLineAsync(originalFileName);
                            writer.Close();
                        }
                    }
                }
            }

            return true;
        }
        catch (Exception ex){
            return Fin.Fail<bool>(ex);
        }
    }

    public static async Task<Fin<bool>> RestoreBackupAsync(string backUpDirectoryName, string gameDirectoryName){
        try{
            DirectoryInfo backupDirectory = new(backUpDirectoryName);
            IEnumerable<DirectoryInfo> subDirs = await Task.Run(() => backupDirectory
                .GetDirectories("*.*", SearchOption.AllDirectories));

            await Task.Run(async () => {
                foreach (DirectoryInfo currentFolder in subDirs){
                    foreach (FileInfo theFile in currentFolder.GetFiles()){
                        string gameFileName = Path.Combine(gameDirectoryName, currentFolder.Name, theFile.Name);
                        if (File.Exists(gameFileName)){
                            File.Copy(theFile.FullName, gameFileName, true);
                        }
                    }
                }

                string filesToDelete = "FilesToDelete.txt";
                if (File.Exists(filesToDelete)){
                    using StreamReader reader = new StreamReader(filesToDelete);
                    while (!reader.EndOfStream){
                        File.Delete((await reader.ReadLineAsync())!);
                    }

                    File.Delete(filesToDelete);
                }
            });

            return true;
        }
        catch (Exception ex){
            return Fin.Fail<bool>(ex);
        }
    }

    public static Fin<bool> StartGame(string fileLocation){
        if (File.Exists(fileLocation)){
            ProcessStartInfo psInfo = new(){
                FileName = Path.Combine(fileLocation),
                Verb = "runas",
                UseShellExecute = true
            };

            try{
                Process.Start(psInfo);
            }
            catch (Exception ex){
                return Fin.Fail<bool>(ex);
            }
        }
        else{
            return Fin.Fail<bool>($"The {fileLocation} does not exist.");
        }

        return true;
    }

    public static async Task<Fin<bool>> ExtractSkinAsync(string archivePath, string destinationLocation){
        if (File.Exists(archivePath)){
            try{
                if (!Directory.Exists(destinationLocation)) Directory.CreateDirectory(destinationLocation);
                //await Task.Run(() => ZipFile.ExtractToDirectory(archivePath, destinationLocation));
                await using (Stream stream = File.OpenRead(archivePath))
                using (var reader = ReaderFactory.Open(stream)){
                    reader.WriteAllToDirectory(
                        destinationLocation,
                        new ExtractionOptions(){ ExtractFullPath = true, Overwrite = true });
                }

                return true;
            }
            catch (Exception ex){
                return Fin.Fail<bool>(ex);
            }
        }
        else{
            return Fin.Fail<bool>($"The archive {archivePath} does not exist.");
        }
    }

    public static async Task<Fin<T>> LoadJsonToObject<T>(string fileName){
        try{
            if (File.Exists(fileName)){
                await using Stream fileStream = File.OpenRead(fileName);
                return await JsonSerializer.DeserializeAsync<T>(fileStream) switch{
                    { } objectInfo => objectInfo,
                    _ => Fin.Fail<T>("There was an issue reading the file.")
                };
            }

            return Fin.Fail<T>($"The {typeof(T)} file {fileName} does not exist.");
        }
        catch (Exception ex){
            return Fin.Fail<T>(ex);
        }
    }

    public static async Task<IEnumerable<Fin<T>>> LoadJsonsToObjects<T>(IEnumerable<string> fileNames){
        List<Fin<T>> results = [];
        foreach (var fileName in fileNames){
            try{
                if (File.Exists(fileName)){
                    await using Stream fileStream = File.OpenRead(fileName);
                    results.Add(await JsonSerializer.DeserializeAsync<T>(fileStream) switch{
                        { } objectInfo => Fin.Succ<T>(objectInfo),
                        _ => Fin.Fail<T>("There was an issue reading the file.")
                    });
                }

                results.Add(Fin.Fail<T>($"The {typeof(T)} file {fileName} does not exist."));
            }
            catch (Exception ex){
                results.Add(Fin.Fail<T>(ex));
            }
        }

        return results;
    }

    public static async Task<Fin<ImmutableList<T>>> LoadJsonToImmutableList<T>(string fileName){
        try{
            if (File.Exists(fileName)){
                await using Stream fileStream = File.OpenRead(fileName);
                return await JsonSerializer.DeserializeAsync<IEnumerable<T>>(fileStream) switch{
                    { } collection => Fin.Succ<ImmutableList<T>>([..collection]),
                    _ => Fin.Succ<ImmutableList<T>>([])
                };
            }
            else{
                return Fin.Fail<ImmutableList<T>>($"The {typeof(T)} file {fileName} does not exist.");
            }
        }
        catch (Exception ex){
            return Fin.Fail<ImmutableList<T>>(ex);
        }
    }

    public static async Task<Fin<bool>> SaveObjectToJson<T>(T objectToSave, string fileName){
        try{
            await using Stream fileStream = File.Open(fileName, FileMode.Create);
            await JsonSerializer.SerializeAsync(fileStream, objectToSave,
                Options);

            return true;
        }
        catch (Exception ex){
            return Fin.Fail<bool>(ex);
        }
    }

    public static async Task<Fin<bool>> SaveIEnumerableToJson<T>(IEnumerable<T> collection, string fileName){
        try{
            await using Stream fileStream = File.Open(fileName, FileMode.Create);
            await JsonSerializer.SerializeAsync(fileStream, collection,
                Options);

            return true;
        }
        catch (Exception ex){
            return Fin.Fail<bool>(ex);
        }
    }

    public static Fin<IEnumerable<string>> SaveScreenshots(string path, IEnumerable<(Bitmap Screenshot, string FileExtension)> screenshots){
        try{
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            
            return screenshots.Aggregate(new List<string>(), (currentScreenshotLinks, currentScreenshot) => {
                string newPath = Path.Combine(path, $"Screenshot {currentScreenshotLinks.Count + 1}{currentScreenshot.FileExtension}");
                currentScreenshot.Screenshot.Save(newPath);
                return [..currentScreenshotLinks, newPath];
            });
        }
        catch (Exception ex){
            return Fin.Fail<IEnumerable<string>>(ex);
        }
    }

    public static Fin<bool> FolderHasFiles(string folderPath){
        if (Directory.Exists(folderPath)){
            try{
                return new DirectoryInfo(folderPath).EnumerateFiles(".", SearchOption.AllDirectories).Any();
            }
            catch (Exception ex){
                return Fin.Fail<bool>(ex);
            }
        }

        return Fin.Fail<bool>("Folder does not exist.");
    }
}