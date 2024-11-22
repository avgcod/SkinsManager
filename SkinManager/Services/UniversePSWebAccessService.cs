using HtmlAgilityPack;
using SkinManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkinManager.Services;

public class UniversePSWebAccessService : ISkinsAccessService
{
    private readonly Dictionary<SkinType, string> skinTypeAddresses = [];
    private readonly HttpClient _httpClient;

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

    public UniversePSWebAccessService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        Dictionary<SkinsSource, DateOnly> currentDate = [];
        currentDate.Add(SkinsSource.UniversPS, DateOnly.FromDateTime(DateTime.Now));

        skinTypeAddresses.Add(SkinType.Create("Area", [
            "Forest", "Caves", "Mines", "Ruins", "Jungle",
            "Mountain", "Seaside", "CCA", "Seabed", "Crater", "Desert", "Lobby", "City", "Temple", "Spaceship",
            "Towers"
        ], currentDate), areaSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("Pack", ["Other"], currentDate), packSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("Class", ["Humar"], currentDate), humarSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("Class", ["Hunewearl"], currentDate), hunewearlSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("Class", ["Hucast"], currentDate), hucastSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("Class", ["Hucaseal"], currentDate), hucasealSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("Class", ["Ramar"], currentDate), ramarSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("Class", ["Ramarl"], currentDate), ramarlSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("Class", ["Racast"], currentDate), racastSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("Class", ["Racaseal"], currentDate), racasealSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("Class", ["Fomar"], currentDate), fomarSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("Class", ["Fomarl"], currentDate), fomarlSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("Class", ["Fonewm"], currentDate), fonewmSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("Class", ["Fonewearl"], currentDate), fonewearlSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("NPC", ["Ninja"], currentDate), ninjaSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("NPC", ["Rico"], currentDate), ricoSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("NPC", ["Sonic"], currentDate), sonicSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("NPC", ["Knuckles"], currentDate), knucklesSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("NPC", ["Tails"], currentDate), tailsSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("NPC", ["Flowen"], currentDate), flowenSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("NPC", ["Elly"], currentDate), ellySkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("NPC", ["Momoka"], currentDate), momokaSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("NPC", ["Irene"], currentDate), ireneSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("NPC", ["Guild"], currentDate), guildHostessSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("NPC", ["Other"], currentDate), npcSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("Enemy", [
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
        ], currentDate), enemySkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("UI", ["Title Screen"], currentDate), titleScreenSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("UI", ["HUD"], currentDate), hudSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("UI", ["Text"], currentDate), textSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("Object", ["Other"], currentDate), objectSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("Effect", ["Attack", "Buff", "Healing", "Other"], currentDate),
            effectSkinsAddress);
        skinTypeAddresses.Add(SkinType.Create("Other", ["Misc"], currentDate), otherSkinsAddress);
    }

    public async Task<IEnumerable<Skin>> GetAvailableSkinsForSpecificTypeAsync(SkinType localSkinType)
    {
        SkinType webSkinType = skinTypeAddresses
            .First(x => x.Key.Name.Equals(localSkinType.Name, StringComparison.OrdinalIgnoreCase)).Key;

        return await GetSkinsFromWebsite(webSkinType, skinTypeAddresses[webSkinType]);
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

    private async Task<IEnumerable<Skin>> GetSkinsFromWebsite(SkinType webSkinType, string address)
    {
        string response = await _httpClient.GetStringAsync(baseSiteAddress + address);

        HtmlDocument theDoc = new();

        theDoc.LoadHtml(response);

        //The html lists each author in a div with an attribute name with a value that starts with cat_.
        IEnumerable<HtmlNode> authors = theDoc.DocumentNode.SelectNodes("//div[@class='title1 titreCategorie']");

        //The html lists skins for each author in a tbody under the respective author div but not as a child of the author div.
        IEnumerable<HtmlNode> skinTables =
            theDoc.DocumentNode.SelectNodes("//tbody[@class='contenuCategorieSkin']");

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
                    currentSkinName =
                        skinRow.SelectSingleNode(".//a[contains(@href,'#skin_')]")?.InnerText.Trim() ??
                        string.Empty;

                    //The html lists the skin date added in a text field after the 3rd b node.
                    currenSkinAddDate =
                        skinRow.SelectSingleNode(".//b[contains(text(),'Date')]")?.NextSibling?.InnerText.Trim() ??
                        string.Empty;

                    //The html lists the skin download line an a node with an attribute href with a value that contains telechargement and ends in t=d.
                    currentSkinDownloadLink =
                        skinRow.SelectSingleNode(
                                ".//a[contains(@href,'telechargement') and contains(@href,'&t=d')]")
                            ?.GetAttributeValue("href", string.Empty) ?? string.Empty;

                    currentSkinDownloadLink = string.Concat(baseDownloadAddress, currentSkinDownloadLink);

                    //The html lists the full size skin screenshot(s) in the second row in a nodes with an href attribute that contains imageSkin/.
                    //IEnumerable<HtmlNode> currentSkinScreenshotNodes = skinRow.SelectNodes(".//td/a[@class='highslide' and contains(@href,'imageSkin/')]");

                    //The html lists the mini skin screenshot(s) in the second row in img nodes with a src attribute that contains imageSkin/mini.
                    //IEnumerable<HtmlNode> currentSkinScreenshotNodes =
                    //    skinRow.SelectNodes(".//img[contains(@src,'imageSkin/mini')]");

                    if (skinRow.SelectNodes(".//img[contains(@src,'imageSkin/mini')]") is {} currentSkinScreenshotNodes)
                    {
                        currenSkinScreenshots = [];
                        foreach (HtmlNode screenShotNode in currentSkinScreenshotNodes)
                        {
                            currenSkinScreenshots.Add(string.Concat(baseScreenshotsAddress,
                                screenShotNode.Attributes["src"].Value));
                        }
                    }

                    string currentSkinSubType = GetSkinSubType(webSkinType, currentSkinName);
                    string description = $"{webSkinType.Name} {currentSkinSubType} skin {currentSkinName}.";
                    DateOnly currentDate = DateOnly.ParseExact(currenSkinAddDate, "dd/MM/yyyy");
                    Skin tempSkin = Skin.Create
                    (
                        webSkinType.Name, currentSkinSubType, currentSkinName,
                        [currentSkinDownloadLink], currentAuthor, description, currentDate, currentDate,
                        currenSkinScreenshots
                    );
                    foundSkins.Add(tempSkin);
                }
            }
        }

        return foundSkins;
    }

    private string GetSkinSubType(SkinType webSkinType, string skinName)
    {
        foreach (string currentSubType in webSkinType.SubTypes)
        {
            if (skinName.Contains(currentSubType, StringComparison.OrdinalIgnoreCase))
            {
                return currentSubType;
            }
        }

        if (webSkinType.Name != "Area")
        {
            return "Other";
        }

        return webSkinType.SubTypes[0];
    }
}