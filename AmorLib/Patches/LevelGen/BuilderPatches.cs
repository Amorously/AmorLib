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

    /* Borrowed from GTFO-API until it is updated */
    [HarmonyPatch(typeof(LG_Factory), nameof(LG_Factory.OnStart))]
    [HarmonyPostfix]
    [HarmonyWrapSafe]    
    private static void Post_FactoryStart() 
    {
        LevelEvents.FactoryStart();
    }

    [HarmonyPatch(typeof(LG_Factory), nameof(LG_Factory.NextBatch))]
    [HarmonyPrefix]
    [HarmonyWrapSafe]    
    private static void Pre_Batch(LG_Factory __instance) 
    {
        if (__instance.m_batchStep > -1)
        {
            LevelEvents.AfterBuildBatch(__instance.m_currentBatchName);
        }
    }

    [HarmonyPatch(typeof(LG_Factory), nameof(LG_Factory.NextBatch))]
    [HarmonyPostfix]
    [HarmonyWrapSafe]    
    private static void Post_Batch(LG_Factory __instance)
    {
        LevelEvents.BeforeBuildBatch(__instance.m_currentBatchName);
    }

    [HarmonyPatch(typeof(LG_Factory), nameof(LG_Factory.FactoryDone))]
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    private static void Post_FactoryDone() 
    {
        LevelEvents.FactoryFinished();
    } 
}
