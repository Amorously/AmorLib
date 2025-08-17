using AmorLib.Dependencies;
using System.Text.Json;
using JsonSerializer = GTFO.API.JSON.JsonSerializer; // goes to GTFO-API NativeSerializer -> System.Text.Json.JsonSerializer

namespace AmorLib.Utils;

public static class JsonSerializerUtil
{
    /// <summary>
    /// Creates a <see cref="JsonSerializerOptions"/> instance configured with the default serialization options.
    /// </summary>
    /// <returns>A default <see cref="JsonSerializerOptions"/> instance with the specified converters added.</returns>
    public static JsonSerializerOptions CreateDefaultSettings(bool useLocalizedText = true, bool usePartialData = false, bool useInjectLib = false)
    {
        JsonSerializerOptions setting = useLocalizedText ? new(JsonSerializer.DefaultSerializerSettingsWithLocalizedText) : new(JsonSerializer.DefaultSerializerSettings);

        if (usePartialData && PData_Wrapper.IsLoaded)
            setting.Converters.Add(PData_Wrapper.PersistentIDConverter!);

        if (useInjectLib && InjectLib_Wrapper.IsLoaded)
            setting.Converters.Add(InjectLib_Wrapper.InjectLibConverter!);

        return setting;
    }
}