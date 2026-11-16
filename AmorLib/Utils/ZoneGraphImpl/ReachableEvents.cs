namespace AmorLib.Utils.ZoneGraphImpl;

/// <summary>Object used to group flag-based callbacks.</summary>
public sealed class ReachableEvents
{
    private static readonly int s_LayerLen;
    private static readonly int s_FlagLen;

    static ReachableEvents()
    {
        s_LayerLen = Enum.GetValues<GraphLayer>().Length;
        s_FlagLen = Enum.GetValues<GraphLayer>().Sum(val => (int)val) + 1;
    }

    private readonly Action?[,] _events = new Action[s_LayerLen, s_FlagLen];

    public EventAccessor this[GraphLayer layer, ReachableMode mode]
    {
        get => new(this, layer, mode);
        set { }
    }

    internal void Invoke(GraphLayer layer, ReachableMode mode)
    {
        _events[(int)layer, (int)mode]?.Invoke();
    }

    private void Add(GraphLayer layer, ReachableMode mode, Action action)
    {
        _events[(int)layer, (int)mode] += action;
    }

    private void Remove(GraphLayer layer, ReachableMode mode, Action action)
    {
        _events[(int)layer, (int)mode] -= action;
    }

    public readonly struct EventAccessor
    {
        private readonly ReachableEvents _owner;
        private readonly GraphLayer _layer;
        private readonly ReachableMode _mode;

        internal EventAccessor(ReachableEvents owner, GraphLayer layer, ReachableMode mode)
        {
            _owner = owner;
            _layer = layer;
            _mode = mode;
        }

        public static EventAccessor operator +(EventAccessor accessor, Action action)
        {
            accessor._owner.Add(accessor._layer, accessor._mode, action);
            return accessor;
        }

        public static EventAccessor operator -(EventAccessor accessor, Action action)
        {
            accessor._owner.Remove(accessor._layer, accessor._mode, action);
            return accessor;
        }
    }
}
