using komikaan.Data.Enums;

namespace komikaan.Controllers;

public class TransportStatistics
{
    public Dictionary<DataSource, int> Disruptions { get; } = new Dictionary<DataSource, int>();
}