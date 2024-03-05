using komikaan.Data.Configuration;
using komikaan.Data.Enums;
using komikaan.Enums;
using komikaan.Interfaces;
using komikaan.Models;
using komikaan.Models.API.NS;
using Microsoft.Extensions.Options;
using Refit;

namespace komikaan.Context
{
    public class NSContext : IDataSupplierContext
    {
        private readonly INSApi _nsApi;
        private readonly ILogger<NSContext> _logger;

        private List<SimpleDisruption> _allDisruptions;
        private IDictionary<string, Station> _allStations;
        private readonly Dictionary<string, LegType> _mappings;

        public DataSource Supplier { get; } = DataSource.NederlandseSpoorwegen;

        public NSContext(INSApi nsApi, ILogger<NSContext> logger, IOptions<SupplierConfigurations> supplierMappingConfiguration)
        {
            _nsApi = nsApi;
            _logger = logger;
            _mappings = supplierMappingConfiguration.Value.Mappings[Supplier];
            _allDisruptions = new List<SimpleDisruption>();
            _allStations = new Dictionary<string, Station>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await GetAllDataAsync();
        }

        public async Task LoadRelevantDataAsync(CancellationToken cancellationToken)
        {
            await GetAllDataAsync();
        }

        public Task<IEnumerable<SimpleDisruption>> GetDisruptionsAsync(string from, string to)
        {
            var fromStop = _allStations[from];
            var toStop = _allStations[to];

            var relevantDisruptions = _allDisruptions.Where(disruption => disruption.AffectedStops.Any(stop => stop.Equals(fromStop.code, StringComparison.InvariantCultureIgnoreCase) || stop.Equals(toStop.code, StringComparison.InvariantCultureIgnoreCase)));
            relevantDisruptions = relevantDisruptions.ToList().FindAll(disruption => disruption.Active);

            return Task.FromResult(relevantDisruptions);
        }

        public Task<IEnumerable<SimpleDisruption>> GetAllDisruptions(bool active)
        {
            var disruptions = _allDisruptions.Where(disruption => disruption.Active = active);
            return Task.FromResult(disruptions);
        }

        public Task<IDictionary<string, Station>> GetAllStops()
        {
            return Task.FromResult(_allStations);
        }

        public async Task<IEnumerable<SimpleTravelAdvice>> GetTravelAdviceAsync(string from, string to)
        {
            var fromStop = _allStations[from];
            var toStop = _allStations[to];

            var travelAdvice = await _nsApi.GetRouteAdvice(fromStop.code, toStop.code);
            var simplifiedTravelAdvices = new List<SimpleTravelAdvice>();
            foreach (var trip in travelAdvice.trips)
            {
                var simplifiedTravelAdvice = GenerateSimplifiedTravelAdvice(from, trip);
                simplifiedTravelAdvices.Add(simplifiedTravelAdvice);
            }
            return simplifiedTravelAdvices;
        }

