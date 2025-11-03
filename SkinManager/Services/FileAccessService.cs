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
using static LanguageExt.Prelude;

namespace SkinManager.Services;

public static class FileAccessService{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions(){ WriteIndented = true };

    public static async Task<Fin<bool>> ApplySkinAsync(string skinDirectoryName, string gameDirectoryName){
        try{
            await Task.Run(() => {
                foreach (DirectoryInfo folder in new DirectoryInfo(skinDirectoryName).GetDirectories("*.*",
                             SearchOption.AllDirectories)){
                    foreach (FileInfo theFile in folder.GetFiles()){
                        string destinationPath = Path.Combine(gameDirectoryName, folder.Name, theFile.Name);
                        File.Copy(theFile.FullName, destinationPath, true);
                    }
                }
            });

            return true;
        }
        catch (Exception ex){
            return Fin<bool>.Fail(ex);
        }
    }

    private static async Task<Fin<bool>> CreateFolderAsync(string directoryName){
        if (!await DirectoryExists(directoryName).RunAsync()){
            try{
                await Task.Run(() => Directory.CreateDirectory(directoryName));
            }
            catch (Exception ex){
                return Fin<bool>.Fail(ex);
            }
        }

        return true;
    }

    public static async Task<Fin<bool>> CreateBackUpAsync(string skinDirectoryName, string backUpDirectoryName,
        string gameDirectoryName){
        try{
            DirectoryInfo skinDirectory = new(skinDirectoryName);
            DirectoryInfo[] subDirs =
                await Task.Run(() => skinDirectory.GetDirectories("*.*", SearchOption.AllDirectories));

            await Task.Run(async () => {
                foreach (DirectoryInfo currentFolder in subDirs){
                    string newBackUpDirectoryName = Path.Combine(backUpDirectoryName, currentFolder.Name);
                    if (await CreateFolderAsync(newBackUpDirectoryName)){
                        foreach (FileInfo theFile in currentFolder.GetFiles()){
                            string backupFileName = Path.Combine(newBackUpDirectoryName, theFile.Name);
                            string originalFileName =
                                Path.Combine(gameDirectoryName, currentFolder.Name, theFile.Name);
                            if (File.Exists(originalFileName)){
                                File.Copy(originalFileName, backupFileName, false);
                            }
                            else{
                                FileStream originalFileStream = await FileExists("FilesToDelete.txt").RunAsync()
                                    ? File.OpenWrite(originalFileName)
                                    : File.Create(backupFileName);

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
        catch (Exception ex){
            return Fin<bool>.Fail(ex);
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
                        if (await FileExists(gameFileName).RunAsync()){
                            File.Copy(theFile.FullName, gameFileName, true);
                        }
                    }
                }

                string filesToDelete = "FilesToDelete.txt";
                if (await FileExists(filesToDelete).RunAsync()){
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
            return Fin<bool>.Fail(ex);
        }
    }

    private static IO<bool> FileExists(string fileName) =>
        liftIO(async () => await Task.Run(() => File.Exists(fileName)));

    private static IO<bool> DirectoryExists(string directoryName) =>
        liftIO(async () => await Task.Run(() => Directory.Exists(directoryName)));

    public static async Task<Fin<bool>> StartGameAsync(string fileLocation){
        if (await FileExists(fileLocation).RunAsync()){
            ProcessStartInfo psInfo = new(){
                FileName = Path.Combine(fileLocation),
                Verb = "runas",
                UseShellExecute = true
            };

            try{
                await Task.Run(() => Process.Start(psInfo));
            }
            catch (Exception ex){
                return Fin<bool>.Fail(ex);
            }
        }

        return true;
    }

    public static async Task<Fin<bool>> ExtractSkinAsync(string archivePath, string destinationLocation){
        if (await FileExists(archivePath).RunAsync()){
            try{
                Directory.CreateDirectory(destinationLocation);
                await Task.Run(() => ZipFile.ExtractToDirectory(archivePath, destinationLocation));
                return true;
            }
            catch (Exception ex){
                return Fin<bool>.Fail(ex);
            }
        }
        else{
            return Fin<bool>.Fail("The archive does not exist.");
        }
    }

    public static async Task<Fin<Option<T>>> LoadJsonToObject<T>(string fileName){
        try{
            if (await FileExists(fileName).RunAsync()){
                await using Stream fileStream = File.OpenRead(fileName);
                return await JsonSerializer.DeserializeAsync<T>(fileStream) switch{
                    { } objectInfo => Option<T>.Some(objectInfo),
                    _ => Option<T>.None
                };
            }

            return Fin<Option<T>>.Fail("The file does not exist.");
        }
        catch (Exception ex){
            return Fin<Option<T>>.Fail(ex);
        }
    }

    public static async Task<Fin<ImmutableList<T>>> LoadJsonToIEnumerable<T>(string fileName){
        try{
            if (await FileExists(fileName).RunAsync()){
                await using Stream fileStream = File.OpenRead(fileName);
                return await JsonSerializer.DeserializeAsync<IEnumerable<T>>(fileStream) switch{
                    { } collection => Fin<ImmutableList<T>>.Succ([..collection]),
                    _ => Fin<ImmutableList<T>>.Succ([])
                };
            }
            else{
                return Fin<ImmutableList<T>>.Fail("The file does not exist.");
            }
        }
        catch (Exception ex){
            return Fin<ImmutableList<T>>.Fail(ex);
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
            return Fin<bool>.Fail(ex);
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
            return Fin<bool>.Fail(ex);
        }
    }

    public static async Task<Fin<bool>> SaveScreenshotsAsync(string path, IEnumerable<string> screenshots){
        int screenshotNumber = 1;
        try{
            Directory.CreateDirectory(path);
            foreach (var screenshot in screenshots){
                Option<Bitmap> possibleScreenShotBitmap = screenshot.Contains("http") switch{
                    true => await ImageHelperService.LoadFromWeb(new Uri(screenshot)) is {} screenshotBitmap ? Option<Bitmap>.Some(screenshotBitmap) : Option<Bitmap>.None,
                    _ => Option<Bitmap>.Some(ImageHelperService.LoadFromResource(new Uri(screenshot)))
                };

                    possibleScreenShotBitmap.IfSome(screenshotBitmap => screenshotBitmap.Save(Path.Combine(path, $"Screenshot {screenshotNumber}.{
                        screenshot.Split(".").Last()}")));

                screenshotNumber++;
            }

            return true;
        }
        catch (Exception ex){
            return Fin<bool>.Fail(ex);
        }
    }

    public static async Task<Fin<bool>> FolderHasFilesAsync(string folderPath){
        if (await DirectoryExists(folderPath).RunAsync()){
            try{
                return await Task.Run(() =>
                    new DirectoryInfo(folderPath).EnumerateFiles(".", SearchOption.AllDirectories).Any());
            }
            catch (Exception ex){
                return Fin<bool>.Fail(ex);
            }
        }

        return Fin<bool>.Fail("Folder does not exist.");
    }
}