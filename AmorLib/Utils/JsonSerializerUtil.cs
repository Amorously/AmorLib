using AmorLib.Dependencies;
using AmorLib.Utils.JsonElementConverters;
using GTFO.API.JSON.Converters;
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
        var setting = new JsonSerializerOptions(JsonSerializer.DefaultSerializerSettings);

        if (useLocalizedText)
        {
            if (usePartialData && PartialData_Wrapper.IsLoaded)
            {
                setting.Converters.Add(new LocalizedTextPDataConverter()); // using pdata, gonna handle String GUID -> LocalizedText -> LocalizationDatablock uint id
            }
            else
            {
                setting.Converters.Add(new LocalizedTextConverter());
            }
        }

        if (usePartialData && PartialData_Wrapper.IsLoaded)
            setting.Converters.Add(PartialData_Wrapper.PersistentIDConverter!);

        if (useInjectLib && InjectLib_Wrapper.IsLoaded)
            setting.Converters.Add(InjectLib_Wrapper.InjectLibConverter!);

        return setting;
    }
}