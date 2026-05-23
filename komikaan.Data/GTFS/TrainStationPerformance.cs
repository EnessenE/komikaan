namespace komikaan.Data.GTFS
{
    public class TrainStationPerformance
    {
        public Guid PrimaryStopId { get; set; }
        public string StopId { get; set; }
        public string StopName { get; set; }
        public int StopType { get; set; }
        public string DataOrigin { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public decimal OnTimePercentage { get; set; }
        public long OnTimeCount { get; set; }
        public long DelayCount { get; set; }
        public long CancellationCount { get; set; }
        public long TotalTripsCount { get; set; }
    }
}
