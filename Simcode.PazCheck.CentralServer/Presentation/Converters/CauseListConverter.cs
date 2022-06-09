using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    //public class CauseListConverter : JsonConverter<List<Cause>>
    //{
    //    public List<Tag> Tags { get; set; }

    //    public override List<Cause> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        if (reader.TokenType != JsonTokenType.StartArray || !reader.Read())
    //        {
    //            throw new JsonException();
    //        }
    //        var causes = new List<Cause>();
    //        while (reader.TokenType != JsonTokenType.EndArray)
    //        {
    //            var cause = JsonSerializer.Deserialize<Cause>(ref reader, options);
    //            foreach (var identityKey in cause.IdentitiesKeys)
    //            {
    //                var identity = Tags
    //                    .SelectMany(t=>t.TagConditions.Select(i => i))
    //                    .FirstOrDefault(i => i.Key==identityKey);
    //                if (identity!=null)
    //                {
    //                    cause.Identities.Add(identity);
    //                }
    //            }
    //            causes.Add(cause);
    //            if (!reader.Read())
    //            {
    //                throw new JsonException();
    //            }
    //        }
    //        return causes;
    //    }

    //    public override void Write(Utf8JsonWriter writer, List<Cause> causes, JsonSerializerOptions options)
    //    {
    //        writer.WriteStartArray();
    //        foreach (var cause in causes)
    //        {
    //            JsonSerializer.Serialize(writer, cause, options);
    //        }
    //        writer.WriteEndArray();
    //    }
    //}
}
