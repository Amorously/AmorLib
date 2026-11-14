using LevelGeneration;
using UnityEngine;

namespace AmorLib.Utils.ZoneGraphImpl;

internal class NodeEdge<T>
{
    public readonly LG_Gate Gate;

    public readonly T Neighbor;

    public NodeEdge<T> Reverse { get; internal set; }

    public bool IsOpen { get; internal set; }

    public bool IsChanging => _hasDoor && _door!.InAnimation;

    private readonly bool _hasDoor;
    private readonly iLG_Door_Core? _door;

    public NodeEdge(LG_Gate gate, T node, bool startState)
    {
        Gate = gate;
        Reverse = null!;
        _door = gate.SpawnedDoor;
        _hasDoor = _door != null;
        Neighbor = node;
        IsOpen = startState;
    }
}

internal sealed class ZoneNode : GraphNode<LG_Zone, ZoneNode>
{
    private List<(LG_Gate gate, ZoneNode zone)> _neighborsToAdd = new();

    public ZoneNode(LG_Zone zone, GroupManager<ZoneNode> manager) : base(zone, manager)
    {
        var gate = zone.m_sourceGate;
        var parentZone = gate?.m_linksFrom?.m_zone;
        if (parentZone != null)
        {
            // Parent will have generated first, so we know it exists
            var parent = manager.GetNode(parentZone);
            parent._neighborsToAdd.Add((gate!, this));
            _neighborsToAdd.Add((gate!, parent));
        }
    }

    internal override void CreateEdges(ReachableMode mode)
    {
        Edges = _neighborsToAdd.ConvertAll<NodeEdge<ZoneNode>>(pair => new(pair.gate, pair.zone, mode == ReachableMode.Traversable && pair.gate.IsTraversable)).ToArray();
        _neighborsToAdd = null!;
    }
}

internal sealed class AreaNode : GraphNode<LG_Area, AreaNode>
{
    public AreaNode(LG_Area area, GroupManager<AreaNode> manager) : base(area, manager) { }

    internal override void CreateEdges(ReachableMode mode)
    {
        List<NodeEdge<AreaNode>> edges = new();
        foreach (var gate in Base.m_gates)
        {
            var area = gate.m_linksFrom;
            if (area == null) continue;

            if (area.UID == Base.UID)
                area = gate.m_linksTo;

            if (area == null || gate.ExpanderStatus == LG_ZoneExpanderStatus.Blocked) continue;

            edges.Add(new(gate, _manager.GetNode(area), mode == ReachableMode.Traversable && gate.IsTraversable));
        }
        Edges = edges.ToArray();
    }
}

internal abstract class GraphNode<T, E> : GraphNode<E>
    where E : GraphNode<T, E>
    where T : MonoBehaviour
{
    public readonly T Base;

    protected readonly GroupManager<E> _manager;

    public GraphNode(T baseObj, GroupManager<E> manager)
    {
        Base = baseObj;
        _manager = manager;
    }
}

internal abstract class GraphNode<E> : GraphNode
    where E : GraphNode<E>
{
    public NodeEdge<E>[] Edges { get; private protected set; } = null!;

    internal void OnEdgesCreated()
    {
        foreach (var edge in Edges)
        {
            foreach (var otherEdge in edge.Neighbor.Edges)
            {
                if (otherEdge.Neighbor == this)
                {
                    edge.Reverse = otherEdge;
                    break;
                }
            }
        }
    }

    internal bool UpdateEdge(GraphNode<E> neighbor, bool state)
    {
        foreach (var edge in Edges)
        {
            if (edge.Neighbor == neighbor)
            {
                if (edge.IsOpen == state)
                    return false;
                edge.IsOpen = state;
                edge.Reverse.IsOpen = state;
                return true;
            }
        }
        Logger.Warn($"Tried to update edge but failed to find matching neighbor!");
        return false;
    }
}

public abstract class GraphNode
{
    public ushort Group { get; private set; } = ZoneGraphUtil.NoGroup;

    public virtual bool IsReachable(ushort group) => Group == group && Group != ZoneGraphUtil.NoGroup;

    public virtual bool IsReachable() => Group != ZoneGraphUtil.NoGroup;

    internal abstract void CreateEdges(ReachableMode mode);

    internal virtual void Reset() => Group = ZoneGraphUtil.NoGroup;

    internal virtual void SetGroup(ushort group) => Group = group;
}