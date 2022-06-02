using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    public class TagConverter : JsonConverter<Tag>
    {
        public override Tag Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }
            var tag = new Tag();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return tag;
                }
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }
                var propName = reader.GetString();
                reader.Read();
                switch (propName)
                {
                    case "Name": tag.Name = reader.GetString();
                        break;
                    case "Descr": tag.Descr = reader.GetString();
                        break;
                    case "Identities":
                        tag.Identities = JsonSerializer.Deserialize<List<Identity>>(ref reader, options);
                        break;
                    case "Key": tag.Key = reader.GetInt32();
                        break;
                }
            }
            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Tag tag, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Name", tag.Name);
            writer.WriteString("Descr", tag.Descr);
            writer.WritePropertyName("Identities");
            JsonSerializer.Serialize(writer, tag.Identities, options);
            writer.WriteNumber("Key", tag.Id);
            writer.WriteEndObject();
        }
    }
}
