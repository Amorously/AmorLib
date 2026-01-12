using BepInEx.Unity.IL2CPP;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace AmorLib.Dependencies;

[CallConstructorOnLoad]
public static class InjectLib_Wrapper
{
    public const string PLUGIN_GUID = "GTFO.InjectLib";
    public static bool IsLoaded { get; private set; } = false;
    public static JsonConverter? InjectLibConverter { get; private set; } = null;

    static InjectLib_Wrapper()
    {
        if (IL2CPPChainloader.Instance.Plugins.ContainsKey(PLUGIN_GUID))
        {
            IsLoaded = true;
            SetConverter();
        }
        Logger.Debug($"InjectLib is loaded: {IsLoaded}");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void SetConverter()
    {
        InjectLibConverter = new InjectLib.JsonNETInjection.Supports.InjectLibConnector();
    }
}