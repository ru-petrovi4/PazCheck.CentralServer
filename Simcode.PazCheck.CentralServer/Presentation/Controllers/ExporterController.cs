using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using Ssz.Utils.Addons;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    //[Route(@"Exporter")]
    //public class ExporterController : ControllerBase
    //{
    //    #region construction and destruction

    //    public ExporterController(PazCheckDbContext context,
    //        IHostApplicationLifetime applicationLifetime, AddonsManager addonsManager)
    //    {
    //        _context = context;
    //        _cancellationToken = applicationLifetime.ApplicationStopping;
    //        _addonsManager = addonsManager;
    //    }

    //    #endregion

    //    #region public functions

    //    [HttpGet("diagram/{diagId}")]
    //    public async Task<IActionResult> ExportDiagram(int diagId)
    //    {
    //        CeMatrix diagram = await _context.CeMatrices
    //            .FirstAsync(d => d.Id == diagId, cancellationToken: _cancellationToken);

    //        var ceMatrixRuntimeAddon = _addonsManager.Addons.OfType<CeMatrixRuntimeAddonBase>().OrderBy(a => a.IsDummy).FirstOrDefault();
    //        if (ceMatrixRuntimeAddon is not null)
    //        {
    //            string? s = ceMatrixRuntimeAddon.GetCeMatrixString(_context, diagram);
    //            if (!string.IsNullOrEmpty(s))
    //            {
    //                try
    //                {
    //                    var options = new JsonSerializerOptions()
    //                    {
    //                        WriteIndented = false
    //                    };
    //                    string serialized = JsonSerializer.Serialize(s, options);
    //                    return Ok(serialized);
    //                }
    //                catch
    //                {
    //                }
    //            }
    //        }

    //        return NoContent();
    //    }

    //    [HttpGet("diagramresult/{diagId}")]
    //    public async Task<IActionResult> ExportDiagramResult(int diagId)
    //    {
    //        CeMatrixResult diagResult = await _context.CeMatrixResuls
    //            .FirstAsync(d => d.Id == diagId, cancellationToken: _cancellationToken);

    //        var ceMatrixRuntimeAddon = _addonsManager.Addons.OfType<CeMatrixRuntimeAddonBase>().OrderBy(a => a.IsDummy).FirstOrDefault();
    //        if (ceMatrixRuntimeAddon is not null)
    //        {
    //            string? s = ceMatrixRuntimeAddon.GetCeMatrixRuntimeString(_context, diagResult);
    //            if (!string.IsNullOrEmpty(s))
    //            {
    //                return Ok(s);
    //            }
    //        }

    //        return NoContent();
    //    }

    //    #endregion        

    //    #region private fields

    //    private readonly AddonsManager _addonsManager;

    //    private readonly PazCheckDbContext _context;
    //    private readonly CancellationToken _cancellationToken;

    //    #endregion        
    //}
}


//public IActionResult Export()
//{
//    var ret = new {data = $"This is an empty test data for save in file"};
//    return Ok(ret);
//}

//[HttpGet("project/{prjId}")]
//public async Task<IActionResult> ExportProject(int prjId)
//{
//    var project = await _context.Projects
//        .Include(prj => prj.Tags)
//        .ThenInclude(tag => tag.TagConditions)
//        .Include(prj => prj.Effects)
//        .Include(prj => prj.Causes)
//        .ThenInclude(c => c.Identities)
//        .Include(prj => prj.Diagrams)
//        .ThenInclude(d => d.Causes)                
//        .Include(prj => prj.Diagrams)
//        .ThenInclude(d => d.Effects)                
//        .Include(prj => prj.Diagrams)
//        .ThenInclude(d => d.Intersections)                
//        .FirstAsync(prj => prj.Id==prjId, cancellationToken: _cancellationToken);
//    var ret = _unitHelper.ExportUnit(project, _cancellationToken);
//    return Ok(ret);
//}
