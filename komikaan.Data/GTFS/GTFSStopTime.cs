namespace komikaan.Data.GTFS
{
    public class GTFSStopTime
    {
        public string TripId { get; set; }
        public string? ArrivalTime { get; set; }
        public string? DepartureTime { get; set; }
        public string StopHeadsign { get; set; }
        public string Headsign { get; set; }
        public string Shortname { get; set; }
        public string RouteShortName { get; set; }
        public string RouteLongName { get; set; }
        public string DataOrigin { get; set; }
    }
}
