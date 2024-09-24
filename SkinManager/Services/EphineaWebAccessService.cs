using HtmlAgilityPack;
using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public class EphineaWebAccessService
    {
        private readonly Dictionary<SkinType, string> skinTypeAddresses = [];
        private readonly HttpClient _httpClient;
        public List<SkinType> SkinTypes { get; set; } = [];

        #region Web Address Strings
        private const string baseSiteAddress = "https://wiki.pioneer2.net/w/Skin:";
        private const string baseScreenshotsAddress = "https://wiki.pioneer2.net/w/File:";
        private const string baseDownloadAddress = "https://wiki.pioneer2.net/files/";
        private const string areaSkinsAddress = "Areas";
        private const string classesSkinsAddress = "Classes";
        private const string monstersSkinsAddress = "Monsters";
        private const string titleScreenSkinsAddress = "Title_screen";
        private const string hudSkinsAddress = "HUD";
        private const string objectSkinsAddress = "Objects";
        private const string effectSkinsAddress = "Effects";
        private const string audioSkinsAdress = "Audio";
        private const string equipmentSkinsAddress = "Equipment";
        private const string npcsSkinsAddress = "NPCs";
        private const string unitxtSkinsAddress = "Unitxt";
        #endregion

        public EphineaWebAccessService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            skinTypeAddresses.Add(new() { Name = "Area", SubTypes = [] }, areaSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Class", SubTypes = [] }, classesSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Enemy", SubTypes = [] }, monstersSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "NPC", SubTypes = ["Other"] }, npcsSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "UI", SubTypes = ["Title Screen"] }, titleScreenSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "UI", SubTypes = ["HUD"] }, hudSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Object", SubTypes = ["Other"] }, objectSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Effect", SubTypes = ["Attack", "Buff", "Healing", "Other"] }, effectSkinsAddress);
        }

        public async Task<IEnumerable<Skin>> GetAvailableSkinsForSpecificTypeAsync(string skinTypeName)
        {
            SkinType currentType = skinTypeAddresses.First(x => x.Key.Name.Equals(skinTypeName, StringComparison.OrdinalIgnoreCase)).Key;
            return await GetSkinsFromWebsite(currentType, skinTypeAddresses[currentType]);
        }

        public async Task<IEnumerable<Skin>> GetAvailableSkinsAsync()
        {
            List<Skin> skins = [];

            foreach (KeyValuePair<SkinType, string> currentPair in skinTypeAddresses)
            {
                skins.AddRange(await GetSkinsFromWebsite(currentPair.Key, currentPair.Value));
            }

            return skins;
        }

        private async Task<IEnumerable<Skin>> GetSkinsFromWebsite(SkinType skinType, string address)
        {
            string response = await _httpClient.GetStringAsync(baseSiteAddress + address);

            HtmlDocument theDoc = new();

            theDoc.LoadHtml(response);

            //The html lists each author in a div with an attribute name with a value that starts with cat_.
            IEnumerable<HtmlNode> authors = theDoc.DocumentNode.SelectNodes("//div[@class='title1 titreCategorie']");

            //The html lists skins for each author in a tbody under the respective author div but not as a child of the author div.
            IEnumerable<HtmlNode> skinTables = theDoc.DocumentNode.SelectNodes("//tbody[@class='contenuCategorieSkin']");

            IEnumerator<HtmlNode> skinTablesEnumerator = skinTables.GetEnumerator();

            string currentAuthor = string.Empty;
            string currentSkinName = string.Empty;
            string currenSkinAddDate = string.Empty;
            string currentSkinDownloadLink = string.Empty;
            List<Skin> foundSkins = [];
            List<string> currenSkinScreenshots = [];

            foreach (HtmlNode authorNode in authors)
            {
                //The html lists the author's name in the div attribute name with a value that starts with cat_.
                currentAuthor = authorNode.GetAttributeValue("name", "").Replace("cat_", string.Empty);

                if (skinTablesEnumerator.MoveNext())
                {
                    //The html lists skins in a table with one tbody housing all skins in separate rows.
                    //The table is not a child of the author div so it has to be handled as its own node but the number of author and skin tables are 1:1.
                    HtmlNode skinTableNodes = skinTablesEnumerator.Current;

                    //The html lists each skin row as a tr with attribute class with value ligneSkin.
                    IEnumerable<HtmlNode> skinRows = skinTableNodes.SelectNodes(".//tr[@class='ligneSkin']");

                    foreach (HtmlNode skinRow in skinRows)
                    {
                        //The html lists the skin name in the inner text of an a node with the attribute href that has a value that starts with #skin_.
                        //The first td of row is not closed causing the nodes to not be properly linked.
                        currentSkinName = skinRow.SelectSingleNode(".//a[contains(@href,'#skin_')]")?.InnerText.Trim() ?? string.Empty;

                        //The html lists the skin date added in a text field after the 3rd b node.
                        currenSkinAddDate = skinRow.SelectSingleNode(".//b[contains(text(),'Date')]")?.NextSibling?.InnerText.Trim() ?? string.Empty;

                        //The html lists the skin download line an a node with an attribute href with a value that contains telechargement and ends in t=d.
                        currentSkinDownloadLink = skinRow.SelectSingleNode(".//a[contains(@href,'telechargement') and contains(@href,'&t=d')]")?.GetAttributeValue("href", string.Empty) ?? string.Empty;

                        currentSkinDownloadLink = string.Concat(baseDownloadAddress, currentSkinDownloadLink);

                        //The html lists the full size skin screenshot(s) in the second row in a nodes with an href attribute that contains imageSkin/.
                        //IEnumerable<HtmlNode> currentSkinScreenshotNodes = skinRow.SelectNodes(".//td/a[@class='highslide' and contains(@href,'imageSkin/')]");

                        //The html lists the mini skin screenshot(s) in the second row in img nodes with a src attribute that contains imageSkin/mini.
                        IEnumerable<HtmlNode> currentSkinScreenshotNodes = skinRow.SelectNodes(".//img[contains(@src,'imageSkin/mini')]");

                        if (currentSkinScreenshotNodes is not null)
                        {
                            currenSkinScreenshots = [];
                            foreach (HtmlNode screenShotNode in currentSkinScreenshotNodes)
                            {
                                currenSkinScreenshots.Add(string.Concat(baseScreenshotsAddress, screenShotNode.Attributes["src"].Value));
                            }
                        }

                        string currentSkinSubType = GetSkinSubType(skinType, currentSkinName);
                        Skin tempSkin = new()
                        {
                            Author = currentAuthor,
                            CreationDate = DateOnly.ParseExact(currenSkinAddDate, "dd/MM/yyyy"),
                            Description = $"{skinType.Name} {currentSkinSubType} skin {currentSkinName}.",
                            IsWebSkin = true,
                            LastUpdatedDate = DateOnly.ParseExact(currenSkinAddDate, "dd/MM/yyyy"),
                            Name = currentSkinName,
                            Location = currentSkinDownloadLink,
                            Screenshots = currenSkinScreenshots,
                            SkinType = skinType,
                            SubType = currentSkinSubType
                        };
                        foundSkins.Add(tempSkin);
                    }
                }
            }
            return foundSkins;
        }
        private string GetSkinSubType(SkinType tempSkinType, string skinName)
        {
            if (tempSkinType.Name == "Area" || tempSkinType.Name == "Pack" || tempSkinType.Name == "NPC"
        || tempSkinType.Name == "Effect" || tempSkinType.Name == "Object" || tempSkinType.Name == "Enemy")
            {
                foreach (string currentSubType in SkinTypes.Single(x => x.Name == tempSkinType.Name).SubTypes)
                {
                    if (skinName.Contains(currentSubType, StringComparison.OrdinalIgnoreCase))
                    {
                        return currentSubType;
                    }
                }

                if (tempSkinType.Name == "Area")
                {
                    return SkinTypes.Single(x => x.Name == tempSkinType.Name).SubTypes[0];
                }
                else
                {
                    return "Unknown";
                }
            }
            else
            {
                return tempSkinType.SubTypes[0];
            }
        }
    }
}
