namespace komikaan.Models;

public class SimpleRoutePart
{
    public string Name { get; set; }
    public bool Cancelled { get; set; }
    public bool RealisticTransfer { get; set; }
    public bool AlternativeTransport { get; set; }
    public DateTime? PlannedArrival { get; set; }
    public DateTime PlannedDeparture { get; set; }
    public DateTime? ActualDeparture { get; set; }
    public DateTime? ActualArrival { get; set; }

    public string PlannedArrivalTrack { get; set; }
    public string PlannedDepartureTrack { get; set; }
    public string? ActualArrivalTrack { get; set; }
    public string? ActualDepartureTrack { get; set; }
}