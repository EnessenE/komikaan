using komikaan.Data.GTFS;
using komikaan.Data.Models;

namespace komikaan.Data.API
{
    public class NearbyUserData
    {
        public IEnumerable<GTFSSearchStop> Stops { get; set; }
        public IEnumerable<KomIkaanVehiclePosition> Vehicles { get; set; }
    }
}
