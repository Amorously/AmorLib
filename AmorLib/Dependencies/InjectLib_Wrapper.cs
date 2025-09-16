using BepInEx.Unity.IL2CPP;
using HarmonyLib;
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
        if (IL2CPPChainloader.Instance.Plugins.ContainsKey(PLUGIN_GUID))
        {
            try
            {
                var ilType = AccessTools.TypeByName("InjectLib.JsonNETInjection.Supports.InjectLibConnector");
                if (ilType != null && typeof(JsonConverter).IsAssignableFrom(ilType))
                {
                    InjectLibConverter = (JsonConverter?)Activator.CreateInstance(ilType);
                    IsLoaded = InjectLibConverter != null;
                }
            }
            catch (Exception ex)
            {
                IsLoaded = false;
                Logger.Error($"Exception thrown while reading data from InjectLib:\n{ex}");
            }
        }

        Logger.Debug($"InjectLib is loaded: {IsLoaded}");
    }
}
