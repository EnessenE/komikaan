using komikaan.Data.Enums;
using komikaan.Data.GTFS;

namespace komikaan.Data.Models
{
    public class SimpleStop
    {
        public string Name { get; set; }
        public StopType StopType { get; set; }
        public List<string> Ids { get; set; } = new List<string>();
    }
}
