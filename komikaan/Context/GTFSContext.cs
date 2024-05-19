using System.Data;
using Dapper;
using komikaan.Data.Enums;
using komikaan.Data.Models;
using komikaan.Interfaces;
using komikaan.Models;

namespace komikaan.Context
{
    //Random code to mess around with GTFS data
    // Very inefficient and not-prod ready
    // Essentially brute forcing to have fun
    public class GTFSContext : IDataSupplierContext
    {
        private readonly ILogger<GTFSContext> _logger;
        private readonly IList<SimpleStop> _stops;
        private readonly IDictionary<string, GTFSStop> _gtfsStops;

        private readonly string _connectionString;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
        public GTFSContext(ILogger<GTFSContext> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("gtfs") ?? throw new InvalidOperationException("A GTFS postgres database connection should be defined!");
            _stops = new List<SimpleStop>();
            _gtfsStops = new Dictionary<string, GTFSStop>();
        }

        public DataSource Supplier { get; } = DataSource.KomIkAan;
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await GenerateDataAsync();
            _logger.LogInformation("Finished reading GTFS data");
        }

        private async Task GenerateDataAsync()
        {
            var data = await LoadStopsAsync();
            GenerateStops(data);
            _logger.LogInformation("Finished generating {simpleStops}/{gtfsStops} stops", _stops.Count, _gtfsStops.Count);
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


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements", Justification = "TODO")]
        private void GenerateStops(IEnumerable<GTFSStop> stops)
        {
            foreach (var stop in stops)
            {
                _gtfsStops.Add(stop.id, stop);
                var simpleStop = new SimpleStop();
                var existingStop =
                    _stops.FirstOrDefault(existingStop => existingStop.Name.Equals(stop.name, StringComparison.InvariantCultureIgnoreCase) || existingStop.Ids[Supplier].Contains(stop.parentstation, StringComparer.InvariantCultureIgnoreCase));
                if (existingStop != null)
                {
                    if (!existingStop.Ids[Supplier].Contains(stop.id, StringComparer.InvariantCultureIgnoreCase))
                    {
                        existingStop.Ids[Supplier].Add(stop.id);
                    }
                    if (!string.IsNullOrWhiteSpace(stop.code))
                    {
                        existingStop.Codes[Supplier].Add(stop.code);
                    }
                }
                else
                {
                    simpleStop.Name = string.Intern(stop.name);
                    simpleStop.Ids.Add(Supplier, new List<string>() { stop.id });
                    simpleStop.Codes.Add(Supplier, new List<string>());
                    if (!string.IsNullOrWhiteSpace(stop.code))
                    {
                        simpleStop.Codes[Supplier].Add(stop.code);
                    }
                    if (!string.IsNullOrWhiteSpace(stop.parentstation) && !stop.parentstation.Equals(stop.id, StringComparison.InvariantCultureIgnoreCase))
                    {
                        simpleStop.Ids[Supplier].Add(stop.parentstation);
                    }
                    _stops.Add(simpleStop);
                }
            }
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

        public Task<IEnumerable<SimpleStop>> GetAllStopsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_stops.AsEnumerable());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements", Justification = "TODO")]
        public async Task<IEnumerable<SimpleTravelAdvice>> GetTravelAdviceAsync(string from, string to, CancellationToken cancellationToken)
        {
            using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString); // Use the appropriate connection type

            var simpleFromStop = _stops.FirstOrDefault(stop => stop.Name.Equals(from, StringComparison.InvariantCultureIgnoreCase));
            var simpleToStop = _stops.FirstOrDefault(stop => stop.Name.Equals(to, StringComparison.InvariantCultureIgnoreCase));
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
                if (routePartId + 1 <= route.Count-1)
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
    }
}

public class GTFSStop
{
    public string id { get; set; }
    public string code { get; set; }
    public string name { get; set; }
    public string parentstation { get; set; }
}
