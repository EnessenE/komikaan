using komikaan.Data.Enums;

namespace komikaan.Data.Models
{
    public class SimpleStop
    {
        public string Name { get; set; }
        public Dictionary<DataSource, List<string>> Ids { get; set; } = new Dictionary<DataSource, List<string>>();
    }
}
