using GTFS.Entities.Enumerations;

namespace komikaan.Data.GTFS
{
    public class GTFSRoute
    {
        public string ShortName { get; set; }
        public string LongName { get; set; }
        public string DataOrigin { get; set; }
        public string Agency { get; set; }
        public string Description { get; set; }
        public StopType Type { get; set; }
        public string Url { get; set; }
        public string Color { get; set; }
        public string TextColor { get; set; }
        public DateTime LastUpdated { get; set; }
    }


    /// <summary>
    /// Yeah I know
    /// </summary>
    public class GTFSDatabaseRoute : GTFSRoute
    {
        public new RouteTypeExtended Type { get; set; }
    }
}
