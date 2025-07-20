using BepInEx;
using System.Text.RegularExpressions;

namespace AmorLib.Utils.Extensions;

public static class PluginInfoExtensions
{
    /// <summary>
    /// Determines if the <paramref name="pluginInfo"/> version matches the specified version.
    /// </summary>
    /// <remarks><paramref name="verison"/> must be formatted string "major.minor.patch" (ex: "1.2.3")</remarks>
    /// <returns><see langword="true"/> if the versions numbers are the same.</returns>
    public static bool VersionEquals(this PluginInfo pluginInfo, string verison)
    {
        return CompareVersion(pluginInfo, verison) is not null and 0;
    }

    /// <summary>
    /// Determines if the <paramref name="pluginInfo"/> version is greater than the specified version.
    /// </summary>
    /// <remarks><paramref name="verison"/> must be formatted string "major.minor.patch" (ex: "1.2.3")</remarks>
    /// <returns><see langword="true"/> if <paramref name="pluginInfo"/>'s version number is higher.</returns>
    public static bool VersionExceeds(this PluginInfo pluginInfo, string version)
    {
        return CompareVersion(pluginInfo, version) is not null and > 0;
    }

    /// <summary>
    /// Determines if the <paramref name="pluginInfo"/> version is greater than or equal to the specified version.
    /// </summary>
    /// <remarks><paramref name="verison"/> must be formatted string "major.minor.patch" (ex: "1.2.3")</remarks>
    /// <returns><see langword="true"/> if <paramref name="pluginInfo"/>'s version number equal or higher.</returns>
    public static bool VersionAtLeast(this PluginInfo pluginInfo, string version)
    {
        return CompareVersion(pluginInfo, version) is not null and >= 0;
    }

    private static int? CompareVersion(PluginInfo pluginInfo, string verison)
    {
        if (pluginInfo?.Metadata?.Version == null)
        {
            Logger.Error("PluginInfo or Version.Metadata is null");
            return null;
        }

        var match = Regex.Match(verison, @"^(\d+)\.(\d+)\.(\d+)$");
        if (!match.Success)
        {
            Logger.Error("Version string format is incorrect");
            return null;
        }

        int major = int.Parse(match.Groups[1].Value);
        int minor = int.Parse(match.Groups[2].Value);
        int patch = int.Parse(match.Groups[3].Value);
        return pluginInfo.Metadata.Version.CompareTo(new(major, minor, patch));
    }
}
