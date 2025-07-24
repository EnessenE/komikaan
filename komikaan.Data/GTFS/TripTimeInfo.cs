using System.Numerics;
using komikaan.GTFS.Models.Static.Enums;

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

public class RouteInfo
{
    public string out_stop_name { get; set; }
    public string out_stop_id { get; set; }
    public int out_stop_num { get; set; }
    public BigInteger out_stop_pk { get; set; }
    public double out_cost { get; set; }
    public int seq { get; set; }
    public int path_seq { get; set; }
    public BigInteger start_vid { get; set; }
    public BigInteger end_vid { get; set; }
    public BigInteger node { get; set; }
    public BigInteger edge { get; set; }
}
