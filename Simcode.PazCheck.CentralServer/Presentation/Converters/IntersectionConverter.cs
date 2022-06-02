using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    public class IntersectionConverter : JsonConverter<Intersection>
    {
        public override Intersection Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }
            var intersection = new Intersection();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return intersection;
                }
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }
                var propName = reader.GetString();
                reader.Read();
                switch (propName)
                {
                    case "CauseKey":
                        intersection.CauseKey = reader.GetInt32();
                        break;
                    case "EffectKey":
                        intersection.EffectKey = reader.GetInt32();
                        break;
                }
            }
            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Intersection intersection, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("CauseKey", intersection.Cause.Id);
            writer.WriteNumber("EffectKey", intersection.Effect.Id);
            writer.WriteEndObject();
        }
    }
}
