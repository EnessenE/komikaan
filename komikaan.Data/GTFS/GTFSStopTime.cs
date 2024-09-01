namespace komikaan.Data.GTFS
{
    public class GTFSStopTime
    {
        public string TripId { get; set; }
        public DateTimeOffset? ActualArrivalTime { get => ArrivalTime; }
        public DateTimeOffset? ActualDepartureTime { get => DepartureTime; }
        public DateTimeOffset? PlannedArrivalTime { get => ArrivalTime; }
        public DateTimeOffset? plannedDepartureTime { get => DepartureTime;  }

        public DateTimeOffset? ArrivalTime { get; set; }
        public DateTimeOffset? DepartureTime { get; set; }
        public string StopHeadsign { get; set; }
        public string Headsign { get; set; }
        public string Shortname { get; set; }
        public string RouteShortName { get; set; }
        public string RouteLongName { get; set; }
        public string DataOrigin { get; set; }
        public string Operator { get; set; }
        public string PlannedPlatform { get => Platform; }
        public string ActualPlatform { get=> Platform; }
        public string Platform { get; set; }
        public string RouteUrl { get; set; }
        public string RouteType { get; set; }
        public string RouteDesc { get; set; }
        public string RouteColor { get; set; }
        public string RouteTextColor { get; set; }
        public bool RealTime { get; set; }
    }
}
