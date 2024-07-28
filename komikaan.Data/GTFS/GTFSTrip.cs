using GTFS.Entities;

namespace komikaan.Data.GTFS
{
    public class GTFSTrip
    {
        public string Id { get; set; }
        public string? RouteId { get; set; }
        public string? ServiceId { get; set; }
        public string? Headsign { get; set; }
        public string? Shortname { get; set; }
        public int? Direction { get; set; }
        public string? BlockId { get; set; }
        public string? DataOrigin { get; set; }
        public IEnumerable<GTFSTripStop>? Stops { get; set; }

        public IEnumerable<Shape>? Shapes { get; set; }
    }
}
