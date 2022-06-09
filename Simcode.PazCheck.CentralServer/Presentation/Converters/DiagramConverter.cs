using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    //public class DiagramConverter : JsonConverter<CeMatrix>
    //{
    //    public override CeMatrix Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        if (reader.TokenType != JsonTokenType.StartObject)
    //        {
    //            throw new JsonException();
    //        }
    //        var diagram = new CeMatrix();
    //        while (reader.Read())
    //        {
    //            if (reader.TokenType == JsonTokenType.EndObject)
    //            {
    //                return diagram;
    //            }
    //            if (reader.TokenType != JsonTokenType.PropertyName)
    //            {
    //                throw new JsonException();
    //            }
    //            var propName = reader.GetString();
    //            reader.Read();
    //            switch (propName)
    //            {
    //                case "Name": diagram.Title = reader.GetString();
    //                    break;
    //                case "Causes":
    //                    diagram.CauseKeys = JsonSerializer.Deserialize<List<int>>(ref reader, options);
    //                    break;
    //                case "Effects":
    //                    diagram.EffectKeys = JsonSerializer.Deserialize<List<int>>(ref reader, options);
    //                    break;
    //                case "Intersections":
    //                    diagram.Intersections = JsonSerializer.Deserialize<List<Intersection>>(ref reader, options);
    //                    break;
    //            }
    //        }
    //        throw new JsonException();
    //    }

    //    public override void Write(Utf8JsonWriter writer, CeMatrix diagram, JsonSerializerOptions options)
    //    {
    //        writer.WriteStartObject();
    //        writer.WriteString("Name", diagram.Title);
    //        diagram.MapKeys();
    //        writer.WritePropertyName("Causes");
    //        JsonSerializer.Serialize(writer, diagram.CauseKeys, options);
    //        writer.WritePropertyName("Effects");
    //        JsonSerializer.Serialize(writer, diagram.EffectKeys, options);
    //        writer.WritePropertyName("Intersections");
    //        JsonSerializer.Serialize(writer, diagram.Intersections, options);
    //        writer.WriteEndObject();
    //    }
    //}
}
