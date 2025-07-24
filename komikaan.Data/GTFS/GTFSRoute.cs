using komikaan.GTFS.Models.Static.Enums;

namespace komikaan.Data.GTFS
{
    public class GTFSRoute
    {
        public string ShortName { get; set; }
        public string LongName { get; set; }
        public string DataOrigin { get; set; }
        public string Agency { get; set; }
        public string Description { get; set; }
        public RouteType Type { get; set; }
        public string Url { get; set; }
        public string Color { get; set; }
        public string TextColor { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
