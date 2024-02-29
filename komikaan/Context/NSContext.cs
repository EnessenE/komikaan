﻿using komikaan.Data.Enums;
using komikaan.Enums;
using komikaan.Interfaces;
using komikaan.Models;
using komikaan.Models.API.NS;
using Serilog;

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

            var relevantDisruptions = allDisruptions.Where(disruption =>
                disruption.publicationSections.Any(publicationSection => publicationSection.section.stations.Any(station => station.name.Equals(toStop.namen.lang, StringComparison.InvariantCultureIgnoreCase) || station.name.Equals(toStop.namen.lang)))).ToList();

            relevantDisruptions = relevantDisruptions.FindAll(disruption => disruption.isActive).ToList();

            var data = new List<SimpleDisruption>();
            foreach (var disruption in relevantDisruptions)
            {
                GenerateSimplifiedDisruption(disruption, data);
            }

            return data;
        }

        private void GenerateSimplifiedDisruption(Disruption disruption, List<SimpleDisruption> data)
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


        public Task<IDictionary<string, Station>> GetAllStops()
        {
            return Task.FromResult(_allStations);
        }

        public async Task<IEnumerable<SimpleTravelAdvice>> GetTravelAdvice(string from, string to)
        {
            var fromStop = _allStations[from];
            var toStop = _allStations[to];

            var travelAdvice = await _nsApi.GetRouteAdvice(fromStop.code, toStop.code);
            var simplifiedTravelAdvices = new List<SimpleTravelAdvice>();
            foreach (var trip in travelAdvice.trips)
            {
                simplifiedTravelAdvices.Add(GenerateTravelAdvice(from, trip));
            }
            return simplifiedTravelAdvices;
        }

        private SimpleTravelAdvice GenerateTravelAdvice(string from, Trip trip)
        {
            var simpleTravelAdvice = new SimpleTravelAdvice();
            simpleTravelAdvice.Source = DataSource.NederlandseSpoorwegen;
            simpleTravelAdvice.PlannedDurationInMinutes = trip.plannedDurationInMinutes;
            simpleTravelAdvice.ActualDurationInMinutes = trip.actualDurationInMinutes;
            simpleTravelAdvice.Route = new List<SimpleRoutePart>();

            //Add the first station manually
            simpleTravelAdvice.Route.Add(new SimpleRoutePart()
            {
                Cancelled = false,
                Name = from,
                RealisticTransfer = true,
                PlannedArrival = null,
                ActualArrival = null
            });

            // We see these objects different then the NS. We look at it per station as thats in our interest
            // While the NS looks at it per leg, so sometimes we have to look at the previous item to look forward
            var previous = simpleTravelAdvice.Route.First();
            Leg previousLeg = null;

            foreach (var leg in trip.legs)
            {
                var routePart = new SimpleRoutePart();

                routePart.Name = leg.destination.name;

                routePart.Cancelled = leg.partCancelled || leg.cancelled;
                routePart.AlternativeTransport = leg.alternativeTransport;

                previous.PlannedDeparture = leg.origin.plannedDateTime;
                previous.ActualDeparture = leg.origin.actualDateTime;

                previous.PlannedDepartureTrack = leg.origin.plannedTrack;
                previous.ActualDeperatureTrack = leg.origin.actualTrack;

                if (previousLeg != null)
                {
                    previous.PlannedArrival = previousLeg.destination.plannedDateTime;
                    previous.ActualArrival = previousLeg.destination.actualDateTime;

                    previous.PlannedArrivalTrack = previousLeg.destination.plannedTrack;
                    previous.ActualArrivalTrack = previousLeg.destination.actualTrack;

                    previous.RealisticTransfer = previousLeg.changePossible;
                }

                previous = routePart;
                previousLeg = leg;
                simpleTravelAdvice.Route.Add(routePart);
            }

            // To standardize data to our format
            if (previousLeg != null)
            {
                previous.PlannedArrival = previousLeg.destination.plannedDateTime;
                previous.ActualArrival = previousLeg.destination.actualDateTime;
                previous.RealisticTransfer = true;
            }


            return simpleTravelAdvice;
        }
    }
}
