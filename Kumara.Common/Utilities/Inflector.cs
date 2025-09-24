// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Text.RegularExpressions;
using Humanizer;

namespace Kumara.Common.Utilities;

public static class Inflector
{
    private static readonly Regex splitPattern = new Regex(@"(?<=[a-z])(?=[A-Z])|(?=\s)");
    private static readonly Regex ofPattern = new Regex(@"\s?of", RegexOptions.IgnoreCase);

    private static string Inflect(string value, Func<string, string> inflector)
    {
        var parts = splitPattern.Split(value);

        var inflectIndex = parts.Length - 1;
        if (parts.Length == 3 && ofPattern.IsMatch(parts[1]))
        {
            inflectIndex = 0;
        }

        parts[inflectIndex] = inflector(parts[inflectIndex]);

        return string.Concat(parts);
    }

    public static string Pluralize(string value)
    {
        return Inflect(value, part => part.Pluralize());
    }

    public static string Singularize(string value)
    {
        return Inflect(value, part => part.Singularize());
    }

    public static string Camelize(string value)
    {
        return value.Camelize();
    }
}
