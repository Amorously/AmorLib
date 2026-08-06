using AmorLib.Events;
using HarmonyLib;

namespace AmorLib.Patches.SNet;

[HarmonyPatch(typeof(CheckpointManager), nameof(CheckpointManager.OnStateChange))]
internal static class Patch_CheckpointState
{
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void Post_CheckpointStateChange(pCheckpointState oldState, pCheckpointState newState, bool isRecall)
    {
        if (!oldState.isReloadingCheckpoint && !newState.isReloadingCheckpoint)
        {
            // Ignore cases:
            // Client syncs on drop with isRecall: true.
            // Client runs a redundant StoreCheckpoint call w/ no changes prior to any change.
            if (isRecall || oldState.doorLockPosition == newState.doorLockPosition)
                return;

            SNetEvents.CheckpointReached();
        }
        else if (oldState.isReloadingCheckpoint && isRecall)
        {
            SNetEvents.CheckpointReloaded();
        }
    }
}
