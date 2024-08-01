namespace komikaan.Data.GTFS
{
    public class GTFSTripStop
    {
        public string Id { get; set; }
        public int Sequence { get; set; }
        public string Name { get; set; }
        public DateTimeOffset? Arrival { get; set; }
        public DateTimeOffset? Departure { get; set; }
        public string PlatformCode { get; set; }
        public string StopHeadsign { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public StopType StopType { get; set; }
        public int PickupType { get; set; }
        public int DropOffType { get; set; }
        public DateTimeOffset? ActualArrivalTime { get => Arrival; }
        public DateTimeOffset? ActualDepartureTime { get => Departure; }
        public DateTimeOffset? PlannedArrivalTime { get => Arrival; }
        public DateTimeOffset? PlannedDepartureTime { get => Departure; }
        public string PlannedPlatform { get => PlatformCode; }
        public string ActualPlatform { get => PlatformCode; }
    }
}
