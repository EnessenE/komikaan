using System.Data;
using System.Data.Common;
using Dapper;
using GTFS;
using GTFS.Entities;
using GTFS.Entities.Enumerations;
using komikaan.Data.Enums;
using komikaan.Data.Models;
using komikaan.Interfaces;
using komikaan.Models;
using Npgsql;
using NpgsqlTypes;
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
                    _stops.FirstOrDefault(existingStop => existingStop.Name.Equals(stop.Name, StringComparison.InvariantCultureIgnoreCase));
                if (existingStop != null)
                {
                    existingStop.Ids[Supplier].Add(stop.Id);
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

            _logger.LogInformation("Selected from: {ids}", simpleFromStop.Ids[Supplier]);
            _logger.LogInformation("Selected to: {ids}", simpleToStop.Ids[Supplier]);

            var tripIds = await dbConnection.QueryAsync<string>(
                @"SELECT DISTINCT t.trip_id, t.route_id, t.trip_id
              FROM stop_times st
              JOIN trips t ON st.trip_id = t.trip_id
              WHERE st.stop_id IN (@StopId1, @StopId2)
              ORDER BY t.route_id, t.trip_id",
                new { StopId1 = simpleFromStop.Ids[Supplier].First(), StopId2 = simpleToStop.Ids[Supplier].First() }
            );

            if (!tripIds.Any())
            {
               _logger.LogInformation("No trips found between the specified stops.");
            }
            else
            {
                _logger.LogInformation("Found {trips}", tripIds.Count());
            }

            // Retrieve trip times between stops
            var tripTimes = await dbConnection.QueryAsync<TripTimeInfo>(
                @"SELECT st.trip_id, t.route_id, t.trip_headsign, r.route_type, r.route_short_name, r.route_long_name, st.stop_sequence, s.stop_name, st.arrival_time, st.departure_time, agency.agency_name
              FROM stop_times st
              JOIN trips t ON st.trip_id = t.trip_id
              JOIN routes r ON t.route_id = r.route_id
              JOIN agency agency ON agency.agency_id = r.agency_id
              JOIN stops s ON st.stop_id = s.stop_id
              WHERE st.stop_id = ANY(@StopIds) AND st.trip_id = ANY(@TripIds)
              ORDER BY st.trip_id, st.stop_sequence",
                new { StopIds = simpleToStop.Ids[Supplier], TripIds = tripIds },
                commandType: CommandType.Text
            );

            
            _logger.LogInformation("Found {routes} paths", tripTimes.Count());
            var data = new List<SimpleTravelAdvice>();

         
            foreach (var tripTime in tripTimes)
            {
                data.Add(new SimpleTravelAdvice()
                {
                    Source = Supplier,
                    ActualDurationInMinutes = 999,
                    PlannedDurationInMinutes = 999,
                    Route = new List<SimpleRoutePart>()
                    {
                        new SimpleRoutePart()
                        {
                            LineName = tripTime. route_short_name, 
                            Direction= tripTime.trip_headsign,
                            PlannedArrivalTrack = tripTime.stop_sequence?.ToString(),
                            PlannedDeparture = DateTime.UtcNow,
                            PlannedArrival = new DateTime(DateOnly.FromDateTime(DateTime.UtcNow), new TimeOnly(tripTime.arrival_time.Hours, tripTime.arrival_time.Minutes, tripTime.arrival_time.Seconds)),
                            DepartureStation = simpleFromStop.Name,
                            ArrivalStation = tripTime.stop_name,
                            Operator = tripTime.agency_name,
                            RealisticTransfer = true,
                        }
                    }
                });
            }

            return data;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements", Justification = "TODO")]
        private Task<IEnumerable<SimpleTravelAdvice>> OldAttempt(string from, string to)
        {
            var simpleFromStop = _stops.First(stop => stop.Name.Equals(from, StringComparison.InvariantCultureIgnoreCase));
            var simpleToStop = _stops.First(stop => stop.Name.Equals(to, StringComparison.InvariantCultureIgnoreCase));
            _logger.LogInformation("Found a stop");
            var stopTimes = new List<StopTime>();

            foreach (var id in simpleFromStop.Ids[Supplier])
            {
                stopTimes.AddRange(_feed.StopTimes.Where(stopTime =>
                    stopTime.StopId.Equals(id, StringComparison.InvariantCultureIgnoreCase)));
            }

            _logger.LogInformation("Found {count} stopTimes", stopTimes.Count());

            stopTimes = stopTimes.Where(stopTime =>
                stopTime.DepartureTime >= TimeOfDay.FromDateTime(DateTime.UtcNow.AddMinutes(-1)) &&
                stopTime.DepartureTime <= TimeOfDay.FromDateTime(DateTime.UtcNow.AddMinutes(35))).ToList();

            _logger.LogInformation("Found filtered {count} stopTimes", stopTimes.Count());

            var advices = new List<SimpleTravelAdvice>();
            foreach (var stopTime in stopTimes)
            {
                _logger.LogInformation("Processing a stoptime");
                var trip = _trips[stopTime.TripId];
                var calender = DateTime.UtcNow.CreateCalendar(trip.ServiceId);
                //Covers date doesn't check start/end date.
                if (calender.StartDate.ToUniversalTime() >= DateTime.UtcNow &&
                    calender.EndDate.ToUniversalTime() <= DateTime.UtcNow && calender.CoversDate(DateTime.UtcNow))
                {
                    var simpleTravelAdvice = GenerateTravelAdvice(stopTime, trip);

                    advices.Add(simpleTravelAdvice);
                }
                else
                {
                    _logger.LogInformation("{trip}, is not covered today", trip);
                }
            }

            return Task.FromResult(advices.AsEnumerable());
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

public class TripTimeInfo
{
    public string? trip_headsign { get; set; }
    public string trip_id { get; set; }
    public string route_id { get; set; }
    public int? stop_sequence { get; set; }
    public string stop_name { get; set; }
    public string agency_name { get; set; }
    public string route_long_name { get; set; }
    public string route_short_name { get; set; }
    public RouteType route_type { get; set; }
    public TimeSpan arrival_time { get; set; }
    public TimeSpan departure_time { get; set; }
}
