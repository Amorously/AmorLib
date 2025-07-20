using BepInEx.Unity.IL2CPP;
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
            IsLoaded = true;
            try
            {
                InjectLibConverter = (JsonConverter)Activator.CreateInstance(typeof(InjectLib.JsonNETInjection.Supports.InjectLibConnector))!;
            }
            catch (Exception ex)
            {
                Logger.Error($"Exception thrown while reading data from PartialData:\n{ex}");
            }
        }

        Logger.Debug($"InjectLib is loaded: {IsLoaded}");
    }
}
