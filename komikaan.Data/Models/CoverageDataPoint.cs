using komikaan.GTFS.Models.Static.Enums;

namespace komikaan.Data.Models;

public class CoverageDataPoint
{
    public string DataOrigin { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public int ClusterId { get; set; }
    public ExtendedRouteType StopType { get; set; }
}
