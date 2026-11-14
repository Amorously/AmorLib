namespace AmorLib.Utils.ZoneGraphImpl;

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
