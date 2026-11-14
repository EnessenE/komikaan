namespace komikaan.Settings
{
    public class DataServiceSettings
    {
        public const string SectionName = "DataService";

        /// <summary>
        /// When true (default), data suppliers are started on application startup.
        /// Set to false in development to skip the initial load.
        /// </summary>
        public bool StartSuppliers { get; set; } = true;

        /// <summary>
        /// Interval in minutes between reloads of cached data suppliers.
        /// </summary>
        public int ReloadIntervalMinutes { get; set; } = 3;
    }
}
