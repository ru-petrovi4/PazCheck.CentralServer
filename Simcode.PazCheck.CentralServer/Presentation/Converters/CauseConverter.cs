using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    public class CauseConverter : JsonConverter<Cause>
    {
        public override Cause Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }
            var cause = new Cause();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return cause;
                }
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }
                var propName = reader.GetString();
                reader.Read();
                switch (propName)
                {
                    case "Name": cause.Name = reader.GetString();
                        break;
                    case "Limit": cause.Limit = reader.GetInt32();
                        break;
                    case "Complex":
                        cause.Complex = reader.GetBoolean();
                        break;
                    case "Identities":
                        cause.IdentitiesKeys = JsonSerializer.Deserialize<List<int>>(ref reader, options);
                        break;
                    case "Key": cause.Key = reader.GetInt32();
                        break;
                }
            }
            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Cause cause, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Name", cause.Name);
            writer.WriteNumber("Limit", cause.Limit);
            writer.WriteBoolean("Complex", cause.Complex);
            cause.MapIdetitiesKeys();
            writer.WritePropertyName("Identities");
            JsonSerializer.Serialize(writer, cause.IdentitiesKeys, options);
            writer.WriteNumber("Key", cause.Id);
            writer.WriteEndObject();
        }
    }
}
