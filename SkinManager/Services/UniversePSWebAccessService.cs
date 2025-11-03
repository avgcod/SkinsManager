using HtmlAgilityPack;
using SkinManager.Types;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkinManager.Services;

public static class UniversePsWebAccessService
{
    private static readonly Dictionary<(string SkinType, string SkinSubType), string> _skinTypeAddresses = PopulateAddresses();

    #region Web Address Strings

    private const string BaseSiteAddress = "http://universps.online.fr/pso/bb/skin/listeSkinUS.php5";
    private const string BaseScreenshotsAddress = "http://universps.online.fr/pso/bb/skin/";
    private const string BaseDownloadAddress = "http://universps.online.fr/pso/bb/skin/";

    private const string AreaSkinsAddress = "?cat=17";

    //private const string magSkinsAddress = "?cat=36";
    private const string PackSkinsAddress = "?cat=36";
    private const string HumarSkinsAddress = "?cat=1";
    private const string HunewearlSkinsAddress = "?cat=2";
    private const string HucastSkinsAddress = "?cat=3";
    private const string HucasealSkinsAddress = "?cat=4";
    private const string RamarSkinsAddress = "?cat=5";
    private const string RamarlSkinsAddress = "?cat=6";
    private const string RacastSkinsAddress = "?cat=7";
    private const string RacasealSkinsAddress = "?cat=8";
    private const string FomarSkinsAddress = "?cat=9";
    private const string FomarlSkinsAddress = "?cat=10";
    private const string FonewmSkinsAddress = "?cat=11";
    private const string FonewearlSkinsAddress = "?cat=12";
    private const string NinjaSkinsAddress = "?cat=22";
    private const string RicoSkinsAddress = "?cat=23";
    private const string SonicSkinsAddress = "?cat=19";
    private const string KnucklesSkinsAddress = "?cat=21";
    private const string TailsSkinsAddress = "?cat=20";
    private const string FlowenSkinsAddress = "?cat=25";
    private const string EllySkinsAddress = "?cat=24";
    private const string MomokaSkinsAddress = "?cat=26";
    private const string IreneSkinsAddress = "?cat=29";
    private const string GuildHostessSkinsAddress = "?cat=27";
    private const string EnemySkinsAddress = "?cat=14";
    private const string NpcSkinsAddress = "?cat=32";
    private const string TitleScreenSkinsAddress = "?cat=34";
    private const string HudSkinsAddress = "?cat=16";
    private const string TextSkinsAddress = "?cat=33";
    private const string ObjectSkinsAddress = "?cat=15";
    private const string EffectSkinsAddress = "?cat=18";
    private const string OtherSkinsAddress = "?cat=30";

    private static readonly IEnumerable<string> Areas =[
        "Forest", "Caves", "Mines", "Ruins", "Jungle",
        "Mountain", "Seaside", "CCA", "Seabed", "Crater", "Desert", "Lobby", "City", "Temple", "Spaceship",
        "Towers"
    ];

    private static readonly IEnumerable<string> Enemies =[
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
    ];

    private static readonly IEnumerable<string> Effects = ["Attack", "Buff", "Healing", "Other"];

    #endregion

