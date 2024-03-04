using GTFS;
using GTFS.Entities;
using GTFS.IO;
using komikaan.Data.Enums;
using komikaan.Data.Models;
using komikaan.Interfaces;
using komikaan.Models;
using komikaan.Models.API.NS;
using Trip = GTFS.Entities.Trip;

namespace komikaan.Context
{
    public class OpenOVContext : IDataSupplierContext
    {
        private ILogger<OpenOVContext> _logger;
        private GTFSFeed _feed;

        private IList<SimpleStop> _stops;
        private IDictionary<string, Trip> _trips;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
        public OpenOVContext(ILogger<OpenOVContext> logger)
        {
            _logger = logger;
            _stops = new List<SimpleStop>();
            _trips = new Dictionary<string, Trip>();
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
            _logger.LogInformation("Finished generating stops");

            GenerateTrips();
            _logger.LogInformation("Finished generating trips");

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
            var fromStop = _feed.Stops.Get(simpleFromStop.Ids[Supplier].First());

            _logger.LogInformation("Found a stop");

            var stopTimes = _feed.StopTimes.Where(stopTime => stopTime.StopId.Equals(fromStop.Id, StringComparison.InvariantCultureIgnoreCase));
            _logger.LogInformation("Found {count} stopTimes", stopTimes.Count());

            stopTimes = stopTimes.Where(stopTime => stopTime.DepartureTime >= TimeOfDay.FromDateTime(DateTime.UtcNow.AddMinutes(-1)) || stopTime.DepartureTime <= TimeOfDay.FromDateTime(DateTime.UtcNow.AddMinutes(35)));
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
            var simpleTravelAdvice = new SimpleTravelAdvice();
            simpleTravelAdvice.ActualDurationInMinutes = 999;
            simpleTravelAdvice.PlannedDurationInMinutes = 999;
            simpleTravelAdvice.Source = Supplier;
            simpleTravelAdvice.Route = new List<SimpleRoutePart>();
            var routePart = new SimpleRoutePart();
            routePart.Type = LegType.Unknown;
            routePart.ArrivalStation = trip.Headsign;
            routePart.PlannedDeparture = ToDateTime(stopTime.DepartureTime!.Value);
            routePart.PlannedArrival = ToDateTime(stopTime.ArrivalTime!.Value);
            routePart.PlannedArrivalTrack = trip.ShortName;
            routePart.PlannedDepartureTrack = stopTime.ToString();
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
