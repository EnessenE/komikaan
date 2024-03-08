using GTFS.Entities.Enumerations;

public class TripTimeInfo
{
    public string? trip_headsign { get; set; }
    public string trip_id { get; set; }
    public string route_id { get; set; }
    public string? platform_code_start { get; set; }
    public string? platform_code_end { get; set; }
    public string stop_name_start { get; set; }
    public string stop_name_end { get; set; }
    public string agency_name { get; set; }
    public string route_long_name { get; set; }
    public string route_short_name { get; set; }
    public RouteType? route_type { get; set; }
    public TimeSpan arrival_time_end { get; set; }
    public TimeSpan departure_time_start { get; set; }
}
