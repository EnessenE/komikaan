using System.Data;
using Dapper;
using GTFS;
using GTFS.Entities;
using komikaan.Data.Enums;
using komikaan.Data.Models;
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
        private ILogger<OpenOVContext> _logger;
        private GTFSFeed _feed;

        private IList<SimpleStop> _stops;
        private IDictionary<string, Trip> _trips;
        private IDictionary<string, Stop> _gtfsStops;

        private string _connectionString;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
        public OpenOVContext(ILogger<OpenOVContext> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("gtfs") ?? throw new InvalidOperationException("A GTFS postgres database connection should be defined!");
            _stops = new List<SimpleStop>();
            _trips = new Dictionary<string, Trip>();
            _gtfsStops = new Dictionary<string, Stop>();
        }

        public DataSource Supplier { get; } = DataSource.OpenOV;
        public Task StartAsync(CancellationToken cancellationToken)
        {
            string path = "E:\\gtfs-testing\\gtfs-nl-mini.zip";

            var reader = new GTFSReader<GTFSFeed>();
            var feed = reader.Read(path);

            _feed = feed;
            _logger.LogInformation("Finished reading GTFS data");

            GenerateData();
            
            return Task.CompletedTask;
        }

        private void GenerateData()
        {
            GenerateStops();
            _logger.LogInformation("Finished generating {simpleStops}/{gtfsStops} stops", _stops.Count, _gtfsStops.Count);

            GenerateTrips();
            _logger.LogInformation("Finished generating {trips} trips", _trips.Count);
        }
        
        private void GenerateTrips()
        {
            foreach (var feedTrip in _feed.Trips)
            {
                _trips.Add(feedTrip.Id, feedTrip);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements", Justification = "TODO")]
        private void GenerateStops()
        {
            foreach (var stop in _feed.Stops)
            {
                _gtfsStops.Add(stop.Id, stop);
                var simpleStop = new SimpleStop();
                var existingStop =
                    _stops.FirstOrDefault(existingStop => existingStop.Name.Equals(stop.Name, StringComparison.InvariantCultureIgnoreCase)|| existingStop.Ids[Supplier].Contains(stop.ParentStation, StringComparer.InvariantCultureIgnoreCase));
                if (existingStop != null)
                {
                    if (!existingStop.Ids[Supplier].Contains(stop.Id, StringComparer.InvariantCultureIgnoreCase))
                    {
                        existingStop.Ids[Supplier].Add(stop.Id);
                    }
                    if (!string.IsNullOrWhiteSpace(stop.Code))
                    {
                        existingStop.Codes[Supplier].Add(stop.Code);
                    }
                }
                else
                {
                    simpleStop.Name = string.Intern(stop.Name);
                    simpleStop.Ids.Add(Supplier, new List<string>() { stop.Id });
                    simpleStop.Codes.Add(Supplier, new List<string>());
                    if (!string.IsNullOrWhiteSpace(stop.Code))
                    {
                        simpleStop.Codes[Supplier].Add(stop.Code);
                    }
                    if (!string.IsNullOrWhiteSpace(stop.ParentStation) && !stop.ParentStation.Equals(stop.Id, StringComparison.InvariantCultureIgnoreCase))
                    {
                        simpleStop.Ids[Supplier].Add(stop.ParentStation);
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

            var simpleFromStop = _stops.First(stop => stop.Name.Equals(from, StringComparison.InvariantCultureIgnoreCase));
            var simpleToStop = _stops.First(stop => stop.Name.Equals(to, StringComparison.InvariantCultureIgnoreCase));
            var searchDate = new DateTime(2024, 03, 05);

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
                new { StartStopIds = simpleFromStop.Ids[Supplier], EndStopIds = simpleToStop.Ids[Supplier], date = searchDate.Date },
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
                part.PlannedArrival = new DateTime(DateOnly.FromDateTime(DateTime.UtcNow), new TimeOnly(tripTime.arrival_time_end.Hours, tripTime.arrival_time_end.Minutes, tripTime.arrival_time_end.Seconds));
                part.PlannedDeparture = new DateTime(DateOnly.FromDateTime(DateTime.UtcNow), new TimeOnly(tripTime.departure_time_start.Hours, tripTime.departure_time_start.Minutes, tripTime.departure_time_start.Seconds));
                part.DepartureStation = tripTime.stop_name_start;
                part.ArrivalStation = tripTime.stop_name_end;
                part.Operator = tripTime.agency_name;
                part.RealisticTransfer = true;
                part.AlternativeTransport = false;
                part.Type = tripTime.route_type?.ToLegType() ?? LegType.Unknown;
                item.Route.Add(part);
                data.Add(item);
            }

            return data;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements", Justification = "TODO")]
        private SimpleTravelAdvice GenerateTravelAdvice(StopTime stopTime, Trip trip)
        {
            var stop = _gtfsStops[stopTime.StopId];
            var simpleTravelAdvice = new SimpleTravelAdvice();
            simpleTravelAdvice.ActualDurationInMinutes = 999;
            simpleTravelAdvice.PlannedDurationInMinutes = 999;
            simpleTravelAdvice.Source = Supplier;
            simpleTravelAdvice.Route = new List<SimpleRoutePart>();

            var routePart = new SimpleRoutePart();
            routePart.Type = LegType.Unknown;
            routePart.ArrivalStation = "who knows";
            routePart.PlannedDeparture = ToDateTime(stopTime.DepartureTime!.Value);
            routePart.PlannedArrival = ToDateTime(stopTime.ArrivalTime!.Value);
            routePart.PlannedArrivalTrack = "??";
            routePart.PlannedDepartureTrack = stop.PlatformCode;
            routePart.DepartureStation = stop.Name;
            routePart.RealisticTransfer = true;
            routePart.LineName = trip.ShortName;
            routePart.Direction = trip.Headsign;
            var route = _feed.Routes.First(route => route.Id.Equals(trip.RouteId));
            routePart.LineName = route.ShortName; 
            var agency = _feed.Agencies.First(agency => agency.Id == route.AgencyId);
            routePart.Operator = agency.Name;
            
            simpleTravelAdvice.Route.Add(routePart);
            
            return simpleTravelAdvice;
        }

        private DateTime ToDateTime(TimeOfDay timeOfDay)
        {
            var baseDate = DateTime.UtcNow.Date;
            baseDate = baseDate.AddHours(timeOfDay.Hours);
            baseDate= baseDate.AddMinutes(timeOfDay.Minutes);
            baseDate= baseDate.AddSeconds(timeOfDay.Seconds);
            return baseDate;
        }
    }
}
