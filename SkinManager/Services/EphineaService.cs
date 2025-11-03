using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using SkinManager.Extensions;
using SkinManager.Types;

namespace SkinManager.Services;

public static class EphineaService{
    public static async Task<ImmutableList<SkinTypeSiteInformation>> GetAvailableSkinTypes(HttpClient theClient,
        string skinTypesAddress, string baseSkinAddress){
        string mainPageResponse = await theClient.GetStringAsync(skinTypesAddress);

        HtmlDocument mainPageDoc = new();

        mainPageDoc.LoadHtml(mainPageResponse);

        /*
        The html lists all the available skin types in a <ul> tag with each type in it's own <li><a> </a></li> grouping
        The <a> tag has an href that starts with '/w/Skin:' that when combined with the base address links to the
        category page.
        */
        IEnumerable<HtmlNode> skinCategoryNodes =
            mainPageDoc.DocumentNode.SelectNodes(
                "//ul/li/a[contains(@href,'/w/Skin:') and contains(@title,'Skin:')]") ?? Enumerable.Empty<HtmlNode>();

        var foundCategories = skinCategoryNodes
            .Select(currentCategoryNode => new SkinTypeSiteInformation(currentCategoryNode.InnerText,
                string.Concat(baseSkinAddress, currentCategoryNode.GetAttributeValue("href", string.Empty))))
            .Where(result => result.Name != string.Empty)
            .Select(result => result with{
                Name = result.Name.Replace("\n", string.Empty).NormalizeWhiteSpace().ToTitleCase()
            }).ToImmutableList();

        return foundCategories;
    }

    public static async Task<ImmutableList<SkinSiteInformation>> GetAvailableSkins(HttpClient theClient,
        string baseAddress, string skinTypeAddress, string currentType){
        string mainPageResponse = await theClient.GetStringAsync(skinTypeAddress);

        HtmlDocument mainPageDoc = new();

        mainPageDoc.LoadHtml(mainPageResponse);

        //The html lists all skins on the same page in a table with the class 'wikitable'.
        IEnumerable<HtmlNode> skinTableNodes =
            mainPageDoc.DocumentNode.SelectNodes("//table[@class='wikitable']") ?? Enumerable.Empty<HtmlNode>();

        //The html lists all skins on the same page each in there own table with the class 'wikitable' with a <td><a> grouping.
        //IEnumerable<HtmlNode> skinRowNodes =
        //  mainPageDoc.DocumentNode.SelectNodes("//td/a[contains(@href,'/w/Skin:') and contains(@title,'Skin:')]")
        //    .Where(currentNode => currentNode.GetAttributeValue("title",string.Empty).Contains(currentNode.InnerText)) ?? Enumerable.Empty<HtmlNode>();

        string currentSkinName = string.Empty;
        string currentSkinSubTypeName = string.Empty;
        string currentSkinAddress = string.Empty;
        List<SkinSiteInformation> availableSkins = [];

        foreach (var skinTableNode in skinTableNodes){
            currentSkinName = skinTableNode.SelectSingleNode(".//td[@style='width:300px;']/a").InnerText;
            currentSkinSubTypeName = skinTableNode.SelectNodes("preceding-sibling::h3/span")?.Last().InnerText ?? "Not Specified";
            currentSkinAddress = string.Concat(baseAddress,
                skinTableNode.SelectSingleNode(".//td/a[contains(@href,'/w/Skin:') and contains(@title,'Skin:')]")
                    .GetAttributeValue("href", string.Empty));
            availableSkins.Add(new SkinSiteInformation(currentType, currentSkinSubTypeName, currentSkinName,
                currentSkinAddress));
        }

        return availableSkins.ToImmutableList();
    }

    public static async Task<WebSkin> GetSkin(HttpClient theClient, string skinAddress, string baseScreenshotsAddress,
        string skinType, string skinSubType){
        string mainPageResponse = await theClient.GetStringAsync(skinAddress);

        HtmlDocument mainPageDoc = new();

        mainPageDoc.LoadHtml(mainPageResponse);

        //The html has a table with rows for author, type and source.
        HtmlNode skinInfoNode =
            mainPageDoc.DocumentNode.SelectSingleNode(".//th[contains(text(), 'Author')]/../../..")!;
        string currentAuthor = skinInfoNode.SelectSingleNode(".//tr[3]/td[1]")!.InnerText;
        string skinName = mainPageDoc.DocumentNode.SelectSingleNode(".//tr/th").InnerText.NormalizeWhiteSpace();

        //The html has a td node(s) with an a node that contains an href attribute that starts with
        //https://wiki.pioneer2.net/files/ that contains the download link(s) for the skin.
        ImmutableList<string> currentDownloadLinks = mainPageDoc.DocumentNode
            .SelectNodes(".//td/a[contains(@href,'https://wiki.pioneer2.net/files/')]")
            !.Where(x => !x.Attributes["href"].Value
                .Contains("revert", StringComparison.OrdinalIgnoreCase))
            .Select(dlNode => dlNode.Attributes["href"].Value).ToImmutableList();

        //The html has a ul node with class gallery mw-gallery-nolines that contain img nodes with src
        //attributes that starts with /images/thumb/ which are links to the screenshots.
        //Using the thumb version to reduce bandwidth strain but this can be changed to the full versions.
        if (mainPageDoc.DocumentNode
                .SelectSingleNode("//ul[contains(@class, 'gallery mw-gallery-nolines')]")
                ?.SelectNodes(".//img[contains(@src, '/images/thumb/')]") is
            { Count: > 0 } currentScreenshotNodes){
            ImmutableList<string> currentScreenshots = currentScreenshotNodes.Select(imgNode =>
                string.Concat(baseScreenshotsAddress, imgNode.Attributes["src"].Value)).ToImmutableList();

            WebSkin newSkin = new
            (
                skinName, skinType, skinSubType, skinAddress, currentAuthor, currentDownloadLinks,
                currentScreenshots
            );

            return newSkin;
        }
        else{
            WebSkin newSkin = new
            (
                skinName, skinType, skinSubType, skinAddress, currentAuthor, currentDownloadLinks,
                []
            );

            return newSkin;
        }
    }
}