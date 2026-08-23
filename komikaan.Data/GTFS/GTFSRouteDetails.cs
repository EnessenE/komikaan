using komikaan.Context;

namespace komikaan.Data.GTFS
{
    public class GTFSRouteDetails : GTFSRoute
    {
        public int StopCount { get; set; }
        public int TripCount { get; set; }
        public DateOnly? FirstRun { get; set; }
        public DateOnly? LastRun { get; set; }
        public IEnumerable<KomikaanShape> Shapes { get; set; } = Enumerable.Empty<KomikaanShape>();
        public IEnumerable<GTFSSearchStop> Stops { get; set; } = Enumerable.Empty<GTFSSearchStop>();
        public IEnumerable<GTFSRouteTimetableRow> Timetable { get; set; } = Enumerable.Empty<GTFSRouteTimetableRow>();
    }
}
