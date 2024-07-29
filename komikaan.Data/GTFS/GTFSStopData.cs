using System.Text.Json.Serialization;

namespace komikaan.Data.GTFS
{
    public class GTFSStopData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Zone { get; set; }
        public string LocationType { get; set; }
        public string ParentStation { get; set; }
        public string PlatformCode { get; set; }
        public string DataOrigin { get; set; }
        public DateTime LastUpdated { get; set; }
        public int MergedStops { get; set; }
        public IEnumerable<GTFSRoute>? Routes { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StopType StopType { get; set; }
        public IEnumerable<GTFSStopTime>? Departures { get; set; }
        public IEnumerable<GTFSStopData>? RelatedStops { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StopType
    {
        Bus = 1,
        Train = 2,
        Metro = 3,
        Tram = 4,
        Bicycle = 5,
        Coach = 6,
        Ferry = 7,
        CableCar = 8,
        Gondola = 9,
        Monorail = 10,
        Unknown = 1000,
        Mixed = 1001
    }
}
