using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace komikaan.Data.GTFS
{
    public class GTFSStopTime
    {
        public string TripId { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public DateTime? DeperatureTime { get; set; }
        public string StopHeadsign { get; set; }
        public string Shortname { get; set; }
        public string RouteShortName { get; set; }
        public string RouteLongName { get; set; }
        public string DataOrigin { get; set; }
    }
}
