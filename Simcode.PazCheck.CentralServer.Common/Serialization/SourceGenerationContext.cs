using System.Text.Json.Serialization;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{
    [JsonSourceGenerationOptions(
        WriteIndented = true, 
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        IgnoreReadOnlyProperties = true)]
    [JsonSerializable(typeof(SerializationRootObject))]
    [JsonSerializable(typeof(LicenseFileInfo))]    
    public partial class SourceGenerationContext : JsonSerializerContext
    {
    }
}
