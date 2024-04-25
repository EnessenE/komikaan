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
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            foreach (var supplier in _dataSuppliers)
            {
                disruptions.AddRange(await GetSupplierDisruptionsAsync(fromStop, toStop, supplier, cancellationTokenSource.Token));
                travelAdvice.AddRange(await GetSupplierTravelAdviceAsync(fromStop, toStop, supplier, cancellationTokenSource.Token));

                _logger.LogInformation("Calculated by {name}", supplier.Supplier);
            }
            return (travelAdvice, disruptions);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1561:Signature contains too many parameters", Justification = "CTS passing")]
        private async Task<IEnumerable<SimpleTravelAdvice>> GetSupplierTravelAdviceAsync(string fromStop, string toStop, IDataSupplierContext supplier, CancellationToken token)
        {
            using var activity = Telemetry.Activity.StartActivity("Travel advice calculation");
            activity?.SetTag("supplier", supplier.Supplier);
            return await supplier.GetTravelAdviceAsync(fromStop, toStop, token);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1561:Signature contains too many parameters", Justification = "CTS passing")]

        private async Task<IEnumerable<SimpleDisruption>> GetSupplierDisruptionsAsync(string fromStop, string toStop, IDataSupplierContext supplier, CancellationToken token)
        {
            using var activity = Telemetry.Activity.StartActivity("Disruption retrieval");
            activity?.SetTag("supplier", supplier.Supplier);
            var data = await supplier.GetDisruptionsAsync(fromStop, toStop, token);
            return data;
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
            else if (journeyResult.TravelAdvice.Count(advice => advice.Route.Any(route => route.Cancelled)) >= 2 || journeyResult.Disruptions.Any(disruption => disruption.Type == DisruptionType.Disruption))
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
