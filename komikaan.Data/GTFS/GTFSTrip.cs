﻿using GTFS.Entities;

namespace komikaan.Data.GTFS
{
    public class GTFSTrip
    {
        public string Id { get; set; }
        public string? RouteId { get; set; }
        public string? ServiceId { get; set; }
        public string? Headsign { get; set; }
        public string? Shortname { get; set; }
        public int? Direction { get; set; }
        public string? BlockId { get; set; }
        public string? DataOrigin { get; set; }

        //Realtime data
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? CurrentStatus { get; set; }
        public string? CongestionLevel { get; set; }
        public string? OccupancyStatus { get; set; }
        public int? OccupancyPercentage { get; set; }
        public DateTimeOffset? MeasurementTime { get; set; }
        
        // Realtime next stop
        public Guid? TargetStopId { get; set; }
        public string? TargetStopName { get; set; }

       //Route data
        public string? RouteShortName { get; set; }
        public string? RouteLongName { get; set; }

        public IEnumerable<GTFSTripStop>? Stops { get; set; }

        public IEnumerable<Shape>? Shapes { get; set; }
    }
}
