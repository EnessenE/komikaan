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
        private readonly IDataSupplierContext _dataSupplier;

        public DisruptionController(ILogger<DisruptionController> logger, IDataSupplierContext dataSupplier)
        {
            _logger = logger;
            _dataSupplier = dataSupplier;
        }

        [HttpGet("{fromStop}/{toStop}")]
        public async Task<JourneyResult> GetTravelExpectationAsync(string fromStop, string toStop)
        {
            _logger.LogInformation("Calculating trip from {from} > {to}", fromStop, toStop);
            var disruptions = await _dataSupplier.GetDisruptionsAsync(fromStop, toStop);
            var travelAdvice = await _dataSupplier.GetTravelAdviceAsync(fromStop, toStop);

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
            if (journeyResult.TravelAdvice.All(advice => advice.Route.Any(route => route.Cancelled)) && journeyResult.Disruptions.Any(disruption => disruption.Type != DisruptionType.Maintenance && disruption.ExpectedEnd.ToUniversalTime() > DateTime.UtcNow.AddMinutes(15)))
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
