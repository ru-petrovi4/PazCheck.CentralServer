using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Содержимое файла
    /// </summary>
    /// <remarks>
    ///     Должно соответствовать единственному DbFile
    /// </remarks>
    [Resource]
    public class DbFileContent : Identifiable<int>
    {
        /// <summary>
        ///     Содержимое файла в формате Base64
        /// </summary>     
        [Attr]
        public string FileBytes_Base64 { get; set; } = @"";
    }
}
