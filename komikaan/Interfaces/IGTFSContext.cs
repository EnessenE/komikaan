using GTFS.Entities;
using GTFS.Entities.Enumerations;
using komikaan.Data.GTFS;
using komikaan.Data.Models;

namespace komikaan.Interfaces;

public interface IGTFSContext
{
    Task StartAsync(CancellationToken cancellationToken);
    Task LoadRelevantDataAsync(CancellationToken cancellationToken);
    Task<IEnumerable<GTFSSearchStop>> FindAsync(string text, CancellationToken cancellationToken);
    Task<IEnumerable<GTFSSearchStop>> GetNearbyStopsAsync(double longitude, double latitude, CancellationToken cancellationToken);
    Task<IEnumerable<VehiclePosition>> GetNearbyVehiclesAsync(double longitude, double latitude, CancellationToken cancellationToken);
    Task<GTFSTrip> GetTripAsync(Guid tripId, DateTimeOffset date);
    Task<GTFSStopData?> GetStopAsync(Guid stopId, StopType stopType);
    Task<IEnumerable<Feed>> GetFeedsAsync();
    Task<IEnumerable<GTFSRoute>?> GetRoutesAsync(string dataOrigin);
    Task<IEnumerable<Shape>?> GetShapesAsync(string dataOrigin);
    Task<IEnumerable<GTFSSearchStop>?> GetStopsAsync(string dataOrigin);
    Task<IEnumerable<VehiclePosition>?> GetPositionsAsync(string dataOrigin);
}
