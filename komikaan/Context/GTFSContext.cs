using System.Data;
using Dapper;
using GTFS.Entities;
using komikaan.Controllers;
using komikaan.Data.Enums;
using komikaan.Data.GTFS;
using komikaan.Handlers;
using komikaan.Interfaces;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using Npgsql;

namespace komikaan.Context
{
    //Random code to mess around with GTFS data
    // Very inefficient and not-prod ready
    // Essentially brute forcing to have fun
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements", Justification = "TODO")]
    public class GTFSContext : IGTFSContext
    {
        private readonly ILogger<GTFSContext> _logger;

        private readonly string _connectionString;
        private readonly NpgsqlDataSourceBuilder _dataSourceBuilder;
        private List<GTFSSearchStop> _allStops;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
        public GTFSContext(ILogger<GTFSContext> logger, IConfiguration configuration)
        {
            SqlMapper.AddTypeHandler(new SqlDateOnlyTypeHandler());
            SqlMapper.AddTypeHandler(new SqlTimeOnlyTypeHandler());
            SqlMapper.AddTypeHandler(new DoubleArrayHandler());

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

            _logger = logger;
            _connectionString = configuration.GetConnectionString("gtfs") ?? throw new InvalidOperationException("A GTFS postgres database connection should be defined!");


            _dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
            _dataSourceBuilder.UseNetTopologySuite(new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XYM), geographyAsDefault: true);
            _dataSourceBuilder.UseGeoJson();


        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Finished reading GTFS data");
            _allStops = (await GetAllStopsAsync()).ToList();
            await Task.CompletedTask;
        }

        public async Task LoadRelevantDataAsync(CancellationToken cancellationToken)
        {
            _allStops = (await GetAllStopsAsync()).ToList();
        }

        public async Task<IEnumerable<GTFSSearchStop>> FindAsync(string text, CancellationToken cancellationToken)
        {
            using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString);

            var foundStops = await dbConnection.QueryAsync<GTFSSearchStop>(
                @"select * from search_stop(@search)",
                new { search = text.ToLowerInvariant() },
                commandType: CommandType.Text
            );

            foreach ( var stop in foundStops )
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

            var stop = await dbConnection.QueryFirstOrDefaultAsync<GTFSStopData>(
            @"select * from get_stop_from_id(@stopid, @stop_type) LIMIT 1",
                new
                {
                    stopid = stopId,
                    stop_type = stopType
                },
                commandType: CommandType.Text
            );

            if (stop != null)
            {
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

                stop.RelatedStops = await dbConnection.QueryAsync<GTFSStopData>(
                @"select * from get_related_stops(@stop, @stop_type)",
                    new
                    {
                        stop = stopId,
                        stop_type = stopType
                    },
                    commandType: CommandType.Text
                );

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

                stop.RelatedStops = keptStations;
            }
            return stop;
        }

        public async Task<IEnumerable<GTFSSearchStop>> GetNearbyStopsAsync(double longitude, double latitude, CancellationToken cancellationToken)
        {
            await using var connection = await (_dataSourceBuilder.Build()).OpenConnectionAsync();
            var foundStops = await connection.QueryAsync<GTFSSearchStop>(
            @"select * from nearby_stops(@latitude, @longitude)",
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
            await using var connection = await (_dataSourceBuilder.Build()).OpenConnectionAsync();
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

        public Task<List<GTFSSearchStop>> GetCachedStopsAsync()
        {
            return Task.FromResult(_allStops);
        }
    }
}