    private static Dictionary<(string SkinType, string SkinSubType), string> PopulateAddresses(){
        Dictionary<(string SkinType, string SkinSubType), string> skinTypeAddresses = new();
        skinTypeAddresses.Add(new("Area", string.Empty), AreaSkinsAddress);
        skinTypeAddresses.Add(new("Pack", "Other"), PackSkinsAddress);
        skinTypeAddresses.Add(new("Class", "Humar"), HumarSkinsAddress);
        skinTypeAddresses.Add(new("Class", "Hunewearl"), HunewearlSkinsAddress);
        skinTypeAddresses.Add(new("Class", "Hucast"), HucastSkinsAddress);
        skinTypeAddresses.Add(new("Class", "Hucaseal"), HucasealSkinsAddress);
        skinTypeAddresses.Add(new("Class", "Ramar"), RamarSkinsAddress);
        skinTypeAddresses.Add(new("Class", "Ramarl"), RamarlSkinsAddress);
        skinTypeAddresses.Add(new("Class", "Racast"), RacastSkinsAddress);
        skinTypeAddresses.Add(new("Class", "Racaseal"), RacasealSkinsAddress);
        skinTypeAddresses.Add(new("Class", "Fomar"), FomarSkinsAddress);
        skinTypeAddresses.Add(new("Class", "Fomarl"), FomarlSkinsAddress);
        skinTypeAddresses.Add(new("Class", "Fonewm"), FonewmSkinsAddress);
        skinTypeAddresses.Add(new("Class", "Fonewearl"), FonewearlSkinsAddress);
        skinTypeAddresses.Add(new("NPC", "Ninja"), NinjaSkinsAddress);
        skinTypeAddresses.Add(new("NPC", "Rico"), RicoSkinsAddress);
        skinTypeAddresses.Add(new("NPC", "Sonic"), SonicSkinsAddress);
        skinTypeAddresses.Add(new("NPC", "Knuckles"), KnucklesSkinsAddress);
        skinTypeAddresses.Add(new("NPC", "Tails"), TailsSkinsAddress);
        skinTypeAddresses.Add(new("NPC", "Flowen"), FlowenSkinsAddress);
        skinTypeAddresses.Add(new("NPC", "Elly"), EllySkinsAddress);
        skinTypeAddresses.Add(new("NPC", "Momoka"), MomokaSkinsAddress);
        skinTypeAddresses.Add(new("NPC", "Irene"), IreneSkinsAddress);
        skinTypeAddresses.Add(new("NPC", "Guild"), GuildHostessSkinsAddress);
        skinTypeAddresses.Add(new("NPC", "Other"), NpcSkinsAddress);
        skinTypeAddresses.Add(new("Enemy", string.Empty), EnemySkinsAddress);
        skinTypeAddresses.Add(new("UI", "Title Screen"), TitleScreenSkinsAddress);
        skinTypeAddresses.Add(new("UI", "HUD"), HudSkinsAddress);
        skinTypeAddresses.Add(new("UI", "Text"), TextSkinsAddress);
        skinTypeAddresses.Add(new("Object", "Other"), ObjectSkinsAddress);
        skinTypeAddresses.Add(new("Effect", string.Empty),EffectSkinsAddress);
        skinTypeAddresses.Add(new("Other", "Misc"), OtherSkinsAddress);
        return skinTypeAddresses;
    }

    public static async Task<ImmutableList<WebSkin>> GetAvailableSkinsAsync(HttpClient httpClient)
    {
        List<WebSkin> skins = [];

        foreach (KeyValuePair<(string, string),string> currentPair in _skinTypeAddresses)
        {
            skins.AddRange(await GetSkinsFromWebsite(httpClient, currentPair.Key.Item1, currentPair.Key.Item2, currentPair.Value));
        }

        return skins.ToImmutableList();
    }

    private static async Task<ImmutableList<WebSkin>> GetSkinsFromWebsite(HttpClient httpClient, string skinType, string skinSubType, string address)
    {
        string response = await httpClient.GetStringAsync(BaseSiteAddress + address);

        HtmlDocument theDoc = new();

        theDoc.LoadHtml(response);

        //The html lists each author in a div with an attribute name with a value that starts with cat_.
        IEnumerable<HtmlNode> authors = theDoc.DocumentNode.SelectNodes("//div[@class='title1 titreCategorie']")!;

        //The html lists skins for each author in a tbody under the respective author div but not as a child of the author div.
        IEnumerable<HtmlNode> skinTables =
            theDoc.DocumentNode.SelectNodes("//tbody[@class='contenuCategorieSkin']")!;

        using var skinTablesEnumerator = skinTables.GetEnumerator();

        string currentAuthor = string.Empty;
        string currentSkinName = string.Empty;
        string currenSkinAddDate = string.Empty;
        string currentSkinDownloadLink = string.Empty;
        List<WebSkin> foundSkins = [];
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
                IEnumerable<HtmlNode> skinRows = skinTableNodes.SelectNodes(".//tr[@class='ligneSkin']")!;

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

                    currentSkinDownloadLink = string.Concat(BaseDownloadAddress, currentSkinDownloadLink);

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
                            currenSkinScreenshots.Add(string.Concat(BaseScreenshotsAddress,
                                screenShotNode.Attributes["src"].Value));
                        }
                    }

                    string currentSkinSubType = skinSubType == string.Empty ? GetSkinSubType(skinType, currentSkinName) : skinSubType;
                    DateOnly currentDate = DateOnly.ParseExact(currenSkinAddDate, "dd/MM/yyyy");
                    WebSkin tempSkin = new
                    (
                        currentSkinName, skinType, currentSkinSubType, address, currentAuthor, [currentSkinDownloadLink],
                        currenSkinScreenshots.ToImmutableList()
                    );
                    foundSkins.Add(tempSkin);
                }
            }
        }

        return foundSkins.ToImmutableList();
    }

    private static string GetSkinSubType(string skinType, string skinName){
        if (skinType == "Area") return Areas.FirstOrDefault(skinName.Contains) ?? "Other";
        else if (skinType == "Enemy") return Enemies.FirstOrDefault(skinName.Contains) ?? "Other";
        else if (skinType == "Effect") return Effects.FirstOrDefault(skinName.Contains) ?? "Other";
        
            return "Other";
    }
}
