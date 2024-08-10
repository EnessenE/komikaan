using komikaan.Data.Enums;
using komikaan.Data.GTFS;
using komikaan.Interfaces;
using komikaan.Models.API.NS;

namespace komikaan.Tests.Stubs
{
    public class StubContext : IGTFSContext
    {
        public StubContext()
        {
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task LoadRelevantDataAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<GTFSStop>> FindAsync(string text, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<GTFSStop>> GetNearbyStopsAsync(double longitude, double latitude, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<GTFSTrip> GetTripAsync(Guid tripId, DateTimeOffset date)
        {
            throw new NotImplementedException();
        }

        public Task<GTFSStopData?> GetStopAsync(Guid stopId, StopType stopType)
        {
            throw new NotImplementedException();
        }

        public Task<IList<GTFSStop>> GetCachedStopsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
