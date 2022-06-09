using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    //public class EffectsListConverter : JsonConverter<List<Effect>>
    //{
    //    public List<Tag> Tags { get; set; }

    //    public override List<Effect> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        if (reader.TokenType != JsonTokenType.StartArray || !reader.Read())
    //        {
    //            throw new JsonException();
    //        }
    //        var effects = new List<Effect>();
    //        while (reader.TokenType != JsonTokenType.EndArray)
    //        {
    //            var effect = JsonSerializer.Deserialize<Effect>(ref reader, options);
    //            var identity = Tags
    //                .SelectMany(t=>t.TagConditions.Select(i => i))
    //                .FirstOrDefault(i => i.Key==effect.IdentityKey);
    //            if (identity!=null)
    //            {
    //                effect.Identity = identity;
    //            }
    //            effects.Add(effect);
    //            if (!reader.Read())
    //            {
    //                throw new JsonException();
    //            }
    //        }
    //        return effects;
    //    }

    //    public override void Write(Utf8JsonWriter writer, List<Effect> effects, JsonSerializerOptions options)
    //    {
    //        writer.WriteStartArray();
    //        foreach (var effect in effects)
    //        {
    //            JsonSerializer.Serialize(writer, effect, options);
    //        }
    //        writer.WriteEndArray();
    //    }
    //}
}
