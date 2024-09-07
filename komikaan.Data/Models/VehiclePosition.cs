namespace komikaan.Data.Models
{
    public class VehiclePosition
    {
        public DateTimeOffset LastUpdated { get; set; }
        public string Id { get; set; }
        public string TripId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string StopId { get; set; }
        public string CurrentStatus { get; set; }
        public DateTimeOffset MeasurementTime { get; set; }
        public string CongestionLevel { get; set; }
        public string OccupancyStatus { get; set; }
        public int? OccupancyPercentage { get; set; } // Nullable integer to match possible nulls
    }
}
