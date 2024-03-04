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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
        public OpenOVContext(ILogger<OpenOVContext> logger)
        {
            _logger = logger;
            _stops = new List<SimpleStop>();
            _trips = new Dictionary<string, Trip>();
            _gtfsStops = new Dictionary<string, Stop>();
        }

        public DataSource Supplier { get; } = DataSource.OpenOV;
        public Task StartAsync(CancellationToken cancellationToken)
        {
            string path = "C:\\Users\\maile\\Downloads\\gtfs-nl";

            var reader = new GTFSReader<GTFSFeed>();
            var feed = reader.Read(path);

            _feed = feed;
            _logger.LogInformation("Finished reading GTFS data");

            GenerateStops();
            _logger.LogInformation("Finished generating {simpleStops}/{gtfsStops} stops", _stops.Count, _gtfsStops.Count);

            GenerateTrips();
            _logger.LogInformation("Finished generating {trips} trips", _trips.Count);

            return Task.CompletedTask;
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
        public Task<IEnumerable<SimpleTravelAdvice>> GetTravelAdviceAsync(string from, string to)
        {
            var simpleFromStop = _stops.First(stop => stop.Name.Equals(from, StringComparison.InvariantCultureIgnoreCase));
            var simpleToStop = _stops.First(stop => stop.Name.Equals(to, StringComparison.InvariantCultureIgnoreCase));
            _logger.LogInformation("Found a stop");
            var stopTimes = new List<StopTime>();

            foreach (var id in simpleFromStop.Ids[Supplier])
            {
                stopTimes.AddRange(_feed.StopTimes.Where(stopTime => stopTime.StopId.Equals(id, StringComparison.InvariantCultureIgnoreCase)));
            }
            _logger.LogInformation("Found {count} stopTimes", stopTimes.Count());

            stopTimes = stopTimes.Where(stopTime => stopTime.DepartureTime >= TimeOfDay.FromDateTime(DateTime.UtcNow.AddMinutes(-1)) && stopTime.DepartureTime <= TimeOfDay.FromDateTime(DateTime.UtcNow.AddMinutes(35))).ToList();
            _logger.LogInformation("Found filtered {count} stopTimes", stopTimes.Count());

            var advices = new List<SimpleTravelAdvice>();
            foreach (var stopTime in stopTimes)
            {
                _logger.LogInformation("Processing a stoptime");
                var trip = _trips[stopTime.TripId];

                var simpleTravelAdvice = GenerateTravelAdvice(stopTime, trip);

                advices.Add(simpleTravelAdvice);
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
