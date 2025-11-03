using System;
using System.Globalization;
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
}