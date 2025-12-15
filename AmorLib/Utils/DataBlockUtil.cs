using AmorLib.Dependencies;
using GameData;
using System.Diagnostics.CodeAnalysis;

namespace AmorLib.Utils;

public static class DataBlockUtil
{
    public static bool TryGetBlock<T>(string name, [MaybeNullWhen(false)] out T block) where T : GameDataBlockBase<T>
    {
        if (PData_Wrapper.TryGetGUID(name, out uint id))
            return TryGetBlock(id, out block);

        block = GameDataBlockBase<T>.GetBlock(name);
        return block != null && block.internalEnabled;
    }
    
    public static bool TryGetBlock<T>(uint id, [MaybeNullWhen(false)] out T block) where T : GameDataBlockBase<T>
    {
        block = GameDataBlockBase<T>.GetBlock(id);
        return block != null && block.internalEnabled;
    }    
}
