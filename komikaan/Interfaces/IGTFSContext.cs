using GTFS.Entities.Enumerations;
using komikaan.Controllers;
using komikaan.Data.GTFS;

namespace komikaan.Interfaces;

public interface IGTFSContext
{
    Task StartAsync(CancellationToken cancellationToken);
    Task LoadRelevantDataAsync(CancellationToken cancellationToken);
    Task<IEnumerable<GTFSSearchStop>> FindAsync(string text, CancellationToken cancellationToken);
    Task<IEnumerable<GTFSSearchStop>> GetNearbyStopsAsync(double longitude, double latitude, CancellationToken cancellationToken);
    Task<GTFSTrip> GetTripAsync(Guid tripId, DateTimeOffset date);
    Task<GTFSStopData?> GetStopAsync(Guid stopId, StopType stopType);
    Task<List<GTFSSearchStop>> GetCachedStopsAsync();

}
