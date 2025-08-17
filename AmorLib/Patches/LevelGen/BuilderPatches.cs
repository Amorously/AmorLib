using AmorLib.Events;
using HarmonyLib;
using LevelGeneration;

namespace AmorLib.Patches.LevelGen;

[HarmonyPatch]
internal static class BuilderPatches
{
    [HarmonyPatch(typeof(Builder), nameof(Builder.BuildDone))]
    [HarmonyPostfix]
    [HarmonyBefore("dev.gtfomodding.gtfo-api")]
    [HarmonyWrapSafe]
    private static void BuildDone_Early() 
    { 
        LevelEvents.BuildDoneEarly();
    }

    [HarmonyPatch(typeof(Builder), nameof(Builder.BuildDone))]
    [HarmonyPostfix]
    [HarmonyAfter("dev.gtfomodding.gtfo-api")]
    [HarmonyWrapSafe]
    private static void BuildDone_Late()
    { 
        LevelEvents.BuildDoneLate();
    }
}
