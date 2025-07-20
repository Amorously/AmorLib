using AmorLib.Utils.JsonElementConverters;

namespace AmorLib.Utils.Extensions;

public static class StringExtensions
{
    public static bool EqualsAny(this string input, bool ignoreCase = false, params string[] args)
    {
        var comparisonMode = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        foreach (var arg in args)
        {
            if (input.Equals(arg, comparisonMode))
            {
                return true;
            }
        }
        return false;
    }

    public static LocaleText ToLocaleText(this string input)
    {
        return new(input);
    }
}
