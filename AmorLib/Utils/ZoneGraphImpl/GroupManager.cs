using AIGraph;
using LevelGeneration;
using Player;

namespace AmorLib.Utils.ZoneGraphImpl;

internal sealed class GroupManager<E> where E : GraphNode<E>
{
    private readonly Dictionary<int, E> _idToNode = new();
    private readonly Dictionary<int, ushort> _playerToGroup = new();
    private readonly GraphLayer _layer;
    private readonly ReachableMode _mode;
    private ushort _lastGroup;
    private ushort NextGroup
    {
        get
        {
            if (++_lastGroup == ZoneGraphUtil.NoGroup)
                ++_lastGroup;
            return _lastGroup;
        }
    }

    public GroupManager(GraphLayer layer, ReachableMode settings)
    {
        _layer = layer;
        _mode = settings;
    }

    public IReadOnlyCollection<E> Nodes => _idToNode.Values;
    public IReadOnlyDictionary<int, ushort> PlayerGroups => _playerToGroup;

    public bool IsReachable(PlayerAgent player, int id) => _idToNode[id].IsReachable(_playerToGroup.GetValueOrDefault(player.PlayerSlotIndex));
    public bool IsReachable(int id) => _idToNode[id].IsReachable();

    internal E GetNode(PlayerAgent player) => GetNode(player.CourseNode);
    internal E GetNode(AIG_CourseNode courseNode)
    {
        if (_layer == GraphLayer.Area)
            return _idToNode[courseNode.m_area.UID];
        else
            return _idToNode[courseNode.m_zone.ID];
    }
    internal E GetNode(LG_Area area)
    {
        if (_layer == GraphLayer.Area)
            return _idToNode[area.UID];
        else
            return _idToNode[area.m_zone.ID];
    }
    internal E GetNode(LG_Zone zone)
    {
        return _idToNode[zone.ID];
    }

    internal void Internal_AddNode(int id, E node) => _idToNode.TryAdd(id, node);
    internal void Internal_CreateEdges()
    {
        foreach (var node in _idToNode.Values)
            node.CreateEdges(_mode);
    }
    internal void Internal_OnEdgesCreated()
    {
        foreach (var node in _idToNode.Values)
            node.OnEdgesCreated();
    }

    internal void Internal_Cleanup()
    {
        _idToNode.Clear();
        _playerToGroup.Clear();
        _lastGroup = 0;
    }

    internal void Internal_OnCheckpoint()
    {
        foreach (var node in _idToNode.Values)
        {
            foreach (var edge in node.Edges)
            {
                if (_mode == ReachableMode.Changing)
                    edge.IsOpen = edge.IsChanging;
                else
                    edge.IsOpen = edge.Gate.IsTraversable;
            }
        }
        RefreshGraph();
    }

    // Reset all nodes to be unreachable, then discover reachable nodes from all players.
    private void RefreshGraph()
    {
        foreach (var node in _idToNode.Values)
            node.Reset();
        _playerToGroup.Clear();
        _lastGroup = 0;

        foreach (var player in PlayerManager.PlayerAgentsInLevel)
        {
            if (player.CourseNode == null) continue;

            UpdateOrCreateGroup(player, out _);
        }
        ZoneGraphUtil.OnReachableTypeUpdate.Invoke(_layer, _mode);
    }

    // Updates the player's group to match the node they're in, or create one if it doesn't exist.
    private bool UpdateOrCreateGroup(PlayerAgent player, out ushort group)
    {
        group = 0;
        var node = GetNode(player);
        if (node.IsReachable())
        {
            group = node.Group;
        }
        else
        {
            group = NextGroup;
            PropagateGroup(node, group);
        }

        if (!_playerToGroup.TryGetValue(player.PlayerSlotIndex, out var oldGroup) || oldGroup != group)
        {
            _playerToGroup[player.PlayerSlotIndex] = group;
            return true;
        }
        return false;
    }

    // Propagate the group to all connected areas.
    private static void PropagateGroup(E node, ushort group)
    {
        node.SetGroup(group);
        foreach (var edge in node.Edges)
        {
            if (edge.IsOpen && edge.Neighbor.Group != group)
            {
                PropagateGroup(edge.Neighbor, group);
            }
        }
    }

    internal bool Internal_OnPlayerNodeChanged(PlayerAgent player, AIG_CourseNode? oldCourseNode)
    {
        bool changed = UpdateOrCreateGroup(player, out var newGroup);
        if (oldCourseNode != null)
        {
            var oldNode = GetNode(oldCourseNode);
            // Movement within the same group, no update needed.
            if (!oldNode.IsReachable(newGroup))
            {
                // If no players reachable, revoke reachable status.
                if (_playerToGroup.Values.All(group => !oldNode.IsReachable(group)))
                {
                    PropagateGroup(oldNode, ZoneGraphUtil.NoGroup);
                    changed = true;
                }
            }
        }
        ZoneGraphUtil.OnReachableTypeUpdate.Invoke(_layer, _mode);
        return changed;
    }

    internal bool Internal_OnEdgeStateChanged(LG_Gate gate, bool state)
    {
        var from = GetNode(gate.m_linksFrom);
        var to = GetNode(gate.m_linksTo);

        // Update graph edges to match door state.
        if (!from.UpdateEdge(to, state))
            return false;

        if (!state)
        {
            // Open -> closed; either both need an update, or neither does.
            if (from.IsReachable())
            {
                RefreshGraph();
                return true;
            }
        }
        else
        {
            bool TryPropagate(E oldNode, E newNode)
            {
                if (!newNode.IsReachable()) return false;

                foreach ((var index, var group) in _playerToGroup.ToArray())
                    if (group == oldNode.Group)
                        _playerToGroup[index] = newNode.Group;

                PropagateGroup(oldNode, newNode.Group);
                return true;
            }

            // Expand the reachable side or merge the two sides if both are reachable.
            if (TryPropagate(to, from) || TryPropagate(from, to))
            {
                ZoneGraphUtil.OnReachableTypeUpdate.Invoke(_layer, _mode);
                return true;
            }
        }
        return false;
    }
}