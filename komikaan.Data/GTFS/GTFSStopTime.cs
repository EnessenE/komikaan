namespace komikaan.Data.GTFS
{
    public class GTFSStopTime
    {
        public string TripId { get; set; }
        public DateTimeOffset? ActualArrivalTime { get; set; }
        public DateTimeOffset? ActualDepartureTime { get; set; }
        public DateTimeOffset? PlannedArrivalTime { get; set; }
        public DateTimeOffset? PlannedDepartureTime { get; set; }

        public DateTimeOffset? ArrivalTime { get; set; }
        public DateTimeOffset? DepartureTime { get; set; }
        public string StopHeadsign { get; set; }
        public string Headsign { get; set; }
        public string ScheduleRelationship { get; set; }
        public string Shortname { get; set; }
        public string RouteShortName { get; set; }
        public string RouteLongName { get; set; }
        public string DataOrigin { get; set; }
        public string Operator { get; set; }
        public string PlannedPlatform { get; set; }
        public string ActualPlatform { get; set; }
        public string RouteUrl { get; set; }
        public string RouteType { get; set; }
        public string RouteDesc { get; set; }
        public string RouteColor { get; set; }
        public string RouteTextColor { get; set; }
        public bool? RealTime { get; set; }
    }
}
