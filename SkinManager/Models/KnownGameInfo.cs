using System.Collections.Generic;

namespace SkinManager.Models;

public record KnownGameInfo(
    string GameName,
    string SkinsSiteName,
    string SkinsSiteAddress,
    List<SkinType> SkinTypes
)
{
    public static KnownGameInfo Create(string gameName, string skinsSiteName, string skinsSiteAddress, List<SkinType> skinTypes) 
    => new KnownGameInfo(gameName, skinsSiteName, skinsSiteAddress, skinTypes);

    public override string ToString() => $"This is Known Game Info for {GameName}.";
}