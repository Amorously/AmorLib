using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace AmorLib.Dependencies;

[CallConstructorOnLoad]
public class InjectLib_Wrapper
{
    public const string PLUGIN_GUID = "GTFO.InjectLib";
    public static bool IsLoaded { get; private set; } = false;
    public static JsonConverter? InjectLibConverter { get; private set; } = null;

    static InjectLib_Wrapper()
    {
        Setup();
        Logger.Debug($"InjectLib is loaded: {IsLoaded}");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Setup()
    {
        if (!IL2CPPChainloader.Instance.Plugins.ContainsKey(PLUGIN_GUID)) return;

        try
        {
            var ilAsm = AccessTools.TypeByName("InjectLib.JsonNETInjection.Supports.InjectLibConnector");
            if (ilAsm != null && typeof(JsonConverter).IsAssignableFrom(ilAsm))
            {
                InjectLibConverter = (JsonConverter?)Activator.CreateInstance(ilAsm);
                IsLoaded = InjectLibConverter != null;
            }
        }
        catch (Exception ex)
        {
            IsLoaded = false;
            Logger.Error($"Exception thrown while reading data from InjectLib:\n{ex}");
        }
    }
}
