using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Файл со своими данными
    /// </summary>
    [Resource]
    public class DbFile : Identifiable<int>
    {
        [Attr]
        public DateTime FileLastWriteTimeUtc { get; set; }

        [Attr]
        public DateTime? FileDeletionTimeUtc { get; set; }        

        [Attr]
        public string FileName { get; set; } = @"";

        [Attr]
        public byte[] Data { get; set; } = null!;
    }
}
