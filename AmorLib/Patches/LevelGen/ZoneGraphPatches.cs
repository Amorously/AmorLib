using AIGraph;
using AmorLib.Utils;
using HarmonyLib;
using LevelGeneration;
using Player;

namespace AmorLib.Patches.LevelGen;

[HarmonyPatch]
internal static class ZoneGraphPatches
{
    [HarmonyPatch(typeof(PlayerAgent), nameof(PlayerAgent.CourseNode), MethodType.Setter)]
    [HarmonyPrefix]
    private static void Pre_PlayerNodeSet(PlayerAgent __instance, ref AIG_CourseNode __state)
    {
        __state = __instance.CourseNode;
    }

    [HarmonyPatch(typeof(PlayerAgent), nameof(PlayerAgent.CourseNode), MethodType.Setter)]
    [HarmonyPostfix]
    private static void Post_PlayerNodeSet(PlayerAgent __instance, AIG_CourseNode __state)
    {
        var node = __instance.CourseNode;
        if (node == null || __state == node) return;
        ZoneGraphUtil.Current.Internal_OnPlayerNodeChanged(__instance, __state);
    }

    [HarmonyPatch(typeof(LG_Gate), nameof(LG_Gate.IsTraversable), MethodType.Setter)]
    [HarmonyPrefix]
    private static void Pre_GateIsTraversable(LG_Gate __instance, ref bool __state)
    {
        __state = __instance.IsTraversable;
    }

    [HarmonyPatch(typeof(LG_Gate), nameof(LG_Gate.IsTraversable), MethodType.Setter)]
    [HarmonyPostfix]
    private static void Post_GateIsTraversable(LG_Gate __instance, bool __state)
    {
        if (__state !=  __instance.IsTraversable && __instance.ExpanderStatus == LG_ZoneExpanderStatus.Connected)
            ZoneGraphUtil.Current.Internal_OnDoorStateChanged(__instance, !__state);
    }
}
