using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    //public class IdentityConverter : JsonConverter<TagCondition>
    //{
    //    public override TagCondition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        if (reader.TokenType != JsonTokenType.StartObject)
    //        {
    //            throw new JsonException();
    //        }
    //        var identity = new TagCondition();
    //        while (reader.Read())
    //        {
    //            if (reader.TokenType == JsonTokenType.EndObject)
    //            {
    //                return identity;
    //            }
    //            if (reader.TokenType != JsonTokenType.PropertyName)
    //            {
    //                throw new JsonException();
    //            }
    //            var propName = reader.GetString();
    //            reader.Read();
    //            switch (propName)
    //            {
    //                case "Identifier": identity.ElementName = reader.GetString();
    //                    break;
    //                case "EventType": identity.Type = reader.GetString();
    //                    break;
    //                case "Value": identity.Value = reader.GetString();
    //                    break;
    //                case "Key": identity.Key = reader.GetInt32();
    //                    break;
    //            }
    //        }
    //        throw new JsonException();
    //    }

    //    public override void Write(Utf8JsonWriter writer, TagCondition identity, JsonSerializerOptions options)
    //    {
    //        writer.WriteStartObject();
    //        writer.WriteString("Identifier", identity.ElementName);
    //        writer.WriteString("EventType", identity.Type);
    //        writer.WriteString("Value", identity.Value);
    //        writer.WriteNumber("Key", identity.Id);
    //        writer.WriteEndObject();
    //    }
    //}
}
