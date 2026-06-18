using Newtonsoft.Json;

namespace WebColegio.Helpers
{
    /// <summary>Convierte null de la API en false al deserializar bool no nullable.</summary>
    public sealed class NullToFalseBoolConverter : JsonConverter<bool>
    {
        public override bool ReadJson(JsonReader reader, Type objectType, bool existingValue, bool hasExisting, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null || reader.Value == null)
                return false;

            return Convert.ToBoolean(reader.Value);
        }

        public override void WriteJson(JsonWriter writer, bool value, JsonSerializer serializer)
        {
            writer.WriteValue(value);
        }
    }
}
