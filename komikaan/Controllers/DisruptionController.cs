using komikaan.Enums;
using komikaan.Interfaces;
using komikaan.Models;
using Microsoft.AspNetCore.Mvc;

namespace komikaan.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class DisruptionController : ControllerBase
    {
        private readonly ILogger<DisruptionController> _logger;
        private readonly IEnumerable<IDataSupplierContext> _dataSuppliers;

        public DisruptionController(ILogger<DisruptionController> logger, IEnumerable<IDataSupplierContext> dataSuppliers)
        {
            _logger = logger;
            _dataSuppliers = dataSuppliers;
        }

        [HttpGet("{fromStop}/{toStop}")]
        public async Task<JourneyResult> GetTravelExpectationAsync(string fromStop, string toStop)
        {
            _logger.LogInformation("Calculating trip from {from} > {to}", fromStop, toStop);
            var travelAdvice = new List<SimpleTravelAdvice>();
            var disruptions = new List<SimpleDisruption>();

            foreach (var supplier in _dataSuppliers)
            {
                disruptions.AddRange(await supplier.GetDisruptionsAsync(fromStop, toStop));
                travelAdvice.AddRange(await supplier.GetTravelAdviceAsync(fromStop, toStop));
            }

            travelAdvice.OrderBy(advice => advice.Route.First().PlannedDeparture);
            var journeyResult = new JourneyResult
            {
                Disruptions = disruptions.ToList(),
                TravelAdvice = travelAdvice.ToList(),
            };

            CalculateExpectation(journeyResult);
            _logger.LogInformation("Calculated trip from {from} > {to}", fromStop, toStop);

            return journeyResult;
        }

        private static void CalculateExpectation(JourneyResult journeyResult)
        {
            if (journeyResult.TravelAdvice.All(advice => advice.Route.Any(route => route.Cancelled)) && journeyResult.Disruptions.Any(disruption => disruption.Type != DisruptionType.Maintenance && disruption.ExpectedEnd?.ToUniversalTime() > DateTime.UtcNow.AddMinutes(15)))
            {
                journeyResult.JourneyExpectation = JourneyExpectation.Nope;
            }
            else if (journeyResult.Disruptions.All(disruption => disruption.Type != DisruptionType.Maintenance) && journeyResult.TravelAdvice.Any(advice => advice.Route.Any(route => route.Cancelled)))
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
