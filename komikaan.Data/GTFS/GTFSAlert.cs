namespace komikaan.Data.Models
{
    public class GTFSAlert
    {
        public string DataOrigin { get; set; }
        public string InternalId { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public string  Id { get; set; }
        public bool IsDeleted { get; set; }
        public string? ActivePeriods { get; set; } 
        public string? Cause { get; set; }
        public string? Effect { get; set; }
        public string? Url { get; set; }
        public string? HeaderText { get; set; }
        public string? DescriptionText { get; set; }
        public string? TtsHeaderText { get; set; }
        public string? TtsDescriptionText { get; set; }
        public string? SeverityLevel { get; set; }
    }
}
