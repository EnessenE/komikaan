using komikaan.Context;
using komikaan.Data.API;
using komikaan.Data.GTFS;
using komikaan.Data.Models;
using komikaan.GTFS.Models.Static.Enums;
using Shape = komikaan.GTFS.Models.Static.Models.Shape;

namespace komikaan.Interfaces;

public interface IGTFSContext
{
    Task StartAsync(CancellationToken cancellationToken);
    Task LoadRelevantDataAsync(CancellationToken cancellationToken);
    Task<IEnumerable<GTFSSearchStop>> FindAsync(string text, CancellationToken cancellationToken);
    Task<IEnumerable<GTFSSearchStop>> GetNearbyStopsAsync(double longitude, double latitude, CancellationToken cancellationToken);
    Task<IEnumerable<KomIkaanVehiclePosition>> GetNearbyVehiclesAsync(double longitude, double latitude, CancellationToken cancellationToken);
    Task<GTFSTrip?> GetTripAsync(Guid tripId, DateTimeOffset date);
    Task<GTFSStopData?> GetStopAsync(string stopId, ExtendedRouteType stopType);
    Task<GTFSStopData?> GetStopAsync(string dataOrigin, string stopId);

    Task<IEnumerable<Feed>> GetFeedsAsync();
    Task<IEnumerable<string>> GetFeedNamesAsync();
    Task<IEnumerable<string>> GetRealtimeFeedNamesAsync();
    Task<IEnumerable<GTFSRoute>?> GetDataOriginRoutesAsync(string dataOrigin);
    Task<GTFSRouteDetails?> GetRouteAsync(string dataOrigin, string routeId);
    Task<IEnumerable<GTFSRouteTimetableRow>> GetTimetableAsync(string dataOrigin, string routeId, DateOnly? date);
    Task<IEnumerable<DatabaseAgency>?> GetAgenciesAsync(string dataOrigin);
    Task<IEnumerable<Shape>?> GetShapesAsync(string dataOrigin);
    Task<IEnumerable<GTFSSearchStop>?> GetStopsAsync(string dataOrigin);
    Task<IEnumerable<TrainStationPerformance>> GetTrainStationPerformanceAsync();
    Task<IEnumerable<TrainStationPerformance>> GetTrainStationPerformanceInBoundsAsync(TrainStationViewportQuery query);
    Task<MapViewportData> GetMapViewportDataAsync(MapViewportQuery query);
    Task<IEnumerable<GTFSRoute>> GetRoutesByShapeAsync(string dataOrigin, string shapeId);
    Task<IEnumerable<GTFSRoute>> GetRoutesNearPointAsync(double latitude, double longitude, double bboxDegrees);
    Task<IEnumerable<KomIkaanVehiclePosition>?> GetPositionsAsync(string dataOrigin);
    Task<IEnumerable<GTFSAlert>?> GetAlertsAsync(string dataOrigin, int limit, int offset);
    Task<IEnumerable<GTFSAlert>?> GetAlertsForStopAsync(Guid stopId, ExtendedRouteType stopType);
    Task<IEnumerable<TopDelayedStop>> GetTopDelayedStopsAsync(int limit, string? dataOrigin);
    IEnumerable<CoverageDataPoint> GetCoverage();
}
