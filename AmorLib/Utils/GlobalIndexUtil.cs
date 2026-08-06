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
                    instance.Dimension = TryGetDimension(instance.DimensionIndex, out var d) ? d : null;
                    instance.Zone = TryGetZone(instance.DimensionIndex, instance.Layer, instance.LocalIndex, out var z) ? z : null;
                }
            }
        }
    }

    private static void OnLevelCleanup()
    {
        BaseInstances.RemoveAll(weakRef =>
        {
            if (weakRef.TryGetTarget(out var instance))
            {
                instance.Dimension = null;
                instance.Zone = null;
                return false; 
            }
            return true;
        });
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

    public static bool TryGetZone(eDimensionIndex dimension, LG_LayerType layer, eLocalZoneIndex localIndex, [MaybeNullWhen(false)] out LG_Zone zone)
    {
        zone = null;
        if (Builder.CurrentFloor == null || GameStateManager.CurrentStateName < eGameStateName.Generating || GameStateManager.CurrentStateName > eGameStateName.InLevel)
        {
            Logger.Error($"TryGetZone({dimension}, {layer}, {localIndex}): Not in level!");
            return false;
        }

        if (TryGetDimension(dimension, out var dim))
        {
            foreach (var lg_Layer in dim.Layers)
            {
                if (lg_Layer.m_type == layer && lg_Layer.m_zonesByLocalIndex.ContainsKey(localIndex))
                {
                    zone = lg_Layer.m_zonesByLocalIndex[localIndex];
                    return zone != null;
                }
            }
        }
        return false;
    }

    public static bool TryGetDimension(eDimensionIndex dimension, [MaybeNullWhen(false)] out Dimension dim)
    {
        dim = null;
        if (Builder.CurrentFloor == null || GameStateManager.CurrentStateName < eGameStateName.Generating || GameStateManager.CurrentStateName > eGameStateName.InLevel)
        {
            Logger.Error($"TryGetDimension({dimension}): Not in level!");
            return false;
        }

        var indexMap = Builder.CurrentFloor.m_indexToDimensionMap;
        if (indexMap.ContainsKey(dimension))
        {
            dim = indexMap[dimension];
            return dim != null;
        }
        return false;
    }
}
