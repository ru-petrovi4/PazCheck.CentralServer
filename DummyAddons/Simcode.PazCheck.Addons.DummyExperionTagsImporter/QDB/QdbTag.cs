using CsvHelper.Configuration.Attributes;

namespace Simcode.PazCheck.Addons.DummyExperionTagsImporter
{
    public class QdbTag : ITagRecord
    {
        [Name("ItemName")]
        public string Name { get; set; } = @"";
        [Name("ItemDescription")]
        public string Descr { get; set; } = @"";
    }
}
