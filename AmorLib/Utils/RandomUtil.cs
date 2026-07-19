namespace AmorLib.Utils;

public static class RandomUtil
{
    /// <summary>
    /// Creates a new <see cref="System.Random"/> instance seeded with ActiveExpedition.SessionSeed and an offset key.
    /// </summary>
    public static System.Random CreateSessionRandom(string offsetKey)
    {
        int seed = RundownManager.ActiveExpedition.SessionSeed ^ HashOffsetKey(offsetKey);
        return new System.Random(seed);
    }

    private static int HashOffsetKey(string offsetKey)
    {
        uint hash = 2166136261u; // FNV-1a offset basis
        foreach (char c in offsetKey)
        {
            hash ^= c;
            hash *= 16777619u; // FNV-1a prime
        }
        return unchecked((int)hash);
    }
}
