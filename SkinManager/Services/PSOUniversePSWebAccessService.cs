using HtmlAgilityPack;
using SkinManager.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace SkinManager.Services
{
    public class PSOUniversePSWebAccessService
    {
        private readonly Dictionary<SkinType, string> skinTypeAddresses = [];
        private readonly HttpClient _httpClient;
        public List<SkinType> SkinTypes { get; set; } = [];


        #region Web Address Strings
        private const string baseSiteAddress = "http://universps.online.fr/pso/bb/skin/listeSkinUS.php5";
        private const string baseScreenshotsAddress = "http://universps.online.fr/pso/bb/skin/";
        private const string baseDownloadAddress = "http://universps.online.fr/pso/bb/skin/";
        private const string areaSkinsAddress = "?cat=17";
        //private const string magSkinsAddress = "?cat=36";
        private const string packSkinsAddress = "?cat=36";
        private const string humarSkinsAddress = "?cat=1";
        private const string hunewearlSkinsAddress = "?cat=2";
        private const string hucastSkinsAddress = "?cat=3";
        private const string hucasealSkinsAddress = "?cat=4";
        private const string ramarSkinsAddress = "?cat=5";
        private const string ramarlSkinsAddress = "?cat=6";
        private const string racastSkinsAddress = "?cat=7";
        private const string racasealSkinsAddress = "?cat=8";
        private const string fomarSkinsAddress = "?cat=9";
        private const string fomarlSkinsAddress = "?cat=10";
        private const string fonewmSkinsAddress = "?cat=11";
        private const string fonewearlSkinsAddress = "?cat=12";
        private const string ninjaSkinsAddress = "?cat=22";
        private const string ricoSkinsAddress = "?cat=23";
        private const string sonicSkinsAddress = "?cat=19";
        private const string knucklesSkinsAddress = "?cat=21";
        private const string tailsSkinsAddress = "?cat=20";
        private const string flowenSkinsAddress = "?cat=25";
        private const string ellySkinsAddress = "?cat=24";
        private const string momokaSkinsAddress = "?cat=26";
        private const string ireneSkinsAddress = "?cat=29";
        private const string guildHostessSkinsAddress = "?cat=27";
        private const string enemySkinsAddress = "?cat=14";
        private const string npcSkinsAddress = "?cat=32";
        private const string titleScreenSkinsAddress = "?cat=34";
        private const string hudSkinsAddress = "?cat=16";
        private const string textSkinsAddress = "?cat=33";
        private const string objectSkinsAddress = "?cat=15";
        private const string effectSkinsAddress = "?cat=18";
        private const string otherSkinsAddress = "?cat=30";
        #endregion

        public PSOUniversePSWebAccessService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            skinTypeAddresses.Add(new() { Name = "Area", SubTypes = [] }, areaSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Pack", SubTypes = ["Other"] }, packSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Class", SubTypes = ["Humar"] }, humarSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Class", SubTypes = ["Hunewearl"] }, hunewearlSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Class", SubTypes = ["Hucast"] }, hucastSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Class", SubTypes = ["Hucaseal"] }, hucasealSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Class", SubTypes = ["Ramar"] }, ramarSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Class", SubTypes = ["Ramarl"] }, ramarlSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Class", SubTypes = ["Racast"] }, racastSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Class", SubTypes = ["Racaseal"] }, racasealSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Class", SubTypes = ["Fomar"] }, fomarSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Class", SubTypes = ["Fomarl"] }, fomarlSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Class", SubTypes = ["Fonewm"] }, fonewmSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Class", SubTypes = ["Fonewearl"] }, fonewearlSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "NPC", SubTypes = ["Ninja"] }, ninjaSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "NPC", SubTypes = ["Rico"] }, ricoSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "NPC", SubTypes = ["Sonic"] }, sonicSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "NPC", SubTypes = ["Knuckles"] }, knucklesSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "NPC", SubTypes = ["Tails"] }, tailsSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "NPC", SubTypes = ["Flowen"] }, flowenSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "NPC", SubTypes = ["Elly"] }, ellySkinsAddress);
            skinTypeAddresses.Add(new() { Name = "NPC", SubTypes = ["Momoka"] }, momokaSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "NPC", SubTypes = ["Irene"] }, ireneSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "NPC", SubTypes = ["Guild"] }, guildHostessSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Enemy", SubTypes = [] }, enemySkinsAddress);
            skinTypeAddresses.Add(new() { Name = "NPC", SubTypes = ["Other"] }, npcSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "UI", SubTypes = ["Title Screen"] }, titleScreenSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "UI", SubTypes = ["HUD"] }, hudSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "UI", SubTypes = ["Text"] }, textSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Object", SubTypes = ["Other"] }, objectSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Effect", SubTypes = ["Attack", "Buff", "Healing", "Other"] }, effectSkinsAddress);
            skinTypeAddresses.Add(new() { Name = "Other", SubTypes = ["Misc"] }, otherSkinsAddress);
        }

        public IEnumerable<Skin> GetAvailableSkins()
        {
            return GetAvailableSkinsAsync().Result;
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

            HtmlDocument theDoc = new HtmlDocument();

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
