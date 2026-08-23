using komikaan.Data.GTFS;

namespace komikaan.Data.API
{
    public class MapViewportData
    {
        public IEnumerable<MapShapePoint> Shapes { get; set; } = Enumerable.Empty<MapShapePoint>();
        public IEnumerable<GTFSSearchStop> Stops { get; set; } = Enumerable.Empty<GTFSSearchStop>();
    }

    public class MapShapePoint
    {
        public string Id { get; set; } = string.Empty;
        public string DataOrigin { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public long Sequence { get; set; }
    }
}