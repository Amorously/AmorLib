using BepInEx.Unity.IL2CPP;
using System.Runtime.CompilerServices;

namespace AmorLib.Dependencies;

[CallConstructorOnLoad]
public static class MTFO_Wrapper
{
    public const string PLUGIN_GUID = "com.dak.MTFO";
    public static bool IsLoaded { get; private set; } = false;
    public static bool HasGameDataPath { get; private set; } = false;
    public static bool IsPluginGameDataPath { get; private set; } = false;
    public static string GameDataPath { get; private set; } = string.Empty;
    public static bool HasCustomContent { get; private set; } = false;
    public static string CustomPath { get; private set; } = string.Empty;
    public static bool IsHotReloadEnabled { get; private set; } = false;

    static MTFO_Wrapper()
    {
        if (IL2CPPChainloader.Instance.Plugins.ContainsKey(PLUGIN_GUID))
        {
            IsLoaded = true;
            Init();
        }
        Logger.Debug($"MTFO is loaded: {IsLoaded}");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Init()
    {
        HasGameDataPath = MTFO.Managers.ConfigManager.HasGameDataPath;
        IsPluginGameDataPath = MTFO.Managers.ConfigManager.IsPluginGameDataPath;
        GameDataPath = MTFO.Managers.ConfigManager.GameDataPath;
        HasCustomContent = MTFO.Managers.ConfigManager.HasCustomContent;
        CustomPath = MTFO.Managers.ConfigManager.CustomPath;
        IsHotReloadEnabled = MTFO.Managers.ConfigManager.IsHotReloadEnabled;
    }
}
