using GameData;
using GTFO.API;
using LevelGeneration;
using System.Diagnostics.CodeAnalysis;

namespace AmorLib.Utils;

public static class GlobalIndexUtil
{
    internal static readonly List<WeakReference<GlobalBase>> BaseInstances = new();

    static GlobalIndexUtil()
    {
        LevelAPI.OnAfterBuildBatch += OnAfterBuildBatch;
        LevelAPI.OnLevelCleanup += OnLevelCleanup;
    }

    private static void OnAfterBuildBatch(LG_Factory.BatchName batch)
    {
        if (batch == LG_Factory.BatchName.Geomorphs)
        {
            foreach (var weakRef in BaseInstances)
            {
                if (weakRef.TryGetTarget(out var instance))
                {
                    instance.Dimension = Dimension.GetDimension(instance.DimensionIndex, out var d) ? d : null;
                    instance.Zone = TryGetZone(instance.DimensionIndex, instance.Layer, instance.LocalIndex, out var z) ? z : null;
                }
            }
        }
    }
    
    private static void OnLevelCleanup()
    {
        foreach (var weakRef in BaseInstances)
        {
            if (weakRef.TryGetTarget(out var instance))
            {
                instance.Dimension = null;
                instance.Zone = null;
            }
            else
            {
                BaseInstances.Remove(weakRef);
            }
        }
    }

    public static (int dimension, int layer, int zone) ToIntTuple(this LG_Zone zone)
    {
        return ToIntTuple(zone.DimensionIndex, zone.Layer.m_type, zone.LocalIndex);        
    }
    
    public static (int dimension, int layer, int zone) ToIntTuple(this GlobalZoneIndex globalIndex)
    {
        return ToIntTuple(globalIndex.Dimension, globalIndex.Layer, globalIndex.Zone);
    }    

    public static (int dimension, int layer, int zone) ToIntTuple(eDimensionIndex dimension, LG_LayerType layer, eLocalZoneIndex zone)
    {
        return ((int)dimension, (int)layer, (int)zone);
    }

    public static GlobalZoneIndex ToStruct(this LG_Zone zone)
    {
        return ToStruct(zone.DimensionIndex, zone.Layer.m_type, zone.LocalIndex);
    }

    public static GlobalZoneIndex ToStruct(eDimensionIndex dimension, LG_LayerType layer, eLocalZoneIndex zone)
    {
        return new(dimension, layer, zone);
    }

    public static bool TryGetZone(this (int, int, int) index, [MaybeNullWhen(false)] out LG_Zone zone)
    {
        return TryGetZone((eDimensionIndex)index.Item1, (LG_LayerType)index.Item2, (eLocalZoneIndex)index.Item3, out zone);
    }

    public static bool TryGetZone(this GlobalZoneIndex index, [MaybeNullWhen(false)] out LG_Zone zone)
    {
        return TryGetZone(index.Dimension, index.Layer, index.Zone, out zone);
    }

    public static bool TryGetZone(eDimensionIndex dimension, LG_LayerType layer, eLocalZoneIndex localIndex, out LG_Zone? zone)
    {
        if (!Builder.CurrentFloor.TryGetZoneByLocalIndex(dimension, layer, localIndex, out zone))
        {
            if (GameStateManager.CurrentStateName == eGameStateName.Generating)
            {
                Logger.Error($"Unable to find zone in level! (Dimension: {dimension}, Layer: {layer}, LocalIndex: {localIndex})");
            }
            return false;
        }
        return true;
    }
}
