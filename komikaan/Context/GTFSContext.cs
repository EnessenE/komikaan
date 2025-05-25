using System.Data;
using System.Diagnostics;
using Dapper;
using GTFS.Entities;
using GTFS.Entities.Enumerations;
using komikaan.Data.GTFS;
using komikaan.Data.Models;
using komikaan.Extensions;
using komikaan.Handlers;
using komikaan.Interfaces;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using Npgsql;

namespace komikaan.Context
{
    // Random code to mess around with GTFS data
    // Very inefficient and not-prod ready
    // Essentially brute forcing to have fun
    // Also dont write this stuff at 23:00 while drunk
    // I am not even joking
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements", Justification = "This entire class needs a refactor")]
    public class GTFSContext : IGTFSContext
    {
        private readonly ILogger<GTFSContext> _logger;

        private readonly string _connectionString;
        private readonly NpgsqlDataSource _dataSource;
        private List<Feed> _allFeeds;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "This entire class needs a refactor")]
        public GTFSContext(ILogger<GTFSContext> logger, IConfiguration configuration)
        {
            SqlMapper.AddTypeHandler(new SqlDateOnlyTypeHandler());
            SqlMapper.AddTypeHandler(new SqlTimeOnlyTypeHandler());
            SqlMapper.AddTypeHandler(new DoubleArrayHandler());

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

            _logger = logger;
            _connectionString = configuration.GetConnectionString("gtfs") ?? throw new InvalidOperationException("A GTFS postgres database connection should be defined!");

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
            _dataSource = dataSourceBuilder.Build();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Finished reading GTFS data");
            _allFeeds = (await CacheFeedsAsync()).ToList();
            await Task.CompletedTask;
        }

        public async Task LoadRelevantDataAsync(CancellationToken cancellationToken)
        {
            _allFeeds = (await CacheFeedsAsync()).ToList();
        }

        public async Task<IEnumerable<GTFSSearchStop>> FindAsync(string text, CancellationToken cancellationToken)
        {
            using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString);

            var foundStops = await dbConnection.QueryAsync<GTFSSearchStop>(
                @"select * from search_stop(@search)",
                new { search = text.ToLowerInvariant() },
                commandType: CommandType.Text
            );

            foreach (var stop in foundStops)
            {
                FixCoordinates(stop);
            }
            return foundStops;
        }

        private static void FixCoordinates(GTFSSearchStop stop)
        {
            if (stop.Coordinates != null)
            {
                foreach (var item in stop.Coordinates)
                {
                    stop.AdjustedCoordinates.Add(new SimpleCoordinate() { Longitude = item[0], Latitude = item[1] });
                }
            }
        }

        public async Task<GTFSTrip> GetTripAsync(Guid tripId, DateTimeOffset date)
        {
            using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString);
            //TODO: Take in account the relative timezone for us + user

            var trip = await dbConnection.QueryFirstAsync<GTFSTrip>(
            @"select * from get_trip_from_id(@tripid) LIMIT 1",
                new
                {
                    tripid = tripId
                },
                commandType: CommandType.Text
            );
            var foundStops = await dbConnection.QueryAsync<GTFSTripStop>(
            @"select * from get_stop_times_for_trip(@tripid)",
                new
                {
                    tripid = tripId
                },
                commandType: CommandType.Text
            );
            // This is hack, we need a "calibration" point for the time as we can't get it from the DB and summer time is a thing
            // Note, this will horribly break around summer/winter time switches
            DateTimeOffset? previousArrival = null;
            DateTimeOffset? previousDeparture = null;
            int offset = 0;
            foreach (GTFSTripStop stop in foundStops)
            {
                var newArrival = forceDate(offset, stop.Arrival, previousArrival);
                previousArrival = stop.Arrival;
                stop.Arrival = newArrival;

                var newDep = forceDate(offset, stop.Departure, previousDeparture);
                previousDeparture = stop.Departure;
                stop.Departure = newDep;
            }

            trip.Stops = foundStops;
            var shapes = await dbConnection.QueryAsync<Shape>(
            @"select * from get_shapes_from_trip(@tripid)",
                new
                {
                    tripid = tripId
                },
                commandType: CommandType.Text
            );
            trip.Shapes = shapes;

            return trip;
        }

        private DateTimeOffset? forceDate(int offset, DateTimeOffset? date, DateTimeOffset? previous)
        {
            if (date != null)
            {
                var newArrival = date.Value.AddYears(DateTimeOffset.UtcNow.Year - 1);
                newArrival = newArrival.AddMonths(DateTimeOffset.UtcNow.Month - 1);
                newArrival = newArrival.AddDays(DateTimeOffset.UtcNow.Day - 1);
                return newArrival;
            }
            return null;
        }

        public async Task<GTFSStopData?> GetStopAsync(Guid stopId, StopType stopType)
        {
            using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString);
            var stopwatch = Stopwatch.StartNew();
            var stop = await dbConnection.QueryFirstOrDefaultAsync<GTFSStopData>(
            @"select * from get_stop_from_id(@stopid, @stop_type) LIMIT 1",
                new
                {
                    stopid = stopId,
                    stop_type = stopType
                },
                commandType: CommandType.Text
            );
            _logger.LogInformation("Retrieved basic stop {time}", stopwatch.Elapsed);

            if (stop != null)
            {
                var mergedStops = await dbConnection.QueryAsync<GTFSStopData>(
                @"select * from get_stoplocations_from_id(@stopid, @stop_type)",
                    new
                    {
                        stopid = stopId,
                        stop_type = stopType
                    },
                    commandType: CommandType.Text
                );
                stop.MergedStops = mergedStops;
                _logger.LogInformation("Retrieved mergedStops stop {time}", stopwatch.Elapsed);


                //TODO: Take in account used timezone for the user
                var foundStops = await dbConnection.QueryAsync<GTFSStopTime>(
                @"select * from get_stop_times_from_stop(@stop, @stop_type, @time)",
                    new
                    {
                        stop = stopId,
                        stop_type = stopType,
                        time = DateTimeOffset.UtcNow.AddMinutes(-2)
                    },
                    commandType: CommandType.Text
                );
                stop.Departures = foundStops;
                _logger.LogInformation("Retrieved departures {time}", stopwatch.Elapsed);

                stop.RelatedStops = await dbConnection.QueryAsync<GTFSStopData>(
                @"select * from get_related_stops(@stop, @stop_type)",
                    new
                    {
                        stop = stopId,
                        stop_type = stopType
                    },
                    commandType: CommandType.Text
                );
                _logger.LogInformation("Retrieved related stops {time}", stopwatch.Elapsed);

                stop.Routes = await dbConnection.QueryAsync<GTFSRoute>(
                @"select * from get_routes_from_stop(@stop, @stop_type)",
                    new
                    {
                        stop = stopId,
                        stop_type = stopType
                    },
                    commandType: CommandType.Text
                );

                _logger.LogInformation("Retrieved routes {time}", stopwatch.Elapsed);

                var keptStations = new List<GTFSStopData>();

                foreach (var relatedStop in stop.RelatedStops)
                {
                    if (!keptStations.Exists(relatedUnfiltered => relatedUnfiltered.StopType == relatedStop.StopType))
                    {
                        keptStations.Add(relatedStop);
                    }
                    else
                    {
                        if (keptStations.Any(filteredStop => filteredStop.StopType == relatedStop.StopType && (!string.IsNullOrEmpty(relatedStop.ParentStation) && string.IsNullOrEmpty(filteredStop.ParentStation))))
                        {
                            _logger.LogInformation("Showing a nicer station name from a parent");
                            keptStations.RemoveAll(filteredStop => filteredStop.StopType == relatedStop.StopType);
                            keptStations.Add(relatedStop);
                        }
                    }
                }

                _logger.LogInformation("Fixed stops {time}", stopwatch.Elapsed);
                stop.RelatedStops = keptStations;
            }
            return stop;
        }

        public async Task<IEnumerable<GTFSSearchStop>> GetNearbyStopsAsync(double longitude, double latitude, CancellationToken cancellationToken)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var foundStops = await connection.QueryAsync<GTFSSearchStop>(
            @"select * from nearby_stops(@longitude, @latitude)",
                new { longitude = longitude, latitude = latitude },
                commandType: CommandType.Text
            );

            foreach (var stop in foundStops)
            {
                FixCoordinates(stop);
            }
            return foundStops;
        }

        private async Task<IEnumerable<GTFSSearchStop>> GetAllStopsAsync()
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var foundStops = await connection.QueryAsync<GTFSSearchStop>(
            @"select * from get_all_stops()",
                commandType: CommandType.Text
            );
            foreach (var stop in foundStops)
            {
                FixCoordinates(stop);
            }
            return foundStops;
        }

        private async Task<IEnumerable<Feed>> CacheFeedsAsync()
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var data = await connection.QueryAsync<Feed>(
            @"select * from get_all_feeds()",
                commandType: CommandType.Text
            );
            return data;
        }

        public Task<IEnumerable<Feed>> GetFeedsAsync()
        {
            return Task.FromResult(_allFeeds.AsEnumerable());
        }

        public async Task<IEnumerable<GTFSRoute>?> GetRoutesAsync(string dataOrigin)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var data = await connection.QueryAsync<GTFSDatabaseRoute>(
            @"select * from get_routes_from_data_origin(@dataorigin)",
                new { dataorigin = dataOrigin },
                commandType: CommandType.Text
            );
            var items = new List<GTFSRoute>();
            foreach (var item in data)
            {
                var route = (GTFSRoute)item;
                route.Type = item.Type.ConvertStopType();
                items.Add(route);
            }

            return items;
        }

        public async Task<IEnumerable<Shape>?> GetShapesAsync(string dataOrigin)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var data = await connection.QueryAsync<Shape>(
            @"select * from get_shapes_from_data_origin(@dataorigin)",
                new { dataorigin = dataOrigin },
                commandType: CommandType.Text
            );

            return data;
        }

        public async Task<IEnumerable<GTFSSearchStop>?> GetStopsAsync(string dataOrigin)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var data = await connection.QueryAsync<GTFSSearchStop>(
            @"select * from get_stops_from_data_origin(@dataorigin)",
                new { dataorigin = dataOrigin },
                commandType: CommandType.Text
            );

            foreach (var stop in data)
            {
                FixCoordinates(stop);
            }

            return data;
        }

        public async Task<IEnumerable<VehiclePosition>?> GetPositionsAsync(string dataOrigin)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var data = await connection.QueryAsync<VehiclePosition>(
            @"select * from get_positions_from_data_origin(@dataorigin)",
                new { dataorigin = dataOrigin },
                commandType: CommandType.Text
            );

            return data;
        }

        async Task<IEnumerable<VehiclePosition>> IGTFSContext.GetNearbyVehiclesAsync(double longitude, double latitude, CancellationToken cancellationToken)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var vehicles = await connection.QueryAsync<VehiclePosition>(
            @"select * from nearby_vehicles(@longitude, @latitude, @distance)",
                new { longitude = longitude, latitude = latitude, distance = 800 },
                commandType: CommandType.Text
            );

            return vehicles;
        }

        public async Task<IEnumerable<GTFSAlert>?> GetAlertsAsync(string dataOrigin)
        {
            await using var connection = await _dataSource.OpenConnectionAsync(); 
            var alerts = await connection.QueryAsync<GTFSAlert>(
                "SELECT * FROM public.get_alerts_from_data_origin(@dataOriginParam)",
                new { dataOriginParam = dataOrigin },
                commandType: CommandType.Text 
            );

            return alerts;
        }

    }
}
