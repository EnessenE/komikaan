using System.Data;
using System.Data.Common;
using Dapper;
using GTFS;
using GTFS.Entities;
using komikaan.Data.Enums;
using komikaan.Data.Models;
using komikaan.Extensions;
using komikaan.Interfaces;
using komikaan.Models;
using Stop = GTFS.Entities.Stop;
using Trip = GTFS.Entities.Trip;

namespace komikaan.Context
{
    //Random code to mess around with GTFS data
    // Very inefficient and not-prod ready
    // Essentially brute forcing to have fun
    public class OpenOVContext : IDataSupplierContext
    {
        private readonly ILogger<OpenOVContext> _logger;
        private readonly IList<SimpleStop> _stops;
        private readonly IDictionary<string, GTFSStop> _gtfsStops;

        private readonly string _connectionString;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
        public OpenOVContext(ILogger<OpenOVContext> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("gtfs") ?? throw new InvalidOperationException("A GTFS postgres database connection should be defined!");
            _stops = new List<SimpleStop>();
            _gtfsStops = new Dictionary<string, GTFSStop>();
        }

        public DataSource Supplier { get; } = DataSource.OpenOV;
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

        private async Task<IList<GTFSStop>> LoadStopsAsync()
        {
            using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString);
            var stops = await dbConnection.QueryAsync<GTFSStop>(
                @"select stop_id, stop_code, stop_name, parent_station from stops",
                commandType: CommandType.Text
            );

