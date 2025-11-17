using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using SkinManager.Types;
using LanguageExt;
using SkinManager.Extensions;

namespace SkinManager.Services;

public static class WebAccessService{
    public static async Task<Fin<string>> DownloadSkin(HttpClient httpClient, WebSkin theSkin, int downloadLinkNumber, string skinsLocation){
        try{
            /*string downloadLink = theSkin.DownloadLinks.First(currentLink => !currentLink.Contains("Revert", StringComparison.OrdinalIgnoreCase));
            HttpResponseMessage response =
                await httpClient.GetAsync(downloadLink);

            string fileExtension = response.RequestMessage?.RequestUri?.OriginalString.Split('.').Last() ??
                                   string.Empty;

            return (await httpClient.GetStreamAsync(downloadLink), fileExtension);*/
            string downloadLink = theSkin.DownloadLinks.First(currentLink => !currentLink.Contains("Revert", StringComparison.OrdinalIgnoreCase));
            HttpResponseMessage response =
                await httpClient.GetAsync(downloadLink);

            string fileExtension = response.RequestMessage?.RequestUri?.OriginalString.Split('.').Last() ??
                                   string.Empty;

            string filePath = Path.Combine(skinsLocation, $"{theSkin.SkinName.RemoveSpecialCharacters()}.{fileExtension}");

            if (File.Exists(filePath)){
                return filePath;
            }
            else{
                await using Stream webStream = await httpClient.GetStreamAsync(downloadLink);
                await using Stream skinFileStream = File.Create(filePath);
                await webStream.CopyToAsync(skinFileStream);
                return filePath;
            }
        }
        catch (Exception ex){
            return Fin.Fail<string>(ex);
        }
    }

    public static async Task<IEnumerable<WebSkin>> GetWebSkins(HttpClient theClient, AddressBook addressBook){
        if (addressBook.Source == SkinsSource.Ephinea){
            return await GetEphineaSkins(theClient, addressBook);
        }
        else{
            return await GetUniversePsSkins(theClient);
        }
    }
    
    private static async Task<IEnumerable<WebSkin>> GetEphineaSkins(HttpClient theClient, AddressBook addressBook){
        var skinTypes = await EphineaService.GetAvailableSkinTypes(theClient, addressBook.Main, addressBook.Base);

        return await (await skinTypes.SelectManyAsync(async currentSkinTypeSiteInfo
            => await EphineaService.GetAvailableSkins(theClient, addressBook.Base,
                currentSkinTypeSiteInfo.Address, currentSkinTypeSiteInfo.Name)))
            .SelectAsync(async currentWebSkinSiteInfo
                => await EphineaService.GetSkin(theClient, currentWebSkinSiteInfo.Address, addressBook.BaseScreenshots, currentWebSkinSiteInfo.SkinType,  currentWebSkinSiteInfo.SkinSubType));
    }
    private static async Task<IEnumerable<WebSkin>> GetUniversePsSkins(HttpClient theClient) => await UniversePsWebAccessService.GetAvailableSkinsAsync(theClient);
}