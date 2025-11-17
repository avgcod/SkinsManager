using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;
using SkinManager.Types;

namespace SkinManager.Extensions;

public static class SkinExtensions
{
    public static bool IsOriginal(this LocalSkin theSkin) => theSkin.SkinLocation.Contains("originals", StringComparison.OrdinalIgnoreCase);

    public static string ToTitleCase(this string str) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
    //Credit to Alex K https://stackoverflow.com/a/25023688
    public static string NormalizeWhiteSpace(this string input, char normalizeTo = ' ')
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        int current = 0;
        char[] output = new char[input.Length];
        bool skipped = false;

        foreach (char c in input.ToCharArray())
        {
            if (char.IsWhiteSpace(c))
            {
                if (!skipped)
                {
                    if (current > 0)
                        output[current++] = normalizeTo;

                    skipped = true;
                }
            }
            else
            {
                skipped = false;
                output[current++] = c;
            }
        }

        return new string(output, 0, skipped ? current - 1 : current);
    }

    public static string RemoveSpecialCharacters(this string text){
        string[] specialCharacters = ["&", @"/", @"\", "(", ")"];
        return specialCharacters.Aggregate(text, (currentCleanedString, currentSpecialCharacter) => currentCleanedString.Replace(currentSpecialCharacter, string.Empty));
    }
    public static LocalSkin ToLocalSkin(this WebSkin currentSkin, string localSkinPath) 
        => new LocalSkin(currentSkin.SkinName, currentSkin.SkinType, currentSkin.SkinSubType, localSkinPath, currentSkin.Author, currentSkin.ScreenshotLinks);
    //Credit to SergeyGrudskiy https://stackoverflow.com/a/35874937
    public static async Task<IEnumerable<T1>> SelectManyAsync<T, T1>(this IEnumerable<T> enumeration, Func<T, Task<IEnumerable<T1>>> func)
    {
        return (await Task.WhenAll(enumeration.Select(func))).SelectMany(s => s);
    }
    public static async Task<IEnumerable<T1>> SelectAsync<T, T1>(this IEnumerable<T> enumeration, Func<T, Task<T1>> func)
    {
        return await Task.WhenAll(enumeration.Select(func));
    }
}

public static class GameInfoExtensions{
    public static GameInfo UpdateSkinsLocation(this GameInfo currentInfo, string skinsLocation) 
        => currentInfo with{ SkinsLocation = skinsLocation };

    public static GameInfo UpdateGameLocation(this GameInfo currentInfo,string gameLocation) 
        => currentInfo with{ GameLocation = gameLocation };

    public static GameInfo UpdateGameExecutableLocation(this GameInfo currentInfo,string gameExecutableLocation) 
        => currentInfo with{ GameExecutable = gameExecutableLocation };
}

public static class LangExtExtensions{
    public static string GetSkinName(this Option<Skin> optionalSkin){
        return optionalSkin.Match(currentSkin =>
            currentSkin switch{
                WebSkin selectedWebSkin => selectedWebSkin.SkinName,
                LocalSkin selectedLocalSkin => selectedLocalSkin.SkinName
            }, () => string.Empty);

    }
    
    //--- Partionning methods based on  https://github.com/louthy/language-ext/blob/main/LanguageExt.Core/Monads/Alternative%20Monads/Either/Either.Extensions.cs ---
    public static IEnumerable<T> Lefts<T,T2>(this IEnumerable<Either<T, T2>> eithers){
        return eithers.Aggregate(new List<T>(),
            (lefts, currentLeft) =>
                currentLeft switch
                {
                    Either<T, T2>.Left ({ } theLeft) => [..lefts, theLeft],
                    _ => lefts
                });
    }
    
    public static IEnumerable<T2> Rights<T,T2>(this IEnumerable<Either<T, T2>> eithers){
        return eithers.Aggregate(new List<T2>(),
            (rights, currentRight) =>
                currentRight switch
                {
                    Either<T, T2>.Right ({ } theRight) => [..rights, theRight],
                    _ => rights
                });
    }
    
    public static (IEnumerable<T> , IEnumerable<T2>) Partition<T,T2>(this IEnumerable<Either<T, T2>> eithers){
        return eithers.Aggregate((Lefts : new List<T>(), Rights : new List<T2>()),
            (thePartionedEithers, currentEither) =>
                currentEither switch
                {
                    Either<T, T2>.Left ({ } theLeft) => new ([..thePartionedEithers.Lefts, theLeft], thePartionedEithers.Rights),
                    Either<T, T2>.Right ({ } theRight) => new (thePartionedEithers.Lefts, [..thePartionedEithers.Rights, theRight]),
                    _ => throw new NotSupportedException()
                });
    }
    
    public static IEnumerable<T> Successes<T>(this IEnumerable<Fin<T>> theFins){
        return theFins.Aggregate(new List<T>(),
            (successResults, currentSuccess) =>
                currentSuccess switch
                {
                    Fin<T>.Succ ({ } theSuccessResult) => [..successResults, theSuccessResult],
                    _ => successResults
                });
    }
    
    public static IEnumerable<Error> Failures<T>(this IEnumerable<Fin<T>> theFins){
        return theFins.Aggregate(new List<Error>(),
            (failResults, currentFailure) =>
                currentFailure switch
                {
                    Fin<T>.Fail ({ } theFailureResult) => [..failResults, theFailureResult],
                    _ => failResults
                });
    }
    
    public static (IEnumerable<T> , IEnumerable<Error>) Partition<T>(this IEnumerable<Fin<T>> theFins){
        return theFins.Aggregate((Successes : new List<T>(), Failures : new List<Error>()),
            (thePartionedFins, currentFin) =>
                currentFin switch
                {
                    Fin<T>.Succ ({ } theSuccessResult)  => new ([..thePartionedFins.Successes, theSuccessResult], thePartionedFins.Failures),
                    Fin<T>.Fail ({ } theFailureResult) => new (thePartionedFins.Successes, [..thePartionedFins.Failures, theFailureResult]),
                    _ => throw new NotSupportedException()
                });
    }
}