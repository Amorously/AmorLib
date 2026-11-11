using AmorLib.Dependencies;
using GTFO.API.JSON.Converters;
using Localization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmorLib.Utils.JsonElementConverters;

public sealed class LocalizedTextPDataConverter : JsonConverter<LocalizedText>
{
    public override bool HandleNull => false;

    public override LocalizedText Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var text = reader.GetString()!;

            var localizedText = new LocalizedText();

            if (PartialData_Wrapper.TryGetGUID(text, out uint id))
            {
                localizedText.Id = id;
                localizedText.UntranslatedText = null;
            }

            else
            {
                localizedText.Id = 0;
                localizedText.UntranslatedText = text;
            }

            return localizedText;
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            return new() {
                Id = reader.GetUInt32(),
                UntranslatedText = null,
            };
        }

        throw new JsonException($"LocalizedTextJson type: {reader.TokenType} not implemented");
    }

    public override void Write(Utf8JsonWriter writer, LocalizedText value, JsonSerializerOptions options)
    {
        if (value.Id != 0)
        {
            JsonSerializer.Serialize(writer, value.Id, options); // TODO: write pdata GUID
        }
        else
        {
            JsonSerializer.Serialize(writer, value.UntranslatedText, options);
        }
    }
}
