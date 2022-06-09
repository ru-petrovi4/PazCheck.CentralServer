using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.Common;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    //public class ProjectConverter : JsonConverter<Project>
    //{
    //    private readonly EffectsListConverter _effectListConverter;
    //    private readonly Type _effectListType;
    //    private readonly CauseListConverter _causeListConverter;
    //    private readonly Type _causeListType;
    //    private readonly IJobProgress _jobProgress;
    //    private readonly CancellationToken _cancellationToken;

    //    public ProjectConverter(JsonSerializerOptions options, CancellationToken cancellationToken, IJobProgress jobProgress)
    //    {
    //        _jobProgress = jobProgress;
    //        _cancellationToken = cancellationToken;
    //        _effectListConverter = (EffectsListConverter) options
    //            .GetConverter(typeof(List<Effect>));
    //        _effectListType = typeof(List<Effect>);
    //        _causeListConverter = (CauseListConverter) options
    //            .GetConverter(typeof(List<Cause>));
    //        _causeListType = typeof(List<Cause>);
    //    }

    //    public override Project Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    //    {
    //        var progress = 0;
    //        if (reader.TokenType != JsonTokenType.StartObject)
    //        {
    //            throw new JsonException();
    //        }

    //        var project = new Project();
    //        while (reader.Read() && !_cancellationToken.IsCancellationRequested)
    //        {
    //            if (reader.TokenType == JsonTokenType.EndObject)
    //            {
    //                return project;
    //            }

    //            if (reader.TokenType != JsonTokenType.PropertyName)
    //            {
    //                throw new JsonException();
    //            }

    //            var propName = reader.GetString();
    //            reader.Read();
    //            switch (propName)
    //            {
    //                case "Name":
    //                    project.Title = reader.GetString();
    //                    progress++;
    //                    break;
    //                case "Descr":
    //                    project.Desc = reader.GetString();
    //                    progress++;
    //                    break;
    //                case "Tags":
    //                    project.Tags = JsonSerializer.Deserialize<List<Tag>>(ref reader, options);
    //                    progress += 30;
    //                    break;
    //                case "Effects":
    //                    _effectListConverter.Tags = project.Tags;
    //                    project.Effects = _effectListConverter.Read(ref reader, _effectListType, options);
    //                    progress += 19;
    //                    break;
    //                case "Causes":
    //                    _causeListConverter.Tags = project.Tags;
    //                    project.Causes = _causeListConverter.Read(ref reader, _causeListType, options);
    //                    progress += 19;
    //                    break;
    //                case "Diagrams":
    //                    project.CeMatrices = JsonSerializer.Deserialize<List<CeMatrix>>(ref reader, options);
    //                    foreach (var diagram in project.CeMatrices)
    //                    {
    //                        foreach (var cause in diagram.CauseKeys.Select(causeKey =>
    //                            project.Causes.FirstOrDefault(c =>
    //                                c.Key == causeKey)).Where(c => c != null))
    //                        {                                
    //                            diagram.Causes.Add(cause!);
    //                        }
    //                        foreach (var effect in diagram.EffectKeys.Select(effectKey =>
    //                            project.Effects.FirstOrDefault(e =>
    //                                e.Key == effectKey)).Where(e => e != null))
    //                        {
    //                            diagram.Effects.Add(effect!);
    //                        }
    //                        foreach (var intersection in diagram.Intersections)
    //                        {
    //                            var cause = project.Causes.FirstOrDefault(c => c.Key == intersection.CauseKey);
    //                            var effect = project.Effects.FirstOrDefault(e => e.Key == intersection.EffectKey);
    //                            if (cause != null && effect != null)
    //                            {
    //                                intersection.Cause = cause;
    //                                intersection.Effect = effect;
    //                            }                                
    //                        }
    //                    }
    //                    progress += 30;
    //                    break;
    //            }
    //            _jobProgress.ReportAsync(progress, null, null, false);
    //        }

    //        throw new JsonException();
    //    }

    //    public override void Write(Utf8JsonWriter writer, Project project, JsonSerializerOptions options)
    //    {
    //        writer.WriteStartObject();
    //        writer.WriteString("Name", project.Title);
    //        writer.WriteString("Descr", project.Desc);
    //        writer.WritePropertyName("Tags");
    //        JsonSerializer.Serialize(writer, project.Tags, options);
    //        writer.WritePropertyName("Effects");
    //        JsonSerializer.Serialize(writer, project.Effects, options);
    //        writer.WritePropertyName("Causes");
    //        JsonSerializer.Serialize(writer, project.Causes, options);
    //        writer.WritePropertyName("Diagrams");
    //        JsonSerializer.Serialize(writer, project.CeMatrices, options);
    //        writer.WriteEndObject();
    //    }
    //}
}
