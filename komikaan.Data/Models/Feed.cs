namespace komikaan.Data.Models
{
    public class Feed
    {
        public string Name { get; set; }
        public TimeSpan Interval { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset? LastAttempt { get; set; }
        public DateTimeOffset? LastChecked { get; set; }
        public DateTimeOffset? LastFailure { get; set; }
        public bool DownloadPending { get; set; }
        public int Agencies { get; set; }
        public int Routes { get; set; }
        public int Stops { get; set; }
        public int Trips { get; set; }
        public int Alerts { get; set; }
        public int Vehicles { get; set; }
        public bool RealTime { get; set; }

        public List<VehiclePosition> Positions { get; set; }
    }

}
