using System;
using Newtonsoft.Json;

namespace WebColegio.Helpers
{
    /// <summary>Deserializa contraseñas hash (byte[]) que la API envía como Base64 en JSON.</summary>
    public sealed class Base64ByteArrayConverter : JsonConverter<byte[]>
    {
        public override byte[]? ReadJson(JsonReader reader, Type objectType, byte[]? existingValue, bool hasExisting, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.String)
            {
                var s = reader.Value?.ToString();
                if (string.IsNullOrEmpty(s))
                    return null;
                try
                {
                    return Convert.FromBase64String(s);
                }
                catch (FormatException)
                {
                    return System.Text.Encoding.UTF8.GetBytes(s);
                }
            }

            if (reader.TokenType == JsonToken.StartArray)
            {
                var list = new System.Collections.Generic.List<byte>();
                while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                {
                    if (reader.TokenType == JsonToken.Integer && reader.Value != null)
                        list.Add(Convert.ToByte(reader.Value));
                }
                return list.ToArray();
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, byte[]? value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();
            else
                writer.WriteValue(Convert.ToBase64String(value));
        }
    }
}
