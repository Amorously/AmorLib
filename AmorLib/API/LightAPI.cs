using AIGraph;
using AmorLib.Utils;
using AmorLib.Utils.Extensions;
using GTFO.API;
using LevelGeneration;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace AmorLib.API;

[CallConstructorOnLoad]
public static class LightAPI
{
    internal static readonly ConcurrentDictionary<int, LightWorker> AllLightsMap = new();

    static LightAPI()
    {        
        LevelAPI.OnAfterBuildBatch += OnAfterZoneLightsBatch;
        LevelAPI.OnLevelCleanup += OnLevelCleanup;
    }

    private static void OnLevelCleanup()
    {
        AllLightsMap.Clear();
    }
    
    private static void OnAfterZoneLightsBatch(LG_Factory.BatchName batch)
    {
        if (batch == LG_Factory.BatchName.ZoneLights)
        {
            Logger.Debug($"All LightWorkers are setup. Count: {AllLightsMap.Count}");
        }
    }

    public static LightWorker? GetSpecificLightWorker(int instanceID)
    {
        return TryGetSpecificLightWorker(instanceID, out var worker) ? worker : null;
    }
    
    public static bool TryGetSpecificLightWorker(int instanceID, [NotNullWhen(true)] out LightWorker? worker)
    {
        if (!AllLightsMap.TryGetValue(instanceID, out worker))
        {
            Logger.Error($"Failed to find LightWorker: no LG_Light exists with instance id {instanceID}!");
            return false;
        }
        return true;
    }

    public static void ForEachWorker(this IEnumerable<LightWorker> lightWorkers, Action<LightWorker> action)
    {
        foreach (var worker in lightWorkers)
        {
            action(worker);
        }
    }

    public static IEnumerable<ILightModifier> AddLightModifiers(this IEnumerable<LightWorker> lightWorkers, Color color, float intensity, bool enabled, int priority = LightPriority.Normal)
    {
        return lightWorkers.Select(worker => worker.AddModifier(color, intensity, enabled, priority));
    }

    public static void ForEachMod(this IEnumerable<ILightModifier> lightModifiers, Action<ILightModifier> action)
    {
        foreach (var mod in lightModifiers)
        {
            action(mod);
        }
    }

    public static IEnumerable<LightWorker> GetLightWorkersInDimension(params eDimensionIndex[] args)
    {
        return AllLightsMap.Values.Where(light => args.Contains(light.OwnerZone.DimensionIndex));
    }
    
    public static IEnumerable<LightWorker> GetLightWorkersInZone(params (int, int, int)[] args)
    {
        return AllLightsMap.Values.Where(light => args.Contains(light.OwnerZone.ToIntTuple()));
    }

    public static IEnumerable<LightWorker> GetLightWorkersInZone(params LG_Zone[] args)
    {
        var zones = args.Select(z => z.ToIntTuple()).Distinct().ToArray();
        return GetLightWorkersInZone(zones);
    }

    public static IEnumerable<LightWorker> GetLightWorkersInNode(params AIG_CourseNode[] args)
    {
        var nodes = args.Select(n => n.m_searchID).ToHashSet();
        return AllLightsMap.Values.Where(light => nodes.Contains(light.SpawnNode.m_searchID));
    }    

    public static IEnumerable<LightWorker> GetLightWorkersInRange(Vector3 position, float range)
    {
        return AllLightsMap.Values.Where(light => light.Position.IsWithinSqrDistance(position, range, out _));
    }

    public static IEnumerable<LightWorker> GetLightWorkersInRange(eDimensionIndex dimension, Vector3 position, float range)
    {
        return AllLightsMap.Values.Where(light => light.OwnerZone.DimensionIndex == dimension && light.Position.IsWithinSqrDistance(position, range, out _));
    }
}
