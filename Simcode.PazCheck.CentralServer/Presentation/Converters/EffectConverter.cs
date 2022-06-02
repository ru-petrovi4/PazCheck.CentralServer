using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    public class EffectConverter : JsonConverter<Effect>
    {
        public override Effect Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }
            var effect = new Effect();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return effect;
                }
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }
                var propName = reader.GetString();
                reader.Read();
                switch (propName)
                {
                    case "Name": effect.Name = reader.GetString();
                        break;
                    case "IdentityKey": effect.IdentityKey = reader.GetInt32();
                        break;
                    case "Key": effect.Key = reader.GetInt32();
                        break;
                }
            }
            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Effect effect, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Name", effect.Name);
            writer.WriteNumber("IdentityKey", effect.Identity.Id);
            writer.WriteNumber("Key", effect.Id);
            writer.WriteEndObject();
        }
    }
}
