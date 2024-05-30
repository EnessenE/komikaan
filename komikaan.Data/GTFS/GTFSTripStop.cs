
namespace komikaan.Data.GTFS
{
    public class GTFSTripStop
    {
        public string Id { get; set; }
        public int Sequence { get; set; }
        public string Name { get; set; }
        public TimeOnly? Arrival { get; set; }
        public TimeOnly? Departure { get; set; }
        public string PlatformCode { get; set; }
        public string StopHeadsign { get; set; }
        public TimeOnly? ActualArrivalTime { get => Arrival; }
        public TimeOnly? ActualDepartureTime { get => Departure; }
        public TimeOnly? PlannedArrivalTime { get => Arrival; }
        public TimeOnly? plannedDepartureTime { get => Departure; }
        public string PlannedPlatform { get => PlatformCode; }
        public string ActualPlatform { get => PlatformCode; }
    }
}
