using System.Data;
using Dapper;
using komikaan.Data.Enums;
using komikaan.Data.GTFS;
using komikaan.Data.Models;
using komikaan.Extensions;
using komikaan.Interfaces;
using komikaan.Models;
using Npgsql;
using static System.Net.Mime.MediaTypeNames;

namespace komikaan.Context
{
    //Random code to mess around with GTFS data
    // Very inefficient and not-prod ready
    // Essentially brute forcing to have fun
    public class GTFSContext : IDataSupplierContext
    {
        private readonly ILogger<GTFSContext> _logger;
        private readonly IDictionary<string, GTFSStop> _gtfsStops;

        private readonly string _connectionString;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
        public GTFSContext(ILogger<GTFSContext> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("gtfs") ?? throw new InvalidOperationException("A GTFS postgres database connection should be defined!");
            _gtfsStops = new Dictionary<string, GTFSStop>();
        }

        public DataSource Supplier { get; } = DataSource.KomIkAan;
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Finished reading GTFS data");
            await Task.CompletedTask;
        }


        private Task<IEnumerable<GTFSStop>> LoadStopsAsync()
        {
            using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString);
            var stops = dbConnection.Query<GTFSStop>(
                @"select ""Id"", ""Code"", ""Name"", ""ParentStation"" from stops",
                commandType: CommandType.Text,
                buffered: true
            );

            return Task.FromResult(stops);
        }

        public Task LoadRelevantDataAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("No data to reload");
            return Task.CompletedTask;
        }

        public Task<IEnumerable<SimpleDisruption>> GetDisruptionsAsync(string from, string to, CancellationToken cancellationToken)
        {
            return Task.FromResult(Enumerable.Empty<SimpleDisruption>());
        }

        public Task<IEnumerable<SimpleDisruption>> GetAllDisruptionsAsync(bool active, CancellationToken cancellationToken)
        {
            return Task.FromResult(Enumerable.Empty<SimpleDisruption>());
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements", Justification = "TODO")]
        public async Task<IEnumerable<SimpleTravelAdvice>> GetTravelAdviceAsync(string from, string to, CancellationToken cancellationToken)
        {
            using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString); // Use the appropriate connection type

            var searchDate = new DateTime(2024, 05, 19);

            _logger.LogInformation("A unrecognized stop was present {fromStop} - {toStop}", from, to);
            return await Task.FromResult<IEnumerable<SimpleTravelAdvice>>(Enumerable.Empty<SimpleTravelAdvice>());

        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements", Justification = "TODO")]
        private static void CalculateRoute(List<RouteInfo> route, SimpleTravelAdvice travelAdvice)
        {
            for (var routePartId = 0; routePartId < route.Count; routePartId++)
            {
                var routePart = route[routePartId];
                var part = new SimpleRoutePart();

                // part.LineName = tripTime.route_short_name;
                // part.Direction = tripTime.trip_headsign;
                // part.PlannedDepartureTrack = tripTime.platform_code_start?.ToString();
                // part.PlannedArrivalTrack = tripTime.platform_code_end?.ToString();
                // part.PlannedArrival = new DateTime(DateOnly.FromDateTime(DateTime.UtcNow),
                //     new TimeOnly(tripTime.arrival_time_end.Hours, tripTime.arrival_time_end.Minutes,
                //         tripTime.arrival_time_end.Seconds));
                // part.PlannedDeparture = new DateTime(DateOnly.FromDateTime(DateTime.UtcNow),
                //     new TimeOnly(tripTime.departure_time_start.Hours, tripTime.departure_time_start.Minutes,
                //         tripTime.departure_time_start.Seconds));
                // part.DepartureStation = tripTime.stop_name_start;
                // part.ArrivalStation = tripTime.stop_name_end;
                // part.Operator = tripTime.agency_name;
                // part.RealisticTransfer = true;
                // part.AlternativeTransport = false;
                // part.Type = tripTime.route_type?.ToLegType() ?? LegType.Unknown;


                part.PlannedArrival = DateTime.UtcNow;
                part.PlannedDeparture = DateTime.UtcNow.AddMinutes(5);
                part.DepartureStation = routePart.out_stop_name;
                if (routePartId + 1 <= route.Count - 1)
                {
                    part.ArrivalStation = route[routePartId + 1].out_stop_name;
                }

                part.Operator = "flixbus";
                part.RealisticTransfer = true;
                part.AlternativeTransport = false;
                part.Type = LegType.Bus;

                travelAdvice.Route.Add(part);
            }
            travelAdvice.Route.Remove(travelAdvice.Route.Last());
        }

        public async Task FindAsync(string text, List<SimpleStop> stopsToFill, CancellationToken cancellationToken)
        {
            using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString);

            var foundStops = await dbConnection.QueryAsync<GTFSStop>(
                @"select * from search_stop(@search)",
                new { search = text.ToLowerInvariant() },
                commandType: CommandType.Text
            );
            var finalStops = foundStops?.ToList() ?? new List<GTFSStop>();
            var stops = new List<SimpleStop>();
            foreach (GTFSStop stop in finalStops)
            {
                var convertedStop = stop.ToSimpleStop();
                convertedStop.Ids = new List<string>() { stop.Id };
                stopsToFill.Add(convertedStop);
            }
        }

        internal async Task<IEnumerable<GTFSStopTime>> GetDeparturesAsync(string stopId)
        {
            using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString);

            var foundStops = await dbConnection.QueryAsync<GTFSStopTime>(
            @"select * from get_stop_times_from_stop(@search)",
                new { search = stopId.ToLowerInvariant() },
                commandType: CommandType.Text
            );
            return foundStops;
        }
    }
}
