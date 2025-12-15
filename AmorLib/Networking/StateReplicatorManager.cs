using AmorLib.Utils.Extensions;

namespace AmorLib.Networking;

public static class StateReplicatorManager // WIP
{
    internal static readonly Dictionary<Type, HashSet<uint>> _reservedReplicators = new();

    public static void ReserveIDs<T>(params uint[] args)
    {
        var set = _reservedReplicators.GetOrAddNew(typeof(T));

        foreach (var id in args)
        {
            if (set.Contains(id))
            {
                Logger.Error($"StateReplicator {typeof(T)} already has reserved ID {id}");
                continue;
            } 
            set.Add(id);
        }
    }

    public static void DeregisterIds<T>(params uint[] args)
    {
        if (!_reservedReplicators.TryGetValue(typeof(T), out var set))
        {
            return;
        }

        foreach (var id in args)
        {
            set.Remove(id);
        }
    }
}
