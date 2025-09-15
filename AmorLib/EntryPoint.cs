using AmorLib.Dependencies;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Runtime.CompilerServices;

namespace AmorLib;

[BepInPlugin("Amor.AmorLib", "AmorLib", "1.0.4")]
[BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(InjectLib_Wrapper.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(PData_Wrapper.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
internal sealed class EntryPoint : BasePlugin
{
    public override void Load()
    {
        new Harmony("Amor.AmorLib").PatchAll();
        CallAllAutoConstructors();
        Logger.Info("AmorLib is done loading!");
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