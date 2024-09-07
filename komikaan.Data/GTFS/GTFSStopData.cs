﻿using System.Text.Json.Serialization;
using GTFS.Entities.Enumerations;

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
        public string? ScheduleRelationship { get; set; }
        public IEnumerable<GTFSRoute>? Routes { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StopType StopType { get; set; }
        public IEnumerable<GTFSStopTime>? Departures { get; set; }
        public IEnumerable<GTFSStopData>? RelatedStops { get; set; }
    }
}
