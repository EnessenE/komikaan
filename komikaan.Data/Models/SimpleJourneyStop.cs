using komikaan.Data.Enums;

namespace komikaan.Data.Models
{
    public class SimpleJourneyStop
    {
        public string Name { get; set; }

        public DateTime? PlannedArrival { get; set; }
        public DateTime PlannedDeparture { get; set; }
        public DateTime? ActualDeparture { get; set; }
        public DateTime? ActualArrival { get; set; }

        public string? PlannedArrivalTrack { get; set; }
        public string? PlannedDepartureTrack { get; set; }
        public string? ActualArrivalTrack { get; set; }
        public string? ActualDepartureTrack { get; set; }
    }
}
