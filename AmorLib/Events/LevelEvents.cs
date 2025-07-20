using LevelGeneration;

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

    /// <summary>
    /// Invoked after <see cref="LevelGeneration.LG_Factory.OnStart"/>.
    /// </summary>
    /// <remarks>(Borrowed from GTFO-API until it is updated.)</remarks>
    public static event Action? OnFactoryStart;

    /// <summary>
    /// Invoked when LevelGeneration Job Batch has started.
    /// </summary>
    /// <remarks>(Borrowed from GTFO-API until it is updated.)</remarks>
    public static event Action<LG_Factory.BatchName>? OnBeforeBuildBatch;

    /// <summary>
    /// Invoked when LevelGeneration Job Batch has finished.
    /// </summary>
    /// <remarks>(Borrowed from GTFO-API until it is updated.)</remarks>
    public static event Action<LG_Factory.BatchName>? OnAfterBuildBatch;

    /// <summary>
    /// Invoked after <see cref="LevelGeneration.LG_Factory.FactoryDone"/>.
    /// </summary>
    /// <remarks>(Borrowed from GTFO-API until it is updated.)</remarks>
    public static event Action? OnFactoryDone;

    internal static void BuildDoneEarly() => OnBuildDoneEarly?.Invoke();
    internal static void BuildDoneLate() => OnBuildDoneLate?.Invoke();
    internal static void FactoryStart() => OnFactoryStart?.Invoke();
    internal static void BeforeBuildBatch(LG_Factory.BatchName batchName) => OnBeforeBuildBatch?.Invoke(batchName);
    internal static void AfterBuildBatch(LG_Factory.BatchName batchName) => OnAfterBuildBatch?.Invoke(batchName);
    internal static void FactoryFinished() => OnFactoryDone?.Invoke();
}
