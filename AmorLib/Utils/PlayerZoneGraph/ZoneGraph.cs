using Agents;
using AIGraph;
using AmorLib.Events;
using GTFO.API;
using LevelGeneration;
using Player;

namespace AmorLib.Utils.PlayerZoneGraph;

[CallConstructorOnLoad]
public sealed class ZoneGraph // contributed by: Dinorush
{
    public static event Action? OnReachableUpdate;

    public static bool IsReady { get; private set; } = false;
    public static ZoneNode GetZoneNode(Agent agent) => GetZoneNode(agent.CourseNode.m_zone);
    public static ZoneNode GetZoneNode(AIG_CourseNode courseNode) => GetZoneNode(courseNode.m_zone);
    public static ZoneNode GetZoneNode(LG_Zone zone) => Current._zoneToNode[zone.ID];
    public static AreaNode GetAreaNode(Agent agent) => GetAreaNode(agent.CourseNode.m_area);
    public static AreaNode GetAreaNode(AIG_CourseNode courseNode) => GetAreaNode(courseNode.m_area);
    public static AreaNode GetAreaNode(LG_Area area) => Current._areaToNode[area.UID];

    public static ushort GetPlayerGroup(PlayerAgent player) => Current._playerToGroup.GetValueOrDefault(player.PlayerSlotIndex);

    public static bool IsReachable(PlayerAgent player, AIG_CourseNode courseNode) => GetAreaNode(courseNode).IsReachable(GetPlayerGroup(player));
    public static bool IsReachable(PlayerAgent player, LG_Area area) => GetAreaNode(area).IsReachable(GetPlayerGroup(player));
    public static bool IsReachable(PlayerAgent player, LG_Zone zone) => GetZoneNode(zone).IsReachable(GetPlayerGroup(player));
    public static bool IsReachable(AIG_CourseNode courseNode) => GetAreaNode(courseNode).IsReachable();
    public static bool IsReachable(LG_Area area) => GetAreaNode(area).IsReachable();
    public static bool IsReachable(LG_Zone zone) => GetZoneNode(zone).IsReachable();

    public const ushort NoGroup = 0;

    internal static readonly ZoneGraph Current = new();

    private readonly Dictionary<int, ZoneNode> _zoneToNode = new();
    private readonly Dictionary<int, AreaNode> _areaToNode = new();
    private readonly Dictionary<int, ushort> _playerToGroup = new();
    private ushort _lastGroup = 0;

    static ZoneGraph()
    {
        LevelAPI.OnBuildDone += Current.BuildZoneGraph;
        LevelAPI.OnLevelCleanup += Current.Cleanup;
        SNetEvents.OnCheckpointReload += Current.RefreshOnCheckpoint;
    }
    
    private void BuildZoneGraph()
    {
        foreach (var zone in Builder.CurrentFloor.allZones)
        {
            _zoneToNode.Add(zone.ID, new(zone));
            foreach (var area in zone.m_areas)
                _areaToNode.Add(area.UID, new(area));
        }

        foreach (var zone in _zoneToNode.Values)
            zone.OnNodesCreated();
        foreach (var area in _areaToNode.Values)
            area.OnNodesCreated();

        IsReady = true;
    }

    private void Cleanup()
    {
        _zoneToNode.Clear();
        _areaToNode.Clear();
        _playerToGroup.Clear();
        _lastGroup = 0;
        IsReady = false;
    }

    private void RefreshOnCheckpoint()
    {
        foreach (var area in _areaToNode.Values)
            area.UpdateEdges();
        RefreshGraph();
    }

    // Reset all nodes to be unreachable, then discover reachable nodes from all players.
    private void RefreshGraph()
    {
        foreach (var zone in _zoneToNode.Values)
            zone.Reset();
        foreach (var area in _areaToNode.Values)
            area.Reset();
        _playerToGroup.Clear();
        _lastGroup = 0;

        foreach (var player in PlayerManager.PlayerAgentsInLevel)
        {
            if (player.CourseNode == null) continue;

            UpdateOrCreateGroup(player);
        }

        OnReachableUpdate?.Invoke();
    }

    // Updates the player's group to match the node they're in, or create one if it doesn't exist.
    private ushort UpdateOrCreateGroup(PlayerAgent player)
    {
        var playerIndex = player.PlayerSlotIndex;
        var areaNode = GetAreaNode(player);
        if (areaNode.IsReachable())
        {
            return _playerToGroup[playerIndex] = areaNode.Group;
        }
        else
        {
            _playerToGroup[playerIndex] = ++_lastGroup;
            PropogateGroup(areaNode, _lastGroup);
            return _lastGroup;
        }
    }

    // Propogate the group to all connected areas with open doors.
    private void PropogateGroup(AreaNode areaNode, ushort group)
    {
        areaNode.SetGroup(group);
        foreach (var edge in areaNode.Edges)
            if (edge.IsOpen && edge.Neighbor.Group != group)
                PropogateGroup(edge.Neighbor, group);
    }

    internal void Internal_OnPlayerNodeChanged(PlayerAgent player, AIG_CourseNode oldNode)
    {
        if (!IsReady) return;

        var newGroup = UpdateOrCreateGroup(player);
        if (oldNode != null)
        {
            var oldAreaNode = GetAreaNode(oldNode.m_area);
            // Movement within the same group, no update needed.
            if (oldAreaNode.IsReachable(newGroup)) return;

            foreach (var group in _playerToGroup.Values)
            {
                // Group reachable by a different player. Preserve reachable.
                if (oldAreaNode.IsReachable(group))
                {
                    OnReachableUpdate?.Invoke();
                    return;
                }
            }

            // No players reachable, revoke reachable status.
            PropogateGroup(oldAreaNode, NoGroup);
        }

        OnReachableUpdate?.Invoke();
    }

    internal void Internal_OnDoorStateChanged(LG_Gate gate, bool isOpen)
    {
        if (!IsReady) return;

        var from = GetAreaNode(gate.m_linksFrom);
        var to = GetAreaNode(gate.m_linksTo);

        // Update graph edges to match door state.
        from.UpdateEdges();
        to.UpdateEdges();

        if (!isOpen)
        {
            // Door was open; either both are reachable (needs update) or neither is.
            if (from.IsReachable())
                RefreshGraph();
        }
        else
        {
            // Expand the reachable side or merge the two sides if both are reachable.
            if (from.IsReachable())
            {
                foreach ((var index, var group) in _playerToGroup.ToArray())
                    if (group == to.Group)
                        _playerToGroup[index] = from.Group;
                PropogateGroup(to, from.Group);
                OnReachableUpdate?.Invoke();
            }
            else if (to.IsReachable())
            {
                foreach ((var index, var group) in _playerToGroup.ToArray())
                    if (group == from.Group)
                        _playerToGroup[index] = to.Group;
                PropogateGroup(from, to.Group);
                OnReachableUpdate?.Invoke();
            }
        }
    }
}
