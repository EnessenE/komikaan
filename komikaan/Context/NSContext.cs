using komikaan.Data.Enums;
using komikaan.Enums;
using komikaan.Interfaces;
using komikaan.Models;
using komikaan.Models.API.NS;

namespace komikaan.Context
{
    public class NSContext : IDataSupplierContext
    {
        private readonly INSApi _nsApi;
        private readonly ILogger<NSContext> _logger;

        private List<Disruption> _allDisruptions;
        private IDictionary<string, Station> _allStations;

        public NSContext(INSApi nsApi, ILogger<NSContext> logger)
        {
            _nsApi = nsApi;
            _logger = logger;
        }

        public async Task LoadRelevantData(CancellationToken cancellationToken)
        {
            await GetAllData();
        }

        private async Task GetAllData()
        {
            _allStations = await LoadAllStops();
            _allDisruptions = await LoadAllDisruptions();
        }

        private async Task<List<Disruption>> LoadAllDisruptions()
        {
            return await _nsApi.GetAllDisruptions();
        }

        private async Task<Dictionary<string, Station>> LoadAllStops()
        {
            var stationRoot = await _nsApi.GetAllStations();
            var dict = new Dictionary<string, Station>();
            foreach (var station in stationRoot.payload)
            {
                dict.Add(station.namen.lang, station);
            }

            return dict;
        }

        public async Task<IEnumerable<SimpleDisruption>> GetDisruptions(string from, string to)
        {
            var fromStop = _allStations[from];
            var toStop = _allStations[to];

            var allDisruptions = _allDisruptions;

            var relevantFromDisruptions = allDisruptions.Where(disruption =>
                disruption.publicationSections.Any(publicationSection => publicationSection.section.stations.Any(station => station.name.Equals(fromStop.namen.lang, StringComparison.InvariantCultureIgnoreCase)))).ToList();
            var relevantToDisruptions = allDisruptions.Where(disruption =>
                disruption.publicationSections.Any(publicationSection => publicationSection.section.stations.Any(station => station.name.Equals(toStop.namen.lang, StringComparison.InvariantCultureIgnoreCase)))).ToList();

            var disruptions = relevantToDisruptions;
            disruptions.AddRange(relevantFromDisruptions);
            disruptions = disruptions.FindAll(disruption => disruption.isActive).ToList();

            var data = new List<SimpleDisruption>();
            foreach (var disruption in disruptions)
            {
                GenerateDisruption(disruption, data);
            }

            return data;
        }

        private void GenerateDisruption(Disruption disruption, List<SimpleDisruption> data)
        {
            _logger.LogInformation("Relevant: {name}, active {act}", disruption.title, disruption.isActive);
            var simpleDisruption = new SimpleDisruption();
            simpleDisruption.Title = disruption.title;
            var simpleText = disruption.timespans.Select(x => x.situation);
            simpleDisruption.Descriptions = simpleText.Select(situation => situation.label);
            var advices = disruption.timespans.Select(x => x.advices);

            simpleDisruption.Advices = advices.SelectMany(advice => advice);
            simpleDisruption.ExpectedEnd = disruption.end;


            if (!Enum.TryParse(disruption.type, true, out DisruptionType disruptionType))
            {
                //Incase new disruption types are introduces
                disruptionType = DisruptionType.Unknown;
            }

            simpleDisruption.Source = DataSource.NederlandseSpoorwegen;

            simpleDisruption.Type = disruptionType;
            data.Add(simpleDisruption);
        }


        public async Task<IDictionary<string, Station>> GetAllStops()
        {
            return _allStations;
        }

        public async Task<IEnumerable<SimpleTravelAdvice>> GetTravelAdvice(string from, string to)
        {
            var fromStop = _allStations[from];
            var toStop = _allStations[to];

            var travelAdvice = await _nsApi.GetRouteAdvice(fromStop.code, toStop.code);
            var simplifiedTravelAdvices = new List<SimpleTravelAdvice>();
            foreach (var trip in travelAdvice.trips)
            {
                GenerateTravelAdvice(from, trip, simplifiedTravelAdvices);
            }
            return simplifiedTravelAdvices;
        }

        private static void GenerateTravelAdvice(string from, Trip trip, List<SimpleTravelAdvice> simplifiedTravelAdvices)
        {
            var simpleTravelAdvice = new SimpleTravelAdvice();
            simpleTravelAdvice.Source = DataSource.NederlandseSpoorwegen;
            simpleTravelAdvice.PlannedDurationInMinutes = trip.plannedDurationInMinutes;
            simpleTravelAdvice.ActualDurationInMinutes = trip.actualDurationInMinutes;
            simpleTravelAdvice.Route = new List<SimpleRoutePart>();


            simpleTravelAdvice.Route.Add(new SimpleRoutePart()
            {
                Cancelled = false,
                Name = from,
                RealisticTransfer = true,
                PlannedArrival = null,
                ActualArrival = null
            });
            var previous = simpleTravelAdvice.Route.First();
            Leg previousLeg = null;
            foreach (var leg in trip.legs)
            {
                var routePart = new SimpleRoutePart();

                routePart.Name = leg.destination.name;

                routePart.Cancelled = leg.partCancelled || leg.cancelled;
                routePart.RealisticTransfer = leg.changePossible;
                routePart.AlternativeTransport = leg.alternativeTransport;

                previous.PlannedDeparture = leg.origin.plannedDateTime;
                previous.ActualDeparture = leg.origin.actualDateTime;
                if (previousLeg != null)
                {
                    previous.PlannedArrival = previousLeg.destination.plannedDateTime;
                    previous.ActualArrival = previousLeg.destination.actualDateTime;
                }

                previous = routePart;
                previousLeg = leg;
                simpleTravelAdvice.Route.Add(routePart);
            }

            if (previousLeg != null)
            {
                previous.PlannedArrival = previousLeg.destination.plannedDateTime;
                previous.ActualArrival = previousLeg.destination.actualDateTime;
            }


            simplifiedTravelAdvices.Add(simpleTravelAdvice);
        }
    }
}
