using AIGraph;
using AmorLib.Utils.ZoneGraphImpl;
using GTFO.API;
using LevelGeneration;
using Player;

namespace AmorLib.Utils;

/// <summary>Determines what type of layer to check reachability on.</summary>
public enum GraphLayer
{
    /// <summary>Check along all doors.</summary>
    Area,
    /// <summary>Check along security doors.</summary>
    Zone
}

/// <summary>Determines what type of reachablility to use.</summary>
public enum ReachableMode
{
    Traversable = 0,
    /// <summary>Include opening and closing doors.</summary>
    Changing = 1
}

[CallConstructorOnLoad]
/// <summary>Contains functions to get whether a room is reachable with fast performance.</summary>
public sealed class ZoneGraphUtil // contributed by: Dinorush
{
    /// <summary>
    /// Callbacks that run whenever there is a change in reachability to any room or zone.
    /// </summary>
    public static event Action? OnReachableUpdate;

    /// <summary>
    /// Callbacks that run whenever there is a change in reachability for a specific flag(s).
    /// Index as <see cref="OnReachableTypeUpdate"/>[<see cref="ReachableMode"/> settings] to add/remove callbacks.
    /// </summary>
    public static readonly ReachableEvents OnReachableTypeUpdate = new();

    /// <summary>
    /// Returns <see langword="true"/> when the zone graph is built and functions are ready to be called (after OnBuildDone), otherwise <see langword="false"/>.
    /// </summary>
    public static bool IsReady { get; private set; } = false;

    #region IS_REACHABLE
    /// <summary>
    /// Gets whether there is an open path from <paramref name="courseNode"/> to <paramref name="player"/>.
    /// </summary>
    public static bool IsReachable(PlayerAgent player, AIG_CourseNode courseNode, GraphLayer layer = GraphLayer.Area, ReachableMode mode = ReachableMode.Traversable) => IsReachable(player.PlayerSlotIndex, courseNode, layer, mode);
    /// <summary>
    /// Gets whether there is an open path from <paramref name="area"/> to <paramref name="player"/>.
    /// </summary>
    public static bool IsReachable(PlayerAgent player, LG_Area area, GraphLayer layer = GraphLayer.Area, ReachableMode mode = ReachableMode.Traversable) => IsReachable(player.PlayerSlotIndex, area, layer, mode);
    /// <summary>
    /// Gets whether there is an open path from <paramref name="zone"/> to <paramref name="player"/>.
    /// </summary>
    public static bool IsReachable(PlayerAgent player, LG_Zone zone, ReachableMode mode = ReachableMode.Traversable) => IsReachable(player.PlayerSlotIndex, zone, mode);
    /// <summary>
    /// Gets whether there is an open path from <paramref name="courseNode"/> to the player in <paramref name="playerSlotIndex"/>.
    /// </summary>
    public static bool IsReachable(int playerSlotIndex, AIG_CourseNode courseNode, GraphLayer layer = GraphLayer.Area, ReachableMode mode = ReachableMode.Traversable) => GetManager(layer).IsReachable(playerSlotIndex, courseNode, mode);
    /// <summary>
    /// Gets whether there is an open path from <paramref name="area"/> to the player in <paramref name="playerSlotIndex"/>.
    /// </summary>
    public static bool IsReachable(int playerSlotIndex, LG_Area area, GraphLayer layer = GraphLayer.Area, ReachableMode mode = ReachableMode.Traversable) => GetManager(layer).IsReachable(playerSlotIndex, area, mode);
    /// <summary>
    /// Gets whether there is an open path from <paramref name="zone"/> to the player in <paramref name="playerSlotIndex"/>.
    /// </summary>
    public static bool IsReachable(int playerSlotIndex, LG_Zone zone, ReachableMode mode = ReachableMode.Traversable) => GetManager(GraphLayer.Zone).IsReachable(playerSlotIndex, zone, mode);
    /// <summary>
    /// Gets whether there is an open path from <paramref name="courseNode"/> to any player.
    /// </summary>
    public static bool IsReachable(AIG_CourseNode courseNode, GraphLayer layer = GraphLayer.Area, ReachableMode mode = ReachableMode.Traversable) => GetManager(layer).IsReachable(courseNode, mode);
    /// <summary>
    /// Gets whether there is an open path from <paramref name="area"/> to any player.
    /// </summary>
    public static bool IsReachable(LG_Area area, GraphLayer layer = GraphLayer.Area, ReachableMode mode = ReachableMode.Traversable) => GetManager(layer).IsReachable(area, mode);
    /// <summary>
    /// Gets whether there is an open path from <paramref name="zone"/> to any player.
    /// </summary>
    public static bool IsReachable(LG_Zone zone, ReachableMode mode = ReachableMode.Traversable) => GetManager(GraphLayer.Zone).IsReachable(zone, mode);
    #endregion

