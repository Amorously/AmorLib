using AIGraph;
using AmorLib.Utils;
using BepInEx.Unity.IL2CPP;
using GTFO.API;
using GTFO.API.Extensions;
using HarmonyLib;
using LevelGeneration;
using UnityEngine;

namespace AmorLib.Patches.LevelGen;

[HarmonyPatch]
internal static class TerminalPatches
{
    private static readonly Dictionary<(int, int, int), LG_ComputerTerminal> ReactorTerminals = new();

    static TerminalPatches()
    {
        LevelAPI.OnAfterBuildBatch += OnAfterBatchBuild;
        LevelAPI.OnLevelCleanup += OnLevelCleanup;
    }

    private static void OnAfterBatchBuild(LG_Factory.BatchName batch)
    {
        if (batch != LG_Factory.BatchName.SpecificSpawning) return;

        foreach (var kvp in ReactorTerminals)
        {
            if (kvp.Key.TryGetZone(out var zone) && !zone.TerminalsSpawnedInZone.ToManaged().Any(term => term.SyncID == kvp.Value.SyncID))
            {
                zone.TerminalsSpawnedInZone.Add(kvp.Value);
                Logger.Debug("Appended reactor terminal to its TerminalsSpawnedInZone");
            }
        }
    }

    private static void OnLevelCleanup()
    {
        ReactorTerminals.Clear();
    }

    [HarmonyPatch(typeof(LG_ComputerTerminalCommandInterpreter), nameof(LG_ComputerTerminalCommandInterpreter.ReceiveCommand))]
    [HarmonyPrefix]
    [HarmonyPriority(Priority.VeryHigh)]
    [HarmonyWrapSafe]
    private static void HiddenCommandExecutionFix(LG_ComputerTerminalCommandInterpreter __instance, ref TERM_Command cmd)
    {
        if (!LG_ComputerTerminalCommandInterpreter.m_alwaysHiddenCommands.Contains(cmd))
        {
            switch (cmd)
            {
                case TERM_Command.None:
                case TERM_Command.Open:
                case TERM_Command.Close:
                case TERM_Command.InvalidCommand:
                case TERM_Command.DownloadData:
                case TERM_Command.ActivateBeacon:
                case TERM_Command.TryUnlockingTerminal:
                    break;

                default:
                    if (__instance.m_terminal.CommandIsHidden(cmd))
                    {
                        cmd = TERM_Command.InvalidCommand;
                    }
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(LG_WardenObjective_Reactor), nameof(LG_WardenObjective_Reactor.Start))]
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    private static void CustomReactorTerminalFix(LG_WardenObjective_Reactor __instance) // ripped from FlowGeos
    {
        if (IL2CPPChainloader.Instance.Plugins.ContainsKey("FlowGeos") || __instance.m_terminalPrefab != null) return;

        Logger.Info("Resolving terminal prefab for reactor");
        var prefab = AssetAPI.GetLoadedAsset<GameObject>("Assets/AssetPrefabs/Complex/Generic/FunctionMarkers/Terminal_Floor.prefab");
        if (prefab == null)
        {
            Logger.Error("Failed to find terminal prefab loaded asset?");
            return;
        }
        __instance.m_terminalPrefab = prefab;
    }

    [HarmonyPatch(typeof(LG_ComputerTerminal), nameof(LG_ComputerTerminal.SpawnNode), MethodType.Getter)]
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    private static bool ReactorTerminalSpawnNodeFix(LG_ComputerTerminal __instance, ref AIG_CourseNode __result) // ripped from EOS
    {
        if (__instance.ConnectedReactor != null && __instance.m_terminalItem.SpawnNode == null)
        {
            __result = __instance.ConnectedReactor.SpawnNode;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(LG_WardenObjective_Reactor), nameof(LG_WardenObjective_Reactor.GenericObjectiveSetup))]
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    private static void ReactorTerminalInZoneFix(LG_WardenObjective_Reactor __instance) // ripped from AWO
    {
        if (__instance.SpawnNode.m_zone.TerminalsSpawnedInZone != null && __instance.m_terminal != null)
        {
            ReactorTerminals.Add(__instance.SpawnNode.m_zone.ToIntTuple(), __instance.m_terminal);
        }
    }
}