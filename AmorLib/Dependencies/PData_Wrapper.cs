using AmorLib.Utils.Extensions;
using BepInEx.Unity.IL2CPP;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace AmorLib.Dependencies;

[CallConstructorOnLoad]
public static class PData_Wrapper
{
    public const string PLUGIN_GUID = "MTFO.Extension.PartialBlocks";
    public static bool IsLoaded { get; private set; } = false;
    public static bool IsInitialized { get; private set; } = false;
    public static bool IsMainBranch { get; private set; } = false;
    public static JsonConverter? PersistentIDConverter { get; private set; } = null;

    static PData_Wrapper()
    {
        if (IL2CPPChainloader.Instance.Plugins.TryGetValue(PLUGIN_GUID, out var info))
        {
            IsLoaded = true;
            IsInitialized = MTFO.Ext.PartialData.PartialDataManager.Initialized;
            IsMainBranch = info.VersionAtLeast("1.5.2");
            try
            {
                PersistentIDConverter = (JsonConverter)Activator.CreateInstance(typeof(MTFO.Ext.PartialData.JsonConverters.PersistentIDConverter))!;
            } 
            catch (Exception ex)
            {
                Logger.Error($"Exception thrown while reading data from PartialData:\n{ex}");
            }
        }

        Logger.Debug($"PartialData is loaded and initialized: {IsLoaded && IsInitialized}, Version at least \"1.5.2\" (main branch): {IsMainBranch}");
        if (IsLoaded && !IsMainBranch)
        {
            Logger.Warn("AWOPartialDataFixer (PartialDataModCompatible) is deprecated! It will still work, but it's recommended to switch to the main branch of PartialData");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool TryGetGUID(string text, out uint guid)
    {
        if (MTFO.Ext.PartialData.PersistentIDManager.TryGetId(text, out uint id))
        {
            guid = id;
            return true;
        }

        guid = 0u;
        return false;
    }
}
