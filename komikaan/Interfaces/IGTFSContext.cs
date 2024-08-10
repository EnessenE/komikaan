using komikaan.Data.Enums;
using komikaan.Data.GTFS;
using komikaan.Models;

namespace komikaan.Interfaces;

public interface IGTFSContext
{
    Task StartAsync(CancellationToken cancellationToken);
    Task LoadRelevantDataAsync(CancellationToken cancellationToken);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1564:Parameter in public or internal member is of type bool or bool?", Justification = "We are selecting data for active / inactive disruptions. This is intended")]
    Task<IEnumerable<GTFSStop>> FindAsync(string text, CancellationToken cancellationToken);
    Task<IEnumerable<GTFSStop>> GetNearbyStopsAsync(double longitude, double latitude, CancellationToken cancellationToken);
    Task<GTFSTrip> GetTripAsync(Guid tripId, DateTimeOffset date);
    Task<GTFSStopData?> GetStopAsync(Guid stopId, StopType stopType);
    Task<List<GTFSStop>> GetCachedStopsAsync();

}