    /// <summary>
    /// ID that represents no group.
    /// </summary>
    public const ushort NoGroup = 0;

    internal static readonly ZoneGraphUtil Current = new();

    private static GraphManager GetManager(GraphLayer layer) => layer == GraphLayer.Area ? Current._areaManager : Current._zoneManager;

    private readonly ZoneManager _zoneManager = new();
    private readonly AreaManager _areaManager = new();

    static ZoneGraphUtil()
    {
        LevelAPI.OnBuildStart += () => LG_Area.s_areaUIDCounter = 0;
        LevelAPI.OnBuildDone += Current.BuildZoneGraph;
        LevelAPI.OnLevelCleanup += Current.Cleanup;
        EventAPI.OnCheckpointReloaded += Current.RefreshOnCheckpoint;
    }

    // Debug logging - call in static constructor when needed for testing
    private static void DebugLogging()
    {
        OnReachableUpdate += () =>
        {
            Logger.Info($"Graph update\n{Current._areaManager.GetDebugInfo()}\n\n{Current._zoneManager.GetDebugInfo()}");
        };
    }
    
    private void BuildZoneGraph()
    {
        foreach (var zone in Builder.CurrentFloor.allZones)
        {
            _zoneManager.Internal_AddNode(zone.ID, zone);
            foreach (var area in zone.m_areas)
                _areaManager.Internal_AddNode(area.UID, area);
        }

        _zoneManager.Internal_CreateEdges();
        _areaManager.Internal_CreateEdges();
        _zoneManager.Internal_OnEdgesCreated();
        _areaManager.Internal_OnEdgesCreated();
        IsReady = true;
    }

    private void Cleanup()
    {
        _zoneManager.Internal_Cleanup();
        _areaManager.Internal_Cleanup();
        IsReady = false;
    }

    private void RefreshOnCheckpoint()
    {
        _zoneManager.Internal_OnCheckpoint();
        _areaManager.Internal_OnCheckpoint();
        OnReachableUpdate?.Invoke();
    }

    internal void Internal_OnPlayerNodeChanged(PlayerAgent player, AIG_CourseNode? oldNode)
    {
        if (!IsReady) return;

        bool runCallbacks = false;
        runCallbacks |= _zoneManager.Internal_OnPlayerNodeChanged(player, oldNode);
        runCallbacks |= _areaManager.Internal_OnPlayerNodeChanged(player, oldNode);

        if (runCallbacks)
            OnReachableUpdate?.Invoke();
    }

    internal void Internal_OnDoorStateChanged(LG_Gate gate, bool isOpen)
    {
        if (!IsReady) return;

        bool runCallbacks = _areaManager.Internal_OnDoorStateChanged(gate, isOpen);
        if (gate.m_linksTo.m_zone.ID != gate.m_linksFrom.m_zone.ID)
            runCallbacks |= _zoneManager.Internal_OnDoorStateChanged(gate, isOpen);
        
        if (runCallbacks)
            OnReachableUpdate?.Invoke();
    }

    internal void Internal_OnDoorAnimationChanged(LG_Gate gate, bool isChanging)
    {
        if (!IsReady) return;

        bool runCallbacks = _areaManager.Internal_OnDoorAnimationChanged(gate, isChanging);
        if (gate.m_linksTo.m_zone.ID != gate.m_linksFrom.m_zone.ID)
            runCallbacks |= _zoneManager.Internal_OnDoorAnimationChanged(gate, isChanging);

        if (runCallbacks)
            OnReachableUpdate?.Invoke();
    }
}