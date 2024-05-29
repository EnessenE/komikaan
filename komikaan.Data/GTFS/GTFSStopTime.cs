namespace komikaan.Data.GTFS
{
    public class GTFSStopTime
    {
        public string TripId { get; set; }
        public TimeOnly? ActualArrivalTime { get => ArrivalTime; }
        public TimeOnly? ActualDepartureTime { get => DepartureTime; }
        public TimeOnly? PlannedArrivalTime { get => ArrivalTime; }
        public TimeOnly? plannedDepartureTime { get => DepartureTime;  }

        public TimeOnly? ArrivalTime { get; set; }
        public TimeOnly? DepartureTime { get; set; }
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
    }
}
