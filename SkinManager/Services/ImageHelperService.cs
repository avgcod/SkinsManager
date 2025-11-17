using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    //Copied from https://docs.avaloniaui.net/ru/docs/guides/data-binding/how-to-bind-image-files
    public static class ImageHelperService
    {
        public static Bitmap LoadFromResource(string filePath){
            try{
                return new Bitmap(File.OpenRead(filePath));
            }
            catch (Exception ex){
                Console.WriteLine($"An error occurred while trying to load image '{filePath}' : {ex.Message}");
                return null;
            }
            
            //return new Bitmap(AssetLoader.Open(resourceUri));
        }
        public static async Task<Bitmap?> LoadFromWeb(Uri url)
        {
            using var httpClient = new HttpClient();
            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsByteArrayAsync();
                return new Bitmap(new MemoryStream(data));
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"An error occurred while downloading image '{url}' : {ex.Message}");
                return null;
            }
        }
    }
}
