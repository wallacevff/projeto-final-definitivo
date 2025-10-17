using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ProjetoFinal.Infra.CrossCutting.Extensions;

public static class StringExtensions
{
    public static string OnlyNumbers(this string input)
    {
        return Regex.Replace(input, @"[^\d]", "");
    }

    public static string RemoveSpecialChars(this string input)
    {
        return new string(input.ToCharArray().Where(c => !char.IsLetterOrDigit(c)).ToArray());
    }

    public static string RemoveDiacritics(this string input)
    {
        var normalizedString = input.Normalize(NormalizationForm.FormD);
        var stringBuilder = new System.Text.StringBuilder();

        foreach (var c in normalizedString)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    public static string RemoveAllSpaces(this string input)
    {
        return new string(input.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    public static string MakeCharsComparable(this string input)
    {
        return input
            .RemoveDiacritics()
            .ToUpper()
            .RemoveAllSpaces();
    }
}