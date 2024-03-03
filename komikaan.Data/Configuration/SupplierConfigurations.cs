using System.Text.Json.Serialization;
using komikaan.Data.Enums;

namespace komikaan.Data.Configuration
{
    public class SupplierConfigurations
    {
        public required IDictionary<DataSource, Dictionary<string, LegType>> Mappings { get; set; }
    }
}
