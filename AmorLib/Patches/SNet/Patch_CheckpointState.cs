using AmorLib.Events;
using HarmonyLib;

namespace AmorLib.Patches.SNet;

[HarmonyPatch(typeof(CheckpointManager), nameof(CheckpointManager.OnStateChange))]
internal static class Patch_CheckpointState
{
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void Post_CheckpointStateChange(pCheckpointState newState)
    {
        if (newState.lastInteraction == eCheckpointInteractionType.ReloadCheckpoint)
        {
            SNetEvents.CheckpointReloaded();
        }        
    }
}
