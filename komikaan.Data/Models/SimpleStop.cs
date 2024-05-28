using komikaan.Data.Enums;

namespace komikaan.Data.Models
{
    public class SimpleStop
    {
        public string Name { get; set; }
        public List<string> Ids { get; set; } = new List<string>();
    }
}
