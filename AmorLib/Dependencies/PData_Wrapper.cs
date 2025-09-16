using AmorLib.Utils.Extensions;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace AmorLib.Dependencies;

[CallConstructorOnLoad]
public static class PData_Wrapper
{
    public const string PLUGIN_GUID = "MTFO.Extension.PartialBlocks";
    public static bool IsLoaded { get; private set; } = false;
    public static bool IsMainBranch { get; private set; } = false;
    public static JsonConverter? PersistentIDConverter { get; private set; } = null;

    static PData_Wrapper()
    {
        if (IL2CPPChainloader.Instance.Plugins.TryGetValue(PLUGIN_GUID, out var info))
        {
            try
            {
                IsMainBranch = info.VersionAtLeast("1.5.2");
                var pdType = AccessTools.TypeByName("MTFO.Ext.PartialData.JsonConverters.PersistentIDConverter");
                if (pdType != null && typeof(JsonConverter).IsAssignableFrom(pdType))
                {
                    PersistentIDConverter = (JsonConverter?)Activator.CreateInstance(pdType);
                    IsLoaded = PersistentIDConverter != null;
                }
            }
            catch (Exception ex)
            {
                IsLoaded = false;
                IsMainBranch = false;
                PersistentIDConverter = null;
                Logger.Error($"Exception thrown while reading data from PartialData:\n{ex}");
            }
        }

        Logger.Debug($"PartialData is loaded: {IsLoaded}, Version >= 1.5.2 (main branch): {IsMainBranch}");
        if (IsLoaded && !IsMainBranch)
        {
            Logger.Warn("AWOPartialDataFixer (PartialDataModCompatible) is deprecated, or older version! It will still work but it's recommended to switch to the main branch of PartialData");
        }
    }

    public static bool TryGetGUID(string text, out uint guid)
    {
        if (IsLoaded && IsMainBranch)
        {
            guid = GetGUID(text);
            return guid != 0u; 
        }
        guid = 0u;
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static uint GetGUID(string text)
    {
        if (MTFO.Ext.PartialData.PersistentIDManager.TryGetId(text, out uint id))
        {
            return id;
        }
        return 0u;
    }
}
