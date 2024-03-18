using komikaan.Enums;
using komikaan.Interfaces;
using komikaan.Models;
using System.Diagnostics;

namespace komikaan.Handlers
{
    public class TravelAdviceHandler : ITravelAdviceHandler
    {
        private readonly ILogger<TravelAdviceHandler> _logger;
        private readonly IEnumerable<IDataSupplierContext> _dataSuppliers;

        public TravelAdviceHandler(ILogger<TravelAdviceHandler> logger, IEnumerable<IDataSupplierContext> dataSuppliers)
        {
            _logger = logger;
            _dataSuppliers = dataSuppliers;
        }

        public async Task<JourneyResult> GetTravelExpectationAsync(string fromStop, string toStop)
        {
            _logger.LogInformation("Calculating trip from {from} > {to}", fromStop, toStop);
            var (travelAdvice, disruptions) = await GenerateTravelDataAsync(fromStop, toStop);

            travelAdvice = travelAdvice.OrderBy(advice => advice.Route.First().PlannedDeparture.ToUniversalTime()).ToList();
            var journeyResult = GenerateJourneyResult(disruptions, travelAdvice);
            _logger.LogInformation("Calculated trip from {from} > {to}", fromStop, toStop);

            return journeyResult;
        }

        private async Task<(List<SimpleTravelAdvice> travelAdvice, List<SimpleDisruption> disruptions)> GenerateTravelDataAsync(string fromStop, string toStop)
        {
            var travelAdvice = new List<SimpleTravelAdvice>();
            var disruptions = new List<SimpleDisruption>();

            foreach (var supplier in _dataSuppliers)
            {
                var stopwatch = Stopwatch.StartNew();
                disruptions.AddRange(await supplier.GetDisruptionsAsync(fromStop, toStop));
                travelAdvice.AddRange(await supplier.GetTravelAdviceAsync(fromStop, toStop));
                _logger.LogInformation("Calculated by {name} in {time}", supplier.Supplier, stopwatch.Elapsed.ToString("g"));
            }

            return (travelAdvice, disruptions);
        }

        private static JourneyResult GenerateJourneyResult(List<SimpleDisruption> disruptions, List<SimpleTravelAdvice> travelAdvice)
        {
            var journeyResult = new JourneyResult
            {
                Disruptions = disruptions.ToList(),
                TravelAdvice = travelAdvice.ToList(),
            };

            CalculateExpectation(journeyResult);
            return journeyResult;
        }

        private static void CalculateExpectation(JourneyResult journeyResult)
        {
            //TODO: Expand this logic and make it not this
            if (!journeyResult.TravelAdvice.Any())
            {
                journeyResult.JourneyExpectation = JourneyExpectation.Unknown;
            }
            else if (journeyResult.TravelAdvice.All(advice => advice.Route.Any(route => route.Cancelled)) && journeyResult.Disruptions.Any(disruption => disruption.Type != DisruptionType.Maintenance && disruption.ExpectedEnd?.ToUniversalTime() > DateTime.UtcNow.AddMinutes(15)))
            {
                journeyResult.JourneyExpectation = JourneyExpectation.Nope;
            }
            else if (journeyResult.TravelAdvice.Any(advice => advice.Route.Any(route => route.Cancelled)))
            {
                journeyResult.JourneyExpectation = JourneyExpectation.Maybe;
            }
            else
            {
                journeyResult.JourneyExpectation = JourneyExpectation.Full;
            }
        }
    }
}
