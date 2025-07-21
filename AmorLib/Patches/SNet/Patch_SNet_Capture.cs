using AmorLib.Events;
using HarmonyLib;
using SNetwork;

namespace AmorLib.Patches.SNet;

[HarmonyPatch(typeof(SNet_Capture))]
internal static class Patch_SNet_Capture
{
    [HarmonyPatch(nameof(SNet_Capture.TriggerCapture))]
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    private static void Pre_TriggerCapture(SNet_Capture __instance)
    {
        SNetEvents.BufferCaptured(__instance.PrimedBufferType);
    }

    [HarmonyPatch(nameof(SNet_Capture.RecallBuffer))]
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    private static void Post_RecallBuffer(SNet_Capture __instance, eBufferType bufferType)
    {
        if (__instance.IsRecalling) return; 

        SNetEvents.BufferRecalled(bufferType);
    }
}