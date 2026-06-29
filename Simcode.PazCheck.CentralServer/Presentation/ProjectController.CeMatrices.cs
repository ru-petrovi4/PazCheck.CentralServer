using IdentityServer4;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.MicroServices;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Simcode.PazCheck.CentralServer.Common.Serialization;
using Ssz.Utils;
using Ssz.Utils.Addons;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Simcode.PazCheck.CentralServer.Presentation
{
    public partial class ProjectController : ControllerBase
    {
        #region public functions

        /// <summary>
        ///     Возвращается информация об объединении ячеек.
        ///     Если projectVersionNum не задан или 0, то возвращается результат для текущих несохраненных изменений.
        /// </summary>
        /// <param name="ceMatrixId"></param>
        /// <param name="projectVersionNum"></param>
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"GetCellInfos/{ceMatrixId}")]
        public async Task<IActionResult> GetCellInfosAsync(int ceMatrixId, UInt32? projectVersionNum)
        {
            var informationSecurityContext = InformationSecurityContext.CreateFrom(HttpContext);

            try
            {
                if (projectVersionNum == 0)
                    projectVersionNum = null;                

                var loggersSet = new LoggersSet(_logger, null);                

                CeMatrixRuntimeAddonBase? ceMatrixRuntimeAddon = _addonsManager.CreateInitializedAddonThreadSafe<CeMatrixRuntimeAddonBase>(null, CancellationToken.None);
                if (ceMatrixRuntimeAddon is null)
                {
                    _logger.LogError("Cannot find addon of type: {0}", @"CeMatrixRuntimeAddonBase");
                    return NoContent();
                }
                
                return Ok(await ceMatrixRuntimeAddon.GetCellInfosAsync(_dbContextFactory, ceMatrixId, projectVersionNum, informationSecurityContext.User, _cache.DbCache, loggersSet));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"GetCellInfosAsync Error");

                return NotFound();
            }
        }

        /// <summary>
        ///     Возвращается информация об объединении ячеек.        
        /// </summary>
        /// <param name="ceMatrixResultId"></param>        
        /// <returns></returns>
        [DefaultMethod_RoleBusinessFunctions(
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
            )]
        [HttpGet(@"GetResultCellInfos/{ceMatrixResultId}")]
        public async Task<IActionResult> GetResultCellInfosAsync(int ceMatrixResultId)
        {
            var loggersSet = new LoggersSet(_logger, null);            

            try
            {
                CeMatrixRuntimeAddonBase? ceMatrixRuntimeAddon = _addonsManager.CreateInitializedAddonThreadSafe<CeMatrixRuntimeAddonBase>(null, CancellationToken.None);
                if (ceMatrixRuntimeAddon is null)
                {
                    _logger.LogError("Cannot find addon of type: {0}", @"CeMatrixRuntimeAddonBase");
                    return NoContent();
                }

                return Ok(await ceMatrixRuntimeAddon.GetResultCellInfosAsync(_dbContextFactory, _cache.DbCache, ceMatrixResultId, loggersSet));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"GetCellInfosAsync Error");

                return NotFound();
            }
        }

        #endregion        
    }
}