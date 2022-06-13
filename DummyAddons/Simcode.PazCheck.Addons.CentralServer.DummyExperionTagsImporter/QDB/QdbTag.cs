using CsvHelper.Configuration.Attributes;

namespace Simcode.PazCheck.Addons.CentralServer.DummyExperionTagsImporter
{
    public class QdbTag : ITagRecord
    {
        [Name("ItemName")]
        public string Name { get; set; } = @"";
        [Name("ItemDescription")]
        public string Descr { get; set; } = @"";
    }
}
