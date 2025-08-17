using GTFO.API.Utilities;

namespace AmorLib.Events;

public static class LevelEvents
{
    /// <summary>
    /// Invoked before all other <see cref="GTFO.API.LevelAPI.OnBuildDone"/> events are invoked.
    /// </summary>
    public static event Action? OnBuildDoneEarly;

    /// <summary>
    /// Invoked after all other <see cref="GTFO.API.LevelAPI.OnBuildDone"/> events are invoked.
    /// </summary>
    public static event Action? OnBuildDoneLate;

    internal static void BuildDoneEarly() => SafeInvoke.Invoke(OnBuildDoneEarly);
    internal static void BuildDoneLate() => SafeInvoke.Invoke(OnBuildDoneLate);
}
