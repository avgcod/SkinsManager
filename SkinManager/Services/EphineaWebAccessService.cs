using HtmlAgilityPack;
using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkinManager.Services
{
    public class EphineaWebAccessService : ISkinsAccessService
    {
        private readonly Dictionary<SkinType, string> _skinTypeAddresses = [];
        private readonly HttpClient _httpClient;

        #region Web Address Strings

        private const string baseSiteAddress = "https://wiki.pioneer2.net/w/Skin:";
        private const string baseScreenshotsAddress = "https://wiki.pioneer2.net";
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
            Dictionary<SkinsSource, DateOnly> currentDate = [];
            currentDate.Add(SkinsSource.Ephinea, DateOnly.FromDateTime(DateTime.Now));

            _skinTypeAddresses.Add(SkinType.Create("Area", [
                "Multiple areas", "Lobby", "Pioneer 2",
                "Forest", "Caves", "VR Temple", "VR Spaceship", "Central Control Area", "Seabed", "Control Tower",
                "Crater Interior", "Subterranean Desert"
            ], currentDate), areaSkinsAddress);
            _skinTypeAddresses.Add(SkinType.Create("Audio", ["Other"], currentDate), audioSkinsAdress);
            _skinTypeAddresses.Add(SkinType.Create("Class", [
                "Humar", "Hucast", "Hunewearl", "Hucaseal",
                "Fomar", "Fomarl", "Fonewearl", "Ramar", "Racast", "Ramarl", "Racaseal"
            ], currentDate), classesSkinsAddress);
            _skinTypeAddresses.Add(SkinType.Create("Enemy", [
                "Booma", "Gobooma", "Gigobooma", "Rappy",
                "Al Rappy", "Rappy Family", "Monest", "Mothmant", "Savage Wolf", "Barbarouse Wolf", "Hildebear",
                "Hildeblue", "Hidelt", "Hildetorr", "Dargon", "Sil Dragon", "Evil Shark", "Pal Shark", "Guil Shark",
                "Poison Lily", "Nar Lily", "Grass Assassin", "Nano Dragon", "Pan Arms", "Hiddom", "Migium", "DelRoLe",
                "DelRalLie", "Gilchic", "Dubchic", "Canadine", "Canane", "Sinow Beat", "Sinow Gold", "Garanz",
                "Vol Opt", "Dimenian", "La Dimenian", "So Dimenian", "Claw", "Bulclaw", "Bulk", "Delsaber",
                "Dark Belra", "Chaos Sorcerer", "Dark Gunner", "Chaos Bringer", "Dark Falz", "Merillia", "Meritas",
                "Mericarol", "Ul Gibbon", "Zol Gibbon", "Gibbles", "Gee", "Gi Gue", "Sinow Berril", "Sinow Spigell",
                "Gol Dragon", "Gal Gryphon", "Domolm", "Dolmdari", "Recon", "Reconbox", "Sinow Zoa", "Sinow Zele",
                "Morfos", "Deldepth", "Delbiter", "Epsilon", "Olga Flow", "Sand Rappy", "Del Rappy", "Girtablublu",
                "Goran", "Pyro Goran", "Goran Detonator", "Merissa A", "Merissa AA", "Zu", "Pazuzu",
                "Satellite Lizard", "Yowie"
            ], currentDate), monstersSkinsAddress);
            _skinTypeAddresses.Add(SkinType.Create("NPC", ["Other"], currentDate), npcsSkinsAddress);
            _skinTypeAddresses.Add(SkinType.Create("Equipment", [
                "Armor", "Barrier", "Weapon",
                "Mag"
            ], currentDate), equipmentSkinsAddress);
            _skinTypeAddresses.Add(SkinType.Create("UI", ["Title Screen"], currentDate), titleScreenSkinsAddress);
            _skinTypeAddresses.Add(SkinType.Create("UI", ["HUD"], currentDate), hudSkinsAddress);
            _skinTypeAddresses.Add(SkinType.Create("Object", ["Other"], currentDate), objectSkinsAddress);
            _skinTypeAddresses.Add(SkinType.Create("Effect", ["Attack", "Buff", "Healing", "Other"], currentDate),
                effectSkinsAddress);
            _skinTypeAddresses.Add(SkinType.Create("Unitxt", ["Other"], currentDate), unitxtSkinsAddress);
            _skinTypeAddresses.Add(SkinType.Create("Other", ["Other"], currentDate), String.Empty);
        }

        public async Task<IEnumerable<Skin>> GetAvailableSkinsForSpecificTypeAsync(SkinType skinType)
        {
            SkinType currentType = _skinTypeAddresses
                .First(x => x.Key.Name.Equals(skinType.Name, StringComparison.OrdinalIgnoreCase)).Key;
            return await GetSkinsFromWebsite(currentType, _skinTypeAddresses[currentType]);
        }

        public async Task<IEnumerable<Skin>> GetAvailableSkinsAsync()
        {
            List<Skin> skins = [];

            foreach (KeyValuePair<SkinType, string> currentPair in _skinTypeAddresses)
            {
                skins.AddRange(await GetSkinsFromWebsite(currentPair.Key, currentPair.Value));
            }

            return skins;
        }

        private async Task<IEnumerable<Skin>> GetSkinsFromWebsite(SkinType webSkinType, string address)
        {
            if (webSkinType.Name == "Other")
            {
                return [];
            }
            
            try
            {
                List<Skin> foundSkins = [];
                string mainPageResponse = await _httpClient.GetStringAsync(baseSiteAddress + address);

                HtmlDocument mainPageDoc = new();

                mainPageDoc.LoadHtml(mainPageResponse);

                //The html lists all skins on the same page in a table with the class 'wikitable'.
                IEnumerable<HtmlNode> skinTableNodes =
                    mainPageDoc.DocumentNode.SelectNodes("//table[@class='wikitable']") ?? Enumerable.Empty<HtmlNode>();

                foreach (HtmlNode skinNode in skinTableNodes)
                {
                    //The html contains an a node with an href that starts with /w/Skin: that when combined with the base address links to the skins specific page
                    HtmlNode currentSkinNode = skinNode.SelectSingleNode(".//td/a[contains(@href,'/w/Skin:')]");

                    //The html contains a td node with an a node with attribute title that starts with Skin:
                    //That contains the skin name.
                    string currentSkinName = currentSkinNode.GetAttributeValue("title", string.Empty)
                        .Replace("Skin:", String.Empty);

                    if (currentSkinNode?.GetAttributeValue("href", string.Empty) is { Length : > 0 } currentSkinLink)
                    {
                        string skinPageResponse =
                            await _httpClient.GetStringAsync(baseSiteAddress +
                                                             currentSkinLink.Replace("/w/Skin:", string.Empty));

                        HtmlDocument skinPageDoc = new();

                        skinPageDoc.LoadHtml(skinPageResponse);

                        //The html has a table with rows for author, type and source.
                        HtmlNode skinInfoNode =
                            skinPageDoc.DocumentNode.SelectSingleNode(".//th[contains(text(), 'Author')]/../../..");
                        string currentAuthor = skinInfoNode.SelectSingleNode(".//tr[3]/td[1]").InnerText;
                        string webProvidedType = skinInfoNode.SelectSingleNode(".//tr[4]/td[1]").InnerText
                            .Replace(" skin", string.Empty);

                        //The html has a td node(s) with an a node that contains an href attribute that starts with
                        //https://wiki.pioneer2.net/files/ that contains the download link(s) for the skin.
                        IEnumerable<string> currentDownloadLinks = skinPageDoc.DocumentNode
                            .SelectNodes(".//td/a[contains(@href,'https://wiki.pioneer2.net/files/')]")
                            .Where(x => !x.Attributes["href"].Value
                                .Contains("revert", StringComparison.OrdinalIgnoreCase))
                            .Select(dlNode => dlNode.Attributes["href"].Value);

                        string currentSkinSubType = GetSkinSubType(webSkinType, currentSkinName, webProvidedType);
                        string description = $"{webSkinType.Name} {currentSkinSubType} skin {currentSkinName}.";
                        DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
                        
                        //The html has a ul node with class gallery mw-gallery-nolines that contain img nodes with src
                        //attributes that starts with /images/thumb/ which are links to the screenshots.
                        //Using the thumb version to reduce bandwidth strain but this can be changed to the full versions.
                        if (skinPageDoc.DocumentNode
                                .SelectSingleNode("//ul[contains(@class, 'gallery mw-gallery-nolines')]")
                                ?.SelectNodes(".//img[contains(@src, '/images/thumb/')]") is
                            { Count: > 0 } currentScreenshotNodes)
                        {
                            IEnumerable<string> currentScreenshots = currentScreenshotNodes.Select(imgNode =>
                                string.Concat(baseScreenshotsAddress, imgNode.Attributes["src"].Value));
                            Skin tempSkin = Skin.Create
                            (
                                webSkinType.Name, currentSkinSubType, currentSkinName,
                                currentDownloadLinks, currentAuthor, description, currentDate, currentDate,
                                currentScreenshots, SkinsSource.Ephinea
                            );

                            foundSkins.Add(tempSkin);
                        }
                        else
                        {
                            currentAuthor = skinInfoNode.SelectSingleNode(".//tr[2]/td[1]").InnerText;
                            Skin tempSkin = Skin.Create
                            (
                                webSkinType.Name, currentSkinSubType, currentSkinName,
                                currentDownloadLinks, currentAuthor, description, currentDate, currentDate,
                                [], SkinsSource.Ephinea
                            );

                            foundSkins.Add(tempSkin);
                        }
                    }
                }

                return foundSkins;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return [];
            }
        }

        private string GetSkinSubType(SkinType webSkinType, string skinName, string webProvidedType)
        {
            foreach (string currentSubType in webSkinType.SubTypes)
            {
                if (skinName.Contains(currentSubType, StringComparison.OrdinalIgnoreCase) ||
                    currentSubType.Contains(webProvidedType, StringComparison.OrdinalIgnoreCase))
                {
                    return currentSubType;
                }
            }

            return "Other";
        }
    }
}