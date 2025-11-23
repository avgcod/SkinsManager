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
    public static async Task<Fin<string>> DownloadSkin(HttpClient httpClient, WebSkin theSkin, string skinsLocation){
        try{
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

    public static async Task<IEnumerable<WebSkin>> GetWebSkins(HttpClient theClient, IEnumerable<AddressBook> addressBooks){
        List<WebSkin> newSkins = [];
        foreach (var currentAddressBook in addressBooks){
            if (currentAddressBook.Source == SkinsSource.Ephinea){
                newSkins.AddRange(await GetEphineaSkins(theClient, currentAddressBook));
            }
            else{
                newSkins.AddRange(await GetUniversePsSkins(theClient));
            }
        }

        return newSkins;
    }
    
    private static async Task<IEnumerable<WebSkin>> GetEphineaSkins(HttpClient theClient, AddressBook addressBook){
        var skinTypes = await EphineaService.GetAvailableSkinTypes(theClient, addressBook.Main, addressBook.Base);
        List<SkinSiteInformation> skinSiteInfos = [];
        foreach (var currentSkinTypeSiteInfo in skinTypes){
            skinSiteInfos.AddRange(await EphineaService.GetAvailableSkins(theClient, addressBook.Base,
                currentSkinTypeSiteInfo.Address, currentSkinTypeSiteInfo.Name));
        }

        List<WebSkin> skins = [];
        foreach (var skinSiteInformation in skinSiteInfos){
            skins.AddRange(await EphineaService.GetSkin(theClient, skinSiteInformation.Address, addressBook.BaseScreenshots, skinSiteInformation.SkinType,  skinSiteInformation.SkinSubType));
        }

        return skins;
        /*return await (await skinTypes.SelectManyAsync(async currentSkinTypeSiteInfo
            => await EphineaService.GetAvailableSkins(theClient, addressBook.Base,
                currentSkinTypeSiteInfo.Address, currentSkinTypeSiteInfo.Name)))
            .SelectAsync(async currentWebSkinSiteInfo
                => await EphineaService.GetSkin(theClient, currentWebSkinSiteInfo.Address, addressBook.BaseScreenshots, currentWebSkinSiteInfo.SkinType,  currentWebSkinSiteInfo.SkinSubType));*/
    }
    private static async Task<IEnumerable<WebSkin>> GetUniversePsSkins(HttpClient theClient) => await UniversePsWebAccessService.GetAvailableSkinsAsync(theClient);
}