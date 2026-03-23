using LevelGeneration;

namespace AmorLib.Utils.PlayerZoneGraph;

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

    public ushort Group { get; private set; } = ZoneGraph.NoGroup;

    public ZoneNode Zone { get; private set; } = null!;
    public AreaEdge[] Edges { get; private set; } = null!;

    public AreaNode(LG_Area area)
    {
        Area = area;
    }

    public bool IsReachable(ushort group) => Group == group && group > 0;
    public bool IsReachable() => Group != ZoneGraph.NoGroup;

    internal void OnNodesCreated()
    {
        Zone = ZoneGraph.GetZoneNode(Area.m_zone);
        List<AreaEdge> edges = new();
        foreach (var gate in Area.m_gates)
        {
            var area = gate.m_linksFrom;
            if (area == null) continue;

            if (area.UID == Area.UID)
                area = gate.m_linksTo;

            if (area == null || gate.ExpanderStatus == LG_ZoneExpanderStatus.Blocked) continue;

            edges.Add(new(gate, ZoneGraph.GetAreaNode(area)));
        }
        Edges = edges.ToArray();
    }

    internal void Reset()
    {
        Group = ZoneGraph.NoGroup;
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
