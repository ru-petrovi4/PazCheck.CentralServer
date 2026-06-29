using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    public class FileContentType
    {
        public string Id { get; init; } = @"";

        public string Desc { get; init; } = @"";

        public static readonly FileContentType Unknown = new() { Id = @"Unknown", Desc = Properties.Resources.Unknown_ContentType };
    }
}
