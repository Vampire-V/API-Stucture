using System.Text.RegularExpressions;

namespace Infrastructure.Extensions;

public static class StringExtensions
{
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var regex = new Regex("([a-z0-9])([A-Z])");
        return regex.Replace(input, "$1_$2").ToLower();
    }
}
