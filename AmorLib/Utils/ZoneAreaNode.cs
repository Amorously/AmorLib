using LevelGeneration;

namespace AmorLib.Utils;

public sealed class ZoneNode
{
    public readonly LG_Zone Zone;
    public AreaNode[] Areas { get; private set; } = null!;
    private readonly Dictionary<ushort, int> _groups = new();
    public IReadOnlyCollection<ushort> Groups => _groups.Keys;

    public ZoneNode(LG_Zone zone)
    {
        Zone = zone;
    }

    internal void OnNodesCreated()
    {
        List<AreaNode> children = new(Zone.m_areas.Count);
        foreach (var area in Zone.m_areas)
            children.Add(ZoneGraphUtil.GetAreaNode(area));
        Areas = children.ToArray();
    }

    public bool IsReachable(ushort group) => _groups.ContainsKey(group);
    public bool IsReachable() => _groups.Count > 0;

    internal void Reset()
    {
        _groups.Clear();
    }

    internal void OnAreaReachable(ushort newGroup, ushort oldGroup)
    {
        if (_groups.TryGetValue(oldGroup, out var count) && --count == 0)
            _groups.Remove(oldGroup);

        if (newGroup == ZoneGraphUtil.NoGroup) return;

        count = _groups.GetValueOrDefault(newGroup);
        _groups[newGroup] = count + 1;
    }
}


public sealed class AreaNode
{
    public class AreaEdge
    {
        public readonly LG_Gate Gate;
        public readonly AreaNode Neighbor;

        public bool IsOpen { get; private set; }

        public AreaEdge(LG_Gate gate, AreaNode area)
        {
            Gate = gate;
            Neighbor = area;
            UpdateOpen();
        }

        internal void UpdateOpen() => IsOpen = Gate.IsTraversable;
    }

    public readonly LG_Area Area;

    public ushort Group { get; private set; } = ZoneGraphUtil.NoGroup;

    public ZoneNode Zone { get; private set; } = null!;
    public AreaEdge[] Edges { get; private set; } = null!;

    public AreaNode(LG_Area area)
    {
        Area = area;
    }

    public bool IsReachable(ushort group) => Group == group && group > 0;
    public bool IsReachable() => Group != ZoneGraphUtil.NoGroup;

    internal void OnNodesCreated()
    {
        Zone = ZoneGraphUtil.GetZoneNode(Area.m_zone);
        List<AreaEdge> edges = new();
        foreach (var gate in Area.m_gates)
        {
            var area = gate.m_linksFrom;
            if (area == null) continue;

            if (area.UID == Area.UID)
                area = gate.m_linksTo;

            if (area == null || gate.ExpanderStatus == LG_ZoneExpanderStatus.Blocked) continue;

            edges.Add(new(gate, ZoneGraphUtil.GetAreaNode(area)));
        }
        Edges = edges.ToArray();
    }

    internal void Reset()
    {
        Group = ZoneGraphUtil.NoGroup;
    }

    internal void SetGroup(ushort group)
    {
        Zone.OnAreaReachable(group, Group);
        Group = group;
    }

    internal void UpdateEdges()
    {
        foreach (var edge in Edges)
            edge.UpdateOpen();
    }
}