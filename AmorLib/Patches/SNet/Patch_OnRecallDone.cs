using HarmonyLib;
using SNetwork;

namespace AmorLib.Patches.SNet;

[HarmonyPatch(typeof(SNet_SyncManager), nameof(SNet_SyncManager.OnRecallDone))]
internal static class Patch_OnRecallDone
{    
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    private static void Post_RecallDone()
    {
        Events.SNetEvents.RecallDone();
    }
}