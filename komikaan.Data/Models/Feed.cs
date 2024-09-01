namespace komikaan.Controllers
{
    public class Feed
    {
        public string Name { get; set; }
        public TimeSpan Interval { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public DateTimeOffset? LastAttempt { get; set; }
        public DateTimeOffset? LastChecked { get; set; }
        public bool DownloadPending { get; set; }
        public int Agencies { get; set; }
        public int Routes { get; set; }
        public int Stops { get; set; }
        public int Trips { get; set; }
    }
}
