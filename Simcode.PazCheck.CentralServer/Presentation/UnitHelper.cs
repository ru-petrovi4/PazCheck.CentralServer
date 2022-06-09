using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Presentation;
using Simcode.PazCheck.Common;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    //public class UnitHelper : IUnitHelper
    //{
    //    private readonly PazCheckDbContext _context;

    //    public UnitHelper(PazCheckDbContext context)
    //    {
    //        _context = context;
    //    }

    //    public string ExportUnit(Project project, CancellationToken cancellationToken)
    //    {
    //        var jsonString = "";
    //        if (!cancellationToken.IsCancellationRequested)
    //        {
    //            var options = new JsonSerializerOptions()
    //            {
    //                WriteIndented = false
    //            };
    //            options.Converters.Add(new TagConverter());
    //            options.Converters.Add(new IdentityConverter());
    //            options.Converters.Add(new EffectsListConverter());
    //            options.Converters.Add(new EffectConverter());
    //            options.Converters.Add(new CauseListConverter());
    //            //options.Converters.Add(new CauseConverter());
    //            options.Converters.Add(new IntersectionConverter());
    //            options.Converters.Add(new DiagramConverter());
    //            options.Converters.Add(new ProjectConverter(options, cancellationToken, null));
    //            jsonString = JsonSerializer.Serialize(project, options);
    //        }
    //        return jsonString;
    //    }

    //    public async Task ImportUnitAsync(string filePath, CancellationToken cancellationToken, IJobProgress jobProgress)
    //    {
    //        if (!cancellationToken.IsCancellationRequested)
    //        {
    //            await using var stream = File.OpenRead(filePath);
    //            var options = new JsonSerializerOptions();
    //            options.Converters.Add(new TagConverter());
    //            options.Converters.Add(new IdentityConverter());
    //            options.Converters.Add(new EffectsListConverter());
    //            options.Converters.Add(new EffectConverter());
    //            options.Converters.Add(new CauseListConverter());
    //            //options.Converters.Add(new CauseConverter());
    //            options.Converters.Add(new IntersectionConverter());
    //            options.Converters.Add(new DiagramConverter());
    //            options.Converters.Add(new ProjectConverter(options, cancellationToken, jobProgress));
    //            var project = await JsonSerializer.DeserializeAsync<Project>(stream, options, cancellationToken);
    //            await _context.Projects.AddAsync(project!, cancellationToken);
    //            await _context.SaveChangesAsync(cancellationToken);
    //            stream.Close();
    //            File.Delete(filePath);
    //        }
    //    }
    //}
}
