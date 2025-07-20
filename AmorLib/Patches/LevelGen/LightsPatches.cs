using AmorLib.API;
using HarmonyLib;
using LevelGeneration;

namespace AmorLib.Patches.LevelGen;

[HarmonyPatch(typeof(LG_BuildZoneLightsJob), nameof(LG_BuildZoneLightsJob.Build))]
internal static class LightsPatches
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    [HarmonyWrapSafe]
    private static void Pre_ZoneBuild(LG_BuildZoneLightsJob __instance, out List<LightWorker>? __state)
    {
        var zone = __instance.m_zone;
        if (zone == null)
        {
            __state = null;
            return;
        }

        __state = new List<LightWorker>();
        foreach (var node in zone.m_courseNodes)
        {
            foreach (var light in node.m_area.GetComponentsInChildren<LG_Light>(false))
            {
                __state.Add(new LightWorker(zone, node, light, light.GetInstanceID(), light.m_intensity));
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.High)]
    [HarmonyWrapSafe]
    private static void Post_ZoneBuild(LG_BuildZoneLightsJob __instance, bool __result, List<LightWorker>? __state)
    {
        if (__instance.m_zone == null || !__result || __state == null)
        {
            return;
        }

        foreach (var worker in __state)
        {
            worker.Setup();         
            LightAPI.AllLightsMap.TryAdd(worker.InstanceID, worker);
        }
    }
}
