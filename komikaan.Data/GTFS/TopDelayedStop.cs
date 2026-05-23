namespace komikaan.Data.GTFS
{
    public class TopDelayedStop
    {
        public Guid PrimaryStopId { get; set; }
        public string StopId { get; set; }
        public string StopName { get; set; }
        public long StopType { get; set; }
        public string DataOrigin { get; set; }
        public int? AverageDelay { get; set; }
        public long DelayCount { get; set; }
        public long CancellationCount { get; set; }
        public int? MaxDelay { get; set; }
        public int? MinDelay { get; set; }
    }
}
