namespace komikaan.Data.Models
{
    public class Feed
    {
        public string Name { get; set; }
        public string Credits { get; set; }
        public TimeSpan Interval { get; set; }
        public DateTimeOffset? LastChecked { get; set; }
        public string State { get; set; }
        public DateTimeOffset? LastFailure { get; set; }
        public DateTimeOffset? LastCheckFailure { get; set; } // last_check_failure

        public DateTimeOffset? LastCheck { get; set; }        // last_check
        public DateTimeOffset? LastImportStart { get; set; }  // last_import_start
        public DateTimeOffset? LastImportSuccess { get; set; } // last_import_success
        public DateTimeOffset? LastImportFailure { get; set; } // last_import_failure

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
