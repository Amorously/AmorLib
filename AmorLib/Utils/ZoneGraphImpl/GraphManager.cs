using AIGraph;
using LevelGeneration;
using Player;
using UnityEngine;

namespace AmorLib.Utils.ZoneGraphImpl;

internal sealed class ZoneManager : GraphManager<LG_Zone, ZoneNode>
{
    public ZoneManager() : base(GraphLayer.Zone) { }

    public string GetDebugInfo()
    {
        static string GetInnerInfo(GroupManager<ZoneNode> manager)
        {
            var playerGroups = string.Join(", ", manager.PlayerGroups.ToList().ConvertAll(kv => $"(Slot: {kv.Key}, Group: {kv.Value})"));
            var reachableNodes = string.Join(", ", manager.Nodes.Where(a => a.IsReachable()).Select(a => $"({a.Base.NavInfo.ToString()}:{a.Group})"));
            return $"- Players: {playerGroups}\n" +
                $"- Nodes: {reachableNodes}";
        }

        return $"\n--- Zone Group Info---\n\n" +
            $"Traversable:\n" +
            $"{GetInnerInfo(_groupManagers[0])}\n\n" +
            $"Changing:\n" +
            $"{GetInnerInfo(_groupManagers[1])}";
    }

    internal override void Internal_AddNode(int id, LG_Zone zone)
    {
        foreach (var manager in _groupManagers)
            manager.Internal_AddNode(id, new(zone, manager));
    }
}

internal sealed class AreaManager : GraphManager<LG_Area, AreaNode>
{
    public AreaManager() : base(GraphLayer.Area) { }

    public string GetDebugInfo()
    {
        static string GetInnerInfo(GroupManager<AreaNode> manager)
        {
            var playerGroups = string.Join(", ", manager.PlayerGroups.ToList().ConvertAll(kv => $"(Slot: {kv.Key}, Group: {kv.Value})"));
            var reachableNodes = string.Join(", ", manager.Nodes.Where(a => a.IsReachable()).Select(a => $"({a.Base.m_zone.NavInfo.ToString()}{a.Base.m_navInfo.ToString()}:{a.Group})"));
            return $"- Players: {playerGroups}\n" +
                $"- Nodes: {reachableNodes}";
        }

        return $"\n--- Area Group Info---\n\n" +
            $"Traversable:\n" +
            $"{GetInnerInfo(_groupManagers[0])}\n\n" +
            $"Changing:\n" +
            $"{GetInnerInfo(_groupManagers[1])}";
    }

    internal override void Internal_AddNode(int id, LG_Area area)
    {
        foreach (var manager in _groupManagers)
            manager.Internal_AddNode(id, new(area, manager));
    }
}

internal abstract class GraphManager<T, E> : GraphManager
    where T : MonoBehaviour
    where E : GraphNode<T, E>
{
    private static readonly int s_NumManagers = Enum.GetValues<ReachableMode>().Length;

    private protected readonly GroupManager<E>[] _groupManagers = new GroupManager<E>[s_NumManagers];

    public GraphManager(GraphLayer layer) : base(layer)
    {
        for (int i = 0; i < s_NumManagers; i++)
            _groupManagers[i] = new(layer, (ReachableMode)i);
    }

    public override bool IsReachable(int playerSlotIndex, int id, ReachableMode mode)
    {
        bool result = _groupManagers[(int)ReachableMode.Traversable].IsReachable(playerSlotIndex, id);
        if (mode == ReachableMode.Changing)
            result = result || _groupManagers[(int)ReachableMode.Changing].IsReachable(playerSlotIndex, id);
        return result;
    }

    public override bool IsReachable(int id, ReachableMode mode)
    {
        bool result = _groupManagers[(int)ReachableMode.Traversable].IsReachable(id);
        if (mode == ReachableMode.Changing)
            result = result || _groupManagers[(int)ReachableMode.Changing].IsReachable(id);
        return result;
    }

    internal void Internal_Cleanup()
    {
        foreach (var manager in _groupManagers)
            manager.Internal_Cleanup();
    }

    internal abstract void Internal_AddNode(int id, T area);

    internal void Internal_CreateEdges()
    {
        foreach (var manager in _groupManagers)
            manager.Internal_CreateEdges();
    }

    internal void Internal_OnEdgesCreated()
    {
        foreach (var manager in _groupManagers)
            manager.Internal_OnEdgesCreated();
    }

    internal void Internal_OnCheckpoint()
    {
        foreach (var manager in _groupManagers)
            manager.Internal_OnCheckpoint();
    }

    internal bool Internal_OnPlayerNodeChanged(PlayerAgent player, AIG_CourseNode? oldCourseNode)
    {
        bool result = false;
        foreach (var manager in _groupManagers)
            result |= manager.Internal_OnPlayerNodeChanged(player, oldCourseNode);
        return result;
    }

    internal bool Internal_OnDoorStateChanged(LG_Gate gate, bool isOpen)
    {
        return _groupManagers[(int)ReachableMode.Traversable].Internal_OnEdgeStateChanged(gate, isOpen);
    }

    internal bool Internal_OnDoorAnimationChanged(LG_Gate gate, bool isAnimating)
    {
        return _groupManagers[(int)ReachableMode.Changing].Internal_OnEdgeStateChanged(gate, isAnimating);
    }
}

internal abstract class GraphManager
{
    private readonly GraphLayer _layer;
    public GraphManager(GraphLayer layer)
    {
        _layer = layer;
    }

    public abstract bool IsReachable(int playerSlotIndex, int id, ReachableMode mode);
    public bool IsReachable(int playerSlotIndex, LG_Zone zone, ReachableMode mode) => IsReachable(playerSlotIndex, zone.ID, mode);
    public bool IsReachable(int playerSlotIndex, LG_Area area, ReachableMode mode)
    {
        if (_layer == GraphLayer.Area)
            return IsReachable(playerSlotIndex, area.UID, mode);
        else
            return IsReachable(playerSlotIndex, area.m_zone.ID, mode);
    }
    public bool IsReachable(int playerSlotIndex, AIG_CourseNode courseNode, ReachableMode mode)
    {
        if (_layer == GraphLayer.Area)
            return IsReachable(playerSlotIndex, courseNode.m_area.UID, mode);
        else
            return IsReachable(playerSlotIndex, courseNode.m_zone.ID, mode);
    }

    public abstract bool IsReachable(int id, ReachableMode mode);
    public bool IsReachable(LG_Zone zone, ReachableMode mode) => IsReachable(zone.ID, mode);
    public bool IsReachable(LG_Area area, ReachableMode mode)
    {
        if (_layer == GraphLayer.Area)
            return IsReachable(area.UID, mode);
        else
            return IsReachable(area.m_zone.ID, mode);
    }
    public bool IsReachable(AIG_CourseNode courseNode, ReachableMode mode)
    {
        if (_layer == GraphLayer.Area)
            return IsReachable(courseNode.m_area.UID, mode);
        else
            return IsReachable(courseNode.m_zone.ID, mode);
    }
}

