using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.Common;
using System.IO;

namespace Simcode.PazCheck.CentralServer.Common
{
    public abstract class TagsImporterAddonBase : AddonBase
    {
        public abstract void ImportTags(Stream stream, PazCheckDbContext context, Project project, string user);
    }
}
