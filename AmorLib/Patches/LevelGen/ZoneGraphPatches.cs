using AIGraph;
using AmorLib.Utils;
using HarmonyLib;
using LevelGeneration;
using Player;
using static Il2CppSystem.Linq.Expressions.Interpreter.CastInstruction.CastInstructionNoT;

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
        if (node == null || __state?.NodeID == node.NodeID) return;
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
    private static void Post_GateIsTraversable(LG_Gate __instance, bool value, bool __state)
    {
        if (__state != value && __instance.ExpanderStatus == LG_ZoneExpanderStatus.Connected)
            ZoneGraphUtil.Current.Internal_OnDoorStateChanged(__instance, value);
    }

    [HarmonyPatch(typeof(LG_WeakDoor_Anim._DoorAnimSequence_d__26), nameof(LG_WeakDoor_Anim._DoorAnimSequence_d__26.MoveNext))]
    [HarmonyPostfix]
    private static void Post_WeakDoorAnimSequence(LG_WeakDoor_Anim._DoorAnimSequence_d__26 __instance, bool __result)
    {
        ZoneGraphUtil.Current.Internal_OnDoorAnimationChanged(__instance.__4__this.m_gate, __result);
    }

    [HarmonyPatch(typeof(LG_SecurityDoor_Anim), nameof(LG_SecurityDoor_Anim.Setup))]
    [HarmonyPostfix]
    private static void Post_SecurityDoorAnimSetup(LG_SecurityDoor_Anim __instance)
    {
        __instance.add_OnDoorOpenStarted((Action)(() =>
        {
            ZoneGraphUtil.Current.Internal_OnDoorAnimationChanged(__instance.m_gate, true);
        }));

        __instance.add_OnDoorIsOpen((Action)(() =>
        {
            ZoneGraphUtil.Current.Internal_OnDoorAnimationChanged(__instance.m_gate, false);
        }));
    }

    [HarmonyPatch(typeof(LG_SecurityDoor_Anim._CloseSecurityDoor_d__50), nameof(LG_SecurityDoor_Anim._CloseSecurityDoor_d__50.MoveNext))]
    [HarmonyPostfix]
    private static void Post_SecurityDoorAnimSequence(LG_SecurityDoor_Anim._CloseSecurityDoor_d__50 __instance, bool __result)
    {
        if (!__result)
        {
            ZoneGraphUtil.Current.Internal_OnDoorAnimationChanged(__instance.__4__this.m_gate, false);
        }
        else if (__instance.__1__state == 1)
        {
            ZoneGraphUtil.Current.Internal_OnDoorAnimationChanged(__instance.__4__this.m_gate, true);
        }
    }

    [HarmonyPatch(typeof(LG_ApexDoor_Anim), nameof(LG_ApexDoor_Anim.Setup))]
    [HarmonyPostfix]
    private static void Post_ApexDoorAnimSetup(LG_ApexDoor_Anim __instance)
    {
        __instance.add_OnDoorOpenStarted((Action)(() =>
        {
            ZoneGraphUtil.Current.Internal_OnDoorAnimationChanged(__instance.m_gate, true);
        }));

        __instance.add_OnDoorIsOpen((Action)(() =>
        {
            ZoneGraphUtil.Current.Internal_OnDoorAnimationChanged(__instance.m_gate, false);
        }));
    }

    [HarmonyPatch(typeof(LG_ApexDoor_Anim._CloseApexDoor_d__31), nameof(LG_ApexDoor_Anim._CloseApexDoor_d__31.MoveNext))]
    [HarmonyPostfix]
    private static void Post_ApexDoorAnimSequence(LG_ApexDoor_Anim._CloseApexDoor_d__31 __instance, bool __result)
    {
        if (!__result)
        {
            ZoneGraphUtil.Current.Internal_OnDoorAnimationChanged(__instance.__4__this.m_gate, false);
        }
        else if (__instance.__1__state == 1)
        {
            ZoneGraphUtil.Current.Internal_OnDoorAnimationChanged(__instance.__4__this.m_gate, true);
        }
    }
}
