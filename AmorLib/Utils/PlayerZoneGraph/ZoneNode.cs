using LevelGeneration;

namespace AmorLib.Utils.PlayerZoneGraph;

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
            children.Add(ZoneGraph.GetAreaNode(area));
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

        if (newGroup == ZoneGraph.NoGroup) return;

        count = _groups.GetValueOrDefault(newGroup);
        _groups[newGroup] = count + 1;
    }
}
