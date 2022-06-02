using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    public class IdentityConverter : JsonConverter<Identity>
    {
        public override Identity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }
            var identity = new Identity();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return identity;
                }
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }
                var propName = reader.GetString();
                reader.Read();
                switch (propName)
                {
                    case "Identifier": identity.Identifier = reader.GetString();
                        break;
                    case "EventType": identity.EventType = reader.GetString();
                        break;
                    case "Value": identity.Value = reader.GetString();
                        break;
                    case "Key": identity.Key = reader.GetInt32();
                        break;
                }
            }
            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Identity identity, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Identifier", identity.Identifier);
            writer.WriteString("EventType", identity.EventType);
            writer.WriteString("Value", identity.Value);
            writer.WriteNumber("Key", identity.Id);
            writer.WriteEndObject();
        }
    }
}
