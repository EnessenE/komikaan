using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Dapper;
using komikaan.Data.GTFS;
using komikaan.Data.Models;
using komikaan.GTFS.Models.Static.Enums;
using komikaan.GTFS.Models.Static.Models;
using komikaan.Handlers;
using komikaan.Interfaces;
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

        public async Task<GTFSTrip?> GetTripAsync(Guid tripId, DateTimeOffset date)
        {
            using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString);
            //TODO: Take in account the relative timezone for us + user

            var trip = await dbConnection.QueryFirstOrDefaultAsync<GTFSTrip>(
            @"select * from get_trip_from_id(@tripid) LIMIT 1",
                new
                {
                    tripid = tripId
                },
                commandType: CommandType.Text
            );

            if (trip != null)
            {
                var foundStops = await dbConnection.QueryAsync<GTFSTripStop>(
                @"select * from get_stop_times_for_trip(@tripid)",
                    new
                    {
                        tripid = tripId
                    },
                    commandType: CommandType.Text
                );

                trip.Stops = foundStops;
                var shapes = await dbConnection.QueryAsync<KomikaanShape>(
                @"select * from get_shapes_from_trip(@tripid)",
                    new
                    {
                        tripid = tripId
                    },
                    commandType: CommandType.Text
                );
                trip.Shapes = shapes;
            }

            return trip;
        }

        public async Task<GTFSStopData?> GetStopAsync(string dataOrigin, string stopId)
        {
            using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString);

            var stopwatch = Stopwatch.StartNew();

            var stop = await dbConnection.QueryFirstOrDefaultAsync<GTFSStopData>(
                @"select * from get_exact_stop_from_id(@stopid, @data_origin) LIMIT 1",
                new
                {
                    stopid = stopId,
                    data_origin = dataOrigin
                },
                commandType: CommandType.Text
            );

            _logger.LogInformation("Retrieved basic stop {time}", stopwatch.Elapsed);

            if (stop == null)
                return null;
            var primaryStopId = stop.PrimaryStop.ToString();

            stop.MergedStops = await GetMergedStopsAsync(dbConnection, primaryStopId, stop.StopType);
            _logger.LogInformation("Retrieved merged stops {time}", stopwatch.Elapsed);

            stop.Departures = await GetDeparturesAsync(dbConnection, primaryStopId, stop.StopType);
            _logger.LogInformation("Retrieved departures {time}", stopwatch.Elapsed);

            stop.RelatedStops = await GetRelatedStopsAsync(dbConnection, primaryStopId, stop.StopType);
            _logger.LogInformation("Retrieved related stops {time}", stopwatch.Elapsed);

            stop.Routes = await GetStopRoutesAsync(dbConnection, primaryStopId, stop.StopType);
            _logger.LogInformation("Retrieved routes {time}", stopwatch.Elapsed);

            stop.RelatedStops = FilterRelatedStops(stop.RelatedStops);
            _logger.LogInformation("Fixed stops {time}", stopwatch.Elapsed);

            return stop;
        }

        public async Task<GTFSStopData?> GetStopAsync(string stopId, ExtendedRouteType routeType)
        {
            using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString);
            var stopwatch = Stopwatch.StartNew();

            var stop = await dbConnection.QueryFirstOrDefaultAsync<GTFSStopData>(
                @"select * from get_stop_from_id(@stopid, @stop_type) LIMIT 1",
                new
                {
                    stopid = stopId,
                    stop_type = routeType
                },
                commandType: CommandType.Text
            );
            _logger.LogInformation("Retrieved basic stop {time}", stopwatch.Elapsed);

            if (stop == null)
                return null;
            stop.PrimaryStop = Guid.Parse(stopId);

            stop.MergedStops = await GetMergedStopsAsync(dbConnection, stopId, routeType);
            _logger.LogInformation("Retrieved merged stops {time}", stopwatch.Elapsed);

            stop.Departures = await GetDeparturesAsync(dbConnection, stopId, routeType);
            _logger.LogInformation("Retrieved departures {time}", stopwatch.Elapsed);

            stop.RelatedStops = await GetRelatedStopsAsync(dbConnection, stopId, routeType);
            _logger.LogInformation("Retrieved related stops {time}", stopwatch.Elapsed);

            stop.Routes = await GetStopRoutesAsync(dbConnection, stopId, routeType);
            _logger.LogInformation("Retrieved routes {time}", stopwatch.Elapsed);

            stop.RelatedStops = FilterRelatedStops(stop.RelatedStops);
            _logger.LogInformation("Fixed stops {time}", stopwatch.Elapsed);

            return stop;
        }


        private async Task<IEnumerable<GTFSStopData>> GetMergedStopsAsync(
            Npgsql.NpgsqlConnection dbConnection,
            string stopId,
            ExtendedRouteType routeType)
        {
            return await dbConnection.QueryAsync<GTFSStopData>(
                @"select * from get_stoplocations_from_id(@stopid, @stop_type)",
                new
                {
                    stopid = stopId,
                    stop_type = routeType
                },
                commandType: CommandType.Text
            );
        }

        private async Task<IEnumerable<GTFSStopTime>> GetDeparturesAsync(
            Npgsql.NpgsqlConnection dbConnection,
            string stopId,
            ExtendedRouteType routeType)
        {
            // TODO: take user timezone into account
            return await dbConnection.QueryAsync<GTFSStopTime>(
                @"select * from get_stop_times_from_stop(@stop, @stop_type, @time)",
                new
                {
                    stop = stopId,
                    stop_type = routeType,
                    time = DateTimeOffset.UtcNow.AddMinutes(-2)
                },
                commandType: CommandType.Text
            );
        }

        private async Task<IEnumerable<GTFSStopData>> GetRelatedStopsAsync(
            Npgsql.NpgsqlConnection dbConnection,
            string stopId,
            ExtendedRouteType routeType)
        {
            return await dbConnection.QueryAsync<GTFSStopData>(
                @"select * from get_related_stops(@stop, @stop_type)",
                new
                {
                    stop = stopId,
                    stop_type = routeType
                },
                commandType: CommandType.Text
            );
        }

        private async Task<IEnumerable<GTFSRoute>> GetStopRoutesAsync(Npgsql.NpgsqlConnection dbConnection, string stopId, ExtendedRouteType routeType)
        {
            return await dbConnection.QueryAsync<GTFSRoute>(
                @"select * from get_routes_from_stop(@stop, @stop_type)",
                new
                {
                    stop = stopId,
                    stop_type = routeType
                },
                commandType: CommandType.Text
            );
        }

        private static IEnumerable<GTFSStopData> FilterRelatedStops(IEnumerable<GTFSStopData> relatedStops)
        {
            var keptStations = new List<GTFSStopData>();

            foreach (var relatedStop in relatedStops)
            {
                var existing = keptStations.FirstOrDefault(stops => stops.StopType == relatedStop.StopType);

                if (existing == null)
                {
                    keptStations.Add(relatedStop);
                    continue;
                }

                if (!string.IsNullOrEmpty(relatedStop.ParentStation) &&
                    string.IsNullOrEmpty(existing.ParentStation))
                {
                    keptStations.Remove(existing);
                    keptStations.Add(relatedStop);
                }
            }

            return keptStations;
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

        public async Task<IEnumerable<GTFSRoute>?> GetDataOriginRoutesAsync(string dataOrigin)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var data = await connection.QueryAsync<GTFSRoute>(
            @"select * from get_routes_from_data_origin(@dataorigin)",
                new { dataorigin = dataOrigin },
                commandType: CommandType.Text
            );
            var items = new List<GTFSRoute>();
            foreach (var item in data)
            {
                var route = (GTFSRoute)item;
                items.Add(route);
            }

            return items;
        }

        public async Task<IEnumerable<DatabaseAgency>?> GetAgenciesAsync(string dataOrigin)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var data = await connection.QueryAsync<DatabaseAgency>(
            @"select * from get_agencies_from_data_origin(@dataorigin)",
                new { dataorigin = dataOrigin },
                commandType: CommandType.Text
            );

            return data;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1551:Method overload should call another overload", Justification = "<Pending>")]
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1551:Method overload should call another overload", Justification = "<Pending>")]
        public async Task<IEnumerable<GTFSAlert>?> GetAlertsAsync(Guid stopId, ExtendedRouteType stopType)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var alerts = await connection.QueryAsync<GTFSAlert>(
                "SELECT * FROM public.get_alerts_from_stop(@stop_id, @stop_type)",
                new
                {
                    stop_id = stopId,
                    stop_type = stopType
                },
                commandType: CommandType.Text
            );

            return alerts;
        }
    }
}