            return stops.ToList();
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements", Justification = "TODO")]
        private void GenerateStops(IList<GTFSStop> stops)
        {
            foreach (var stop in stops)
            {
                _gtfsStops.Add(stop.stop_id, stop);
                var simpleStop = new SimpleStop();
                var existingStop =
                    _stops.FirstOrDefault(existingStop => existingStop.Name.Equals(stop.stop_name, StringComparison.InvariantCultureIgnoreCase) || existingStop.Ids[Supplier].Contains(stop.parent_station, StringComparer.InvariantCultureIgnoreCase));
                if (existingStop != null)
                {
                    if (!existingStop.Ids[Supplier].Contains(stop.stop_id, StringComparer.InvariantCultureIgnoreCase))
                    {
                        existingStop.Ids[Supplier].Add(stop.stop_id);
                    }
                    if (!string.IsNullOrWhiteSpace(stop.stop_code))
                    {
                        existingStop.Codes[Supplier].Add(stop.stop_code);
                    }
                }
                else
                {
                    simpleStop.Name = string.Intern(stop.stop_name);
                    simpleStop.Ids.Add(Supplier, new List<string>() { stop.stop_id });
                    simpleStop.Codes.Add(Supplier, new List<string>());
                    if (!string.IsNullOrWhiteSpace(stop.stop_code))
                    {
                        simpleStop.Codes[Supplier].Add(stop.stop_code);
                    }
                    if (!string.IsNullOrWhiteSpace(stop.parent_station) && !stop.parent_station.Equals(stop.stop_id, StringComparison.InvariantCultureIgnoreCase))
                    {
                        simpleStop.Ids[Supplier].Add(stop.parent_station);
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

        public Task<IEnumerable<SimpleDisruption>> GetDisruptionsAsync(string from, string to)
        {
            return Task.FromResult(Enumerable.Empty<SimpleDisruption>());
        }

        public Task<IEnumerable<SimpleDisruption>> GetAllDisruptionsAsync(bool active)
        {
            return Task.FromResult(Enumerable.Empty<SimpleDisruption>());
        }

        public Task<IEnumerable<SimpleStop>> GetAllStopsAsync()
        {
            return Task.FromResult(_stops.AsEnumerable());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements", Justification = "TODO")]
        public async Task<IEnumerable<SimpleTravelAdvice>> GetTravelAdviceAsync(string from, string to)
        {
            using var dbConnection = new Npgsql.NpgsqlConnection(_connectionString); // Use the appropriate connection type

            var simpleFromStop = _stops.FirstOrDefault(stop => stop.Name.Equals(from, StringComparison.InvariantCultureIgnoreCase));
            var simpleToStop = _stops.FirstOrDefault(stop => stop.Name.Equals(to, StringComparison.InvariantCultureIgnoreCase));
            var searchDate = new DateTime(2024, 03, 08);

            if (simpleFromStop != null && simpleToStop != null)
            {
                _logger.LogInformation("Selected from: {ids}", simpleFromStop.Ids[Supplier]);
                _logger.LogInformation("Selected to: {ids}", simpleToStop.Ids[Supplier]);


                // Retrieve trip times between stops
                var tripTimes = await dbConnection.QueryAsync<TripTimeInfo>(
                    @"SELECT
	                    1 as level,
	                    startStopTimes.trip_id, 
	                    trip.route_id, 
	                    trip.trip_headsign, 
	                    route.route_type, 
	                    route.route_short_name, 
	                    route.route_long_name, 
	                    agency.agency_name,
	                    startStopTimes.trip_id AS trip_id_start,
	                    targetStop.platform_code AS platform_code_end,
	                    startStop.stop_name AS stop_name_start,
	                    startStopTimes.departure_time AS departure_time_start,
	                    targetStopTimes.trip_id AS trip_id_end,
	                    startStop.platform_code AS platform_code_start,
	                    targetStop.stop_name AS stop_name_end,
	                    targetStopTimes.arrival_time AS arrival_time_end
                    FROM
	                    stop_times startStopTimes
                        JOIN trips trip ON startStopTimes.trip_id = trip.trip_id
                        JOIN routes route ON trip.route_id = route.route_id
                        JOIN agency agency ON agency.agency_id = route.agency_id
                        JOIN stop_times targetStopTimes ON startStopTimes.trip_id = targetStopTimes.trip_id
                        JOIN stops startStop ON startStopTimes.stop_id = startStop.stop_id
                        JOIN stops targetStop ON targetStopTimes.stop_id = targetStop.stop_id
                        JOIN calendar_dates dates ON trip.service_id = dates.service_id
                    WHERE
	                    startStopTimes.stop_id = ANY(@StartStopIds) AND targetStopTimes.stop_id = ANY(@EndStopIds) AND startStopTimes.departure_time < targetStopTimes.arrival_time
                        and dates.date = @date",
                    new
                    {
                        StartStopIds = simpleFromStop.Ids[Supplier],
                        EndStopIds = simpleToStop.Ids[Supplier],
                        date = searchDate.Date
                    },
                    commandType: CommandType.Text
                );


                _logger.LogInformation("Found {routes} paths", tripTimes.Count());
                var data = new List<SimpleTravelAdvice>();


                foreach (var tripTime in tripTimes)
                {
                    var item = new SimpleTravelAdvice()
                    {
                        Source = Supplier,
                        ActualDurationInMinutes = 999,
                        PlannedDurationInMinutes = 999,
                        Route = new List<SimpleRoutePart>()
                    };
                    var part = new SimpleRoutePart();
                    part.LineName = tripTime.route_short_name;
                    part.Direction = tripTime.trip_headsign;
                    part.PlannedDepartureTrack = tripTime.platform_code_start?.ToString();
                    part.PlannedArrivalTrack = tripTime.platform_code_end?.ToString();
                    part.PlannedArrival = new DateTime(DateOnly.FromDateTime(DateTime.UtcNow),
                        new TimeOnly(tripTime.arrival_time_end.Hours, tripTime.arrival_time_end.Minutes,
                            tripTime.arrival_time_end.Seconds));
                    part.PlannedDeparture = new DateTime(DateOnly.FromDateTime(DateTime.UtcNow),
                        new TimeOnly(tripTime.departure_time_start.Hours, tripTime.departure_time_start.Minutes,
                            tripTime.departure_time_start.Seconds));
                    part.DepartureStation = tripTime.stop_name_start;
                    part.ArrivalStation = tripTime.stop_name_end;
                    part.Operator = tripTime.agency_name;
                    part.RealisticTransfer = true;
                    part.AlternativeTransport = false;
                    part.Type = tripTime.route_type?.ToLegType() ?? LegType.Unknown;
                    item.Route.Add(part);

                    var time = (item.Route.Last().PlannedArrival - item.Route.First().PlannedDeparture);
                    item.PlannedDurationInMinutes = time?.TotalMinutes ?? 0;
                    item.ActualDurationInMinutes = null;
                    data.Add(item);
                }

                return data;
            }
            else
            {
                _logger.LogInformation("A unrecognized stop was present {fromStop} - {toStop}", from, to);
                return Enumerable.Empty<SimpleTravelAdvice>();
            }
        }
    }
}

public class GTFSStop
{
    public string stop_id { get; set; }
    public string stop_code { get; set; }
    public string stop_name { get; set; }
    public string parent_station { get; set; }
}
