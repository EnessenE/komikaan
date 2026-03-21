namespace komikaan.Data.GTFS
{
    /// <summary>
    /// One cell in the classical bus-schedule timetable:
    /// a single (stop × trip) departure time. The frontend pivots
    /// these flat rows into a stops-as-rows / trips-as-columns grid.
    /// </summary>
    public class GTFSRouteTimetableRow
    {
        public string StopId { get; set; } = string.Empty;
        public string StopName { get; set; } = string.Empty;
        public long StopSequence { get; set; }
        public string TripId { get; set; } = string.Empty;
        public string TripHeadsign { get; set; } = string.Empty;
        /// <summary>
        /// Static GTFS departure time at this stop. Null when the stop is
        /// pass-through-only (no recorded departure).
        /// </summary>
        public TimeOnly? DepartureTime { get; set; }
    }
}