        private SimpleTravelAdvice GenerateSimplifiedTravelAdvice(string from, Trip trip)
        {
            var simpleTravelAdvice = new SimpleTravelAdvice();
            simpleTravelAdvice.Source = DataSource.NederlandseSpoorwegen;
            simpleTravelAdvice.PlannedDurationInMinutes = trip.plannedDurationInMinutes;
            simpleTravelAdvice.ActualDurationInMinutes = trip.actualDurationInMinutes;
            simpleTravelAdvice.Route = new List<SimpleRoutePart>();

            CalculateLegs(trip, simpleTravelAdvice);

            return simpleTravelAdvice;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements", Justification = "Collection of leg data, splitting this causes arguably more confusion in the current form")]
        private void CalculateLegs(Trip trip, SimpleTravelAdvice simpleTravelAdvice)
        {

            foreach (var leg in trip.legs)
            {
                var routePart = new SimpleRoutePart();

                routePart.ArrivalStation = leg.destination.name;
                routePart.DepartureStation = leg.origin.name;

                if (_mappings.TryGetValue(leg.product.shortCategoryName, out var foundType))
                {
                    routePart.Type = foundType;
                }
                else
                {
                    routePart.Type = LegType.Unknown;
                }

                routePart.Cancelled = leg.partCancelled || leg.cancelled;
                routePart.AlternativeTransport = leg.alternativeTransport;

                routePart.PlannedDeparture = leg.origin.plannedDateTime;
                routePart.ActualDeparture = leg.origin.actualDateTime;

                routePart.PlannedDepartureTrack = leg.origin.plannedTrack;
                routePart.ActualDepartureTrack = leg.origin.actualTrack;

                routePart.PlannedArrival = leg.destination.plannedDateTime;
                routePart.ActualArrival = leg.destination.actualDateTime;

                routePart.PlannedArrivalTrack = leg.destination.plannedTrack;
                routePart.ActualArrivalTrack = leg.destination.actualTrack;

                routePart.RealisticTransfer = leg.changePossible;

                simpleTravelAdvice.Route.Add(routePart);
            }
        }

        private async Task GetAllDataAsync()
        {
            try
            {
                _allStations = await LoadAllStopsAsync();
                _allDisruptions = await LoadAllDisruptionsAsync();
            }
            catch (ApiException apiException)
            {
                // A backoff should be implemented, for example Polly
                _logger.LogError(apiException, "Failed to reload information");
            }
        }

        private async Task<List<SimpleDisruption>> LoadAllDisruptionsAsync()
        {
            var rawDisruptions = await _nsApi.GetAllDisruptions();

            var data = new List<SimpleDisruption>();
            foreach (var disruption in rawDisruptions)
            {
                var simpleDisruption = GenerateSimplifiedDisruption(disruption);
                data.Add(simpleDisruption);
            }

            return data;
        }

        private async Task<Dictionary<string, Station>> LoadAllStopsAsync()
        {
            var stationRoot = await _nsApi.GetAllStations();
            var dict = new Dictionary<string, Station>();
            foreach (var station in stationRoot.payload)
            {
                dict.Add(station.namen.lang, station);
            }

            return dict;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements", Justification = "Creation of simplified disruptions, splitting this causes arguably more confusion in the current form")]
        private SimpleDisruption GenerateSimplifiedDisruption(Disruption disruption)
        {
            _logger.LogInformation("Loading: {name}, active {act}", disruption.title, disruption.isActive);

            var advices = disruption.timespans?.Select(timespan => timespan.advices);
            var affectedStops = disruption.publicationSections?.SelectMany(section => section.section.stations.Select(disruptionStation => disruptionStation.stationCode)).ToList();
            var situation = disruption.timespans?.Select(timespan => timespan.situation);
            var description = new List<string>();
            if (!Enum.TryParse(disruption.type, true, out DisruptionType disruptionType))
            {
                //Incase new disruption types are introduced
                disruptionType = DisruptionType.Unknown;
            }

            if (!string.IsNullOrWhiteSpace(disruption.description))
            {
                description.Add(disruption.description);
            }

            if (situation != null)
            {
                description.AddRange(situation.Select(situation => situation.label));
            }


            var simpleDisruption = new SimpleDisruption();
            simpleDisruption.Title = disruption.title;
            simpleDisruption.Descriptions = description;
            simpleDisruption.Advices = advices?.SelectMany(advice => advice) ?? Enumerable.Empty<string>();
            simpleDisruption.ExpectedEnd = disruption.end;
            simpleDisruption.Start = disruption.start;
            simpleDisruption.Active = disruption.isActive;
            simpleDisruption.Url = disruption.url;
            // If stops are missing, assume it applies to every station
            // While bad, we miss data to properly argue where it should apply.
            simpleDisruption.AffectedStops = affectedStops ?? _allStations.Values.Select(station => station.code).ToList();
            simpleDisruption.Source = Supplier;
            simpleDisruption.Type = disruptionType;

            return simpleDisruption;
        }

    }
}
