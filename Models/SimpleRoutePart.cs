namespace komikthuis.Models;

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
}