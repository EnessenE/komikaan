using komikaan.Data.Enums;

namespace komikaan.Models;

/// <summary>
/// Represents a part of a journey, or essentially a journey leg
/// Goes from Stop to Stop
/// Always has to have a connection to the other stops for a journey to make sense
/// </summary>
public class SimpleRoutePart
{
    public string DepartureStation { get; set; }
    public string ArrivalStation { get; set; }
    public bool Cancelled { get; set; }
    public bool RealisticTransfer { get; set; }
    public bool AlternativeTransport { get; set; }
    public LegType Type { get; set; }

    public DateTime? PlannedArrival { get; set; }
    public DateTime PlannedDeparture { get; set; }
    public DateTime? ActualDeparture { get; set; }
    public DateTime? ActualArrival { get; set; }

    public string PlannedArrivalTrack { get; set; }
    public string PlannedDepartureTrack { get; set; }
    public string? ActualArrivalTrack { get; set; }
    public string? ActualDepartureTrack { get; set; }
}
