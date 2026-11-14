using AmorLib.Dependencies;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Runtime.CompilerServices;

namespace AmorLib;

[BepInPlugin("Amor.AmorLib", "AmorLib", "1.3.0")]
[BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(InjectLib_Wrapper.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(MTFO_Wrapper.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(PartialData_Wrapper.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
internal sealed class EntryPoint : BasePlugin
{
    public override void Load()
    {
        Logger.MLS = Log;
        new Harmony("Amor.AmorLib").PatchAll();
        CallAllAutoConstructors();
        Logger.Message("AmorLib is done loading!");
    }

    private void CallAllAutoConstructors()
    {
        var types = AccessTools.GetTypesFromAssembly(GetType().Assembly).Where(t => t.IsDefined(typeof(CallConstructorOnLoadAttribute), false));
        foreach (var type in types)
        {
            RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        }
    }
}