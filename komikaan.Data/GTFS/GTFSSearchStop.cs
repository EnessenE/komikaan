using System.Text.Json.Serialization;
using komikaan.Data.GTFS;

namespace komikaan.Controllers
{
    public class GTFSSearchStop : GTFSStop
    {
        [JsonIgnore]
        public double[][] Coordinates { get; set; }
        public List<SimpleCoordinate> AdjustedCoordinates { get; set; } = new List<SimpleCoordinate>();
    }
}


public class SimpleCoordinate
{
    public double Longitude { get; set; }
    public double Latitude { get; set; }
}
