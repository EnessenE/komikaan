using komikthuis.Interfaces;
using komikthuis.Models;
using Microsoft.AspNetCore.Mvc;

namespace komikthuis.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        public async Task<JourneyResult> Get(string fromStop, string toStop)
        {
            _logger.LogInformation("Calculating trip from {from} > {to}", fromStop, toStop);
            var disruptions = await _dataSupplier.GetDisruptions(fromStop, toStop);
            var journeyResult = new JourneyResult();
            journeyResult.Disruptions = disruptions.ToList();

            journeyResult.TravelAdvice = (await _dataSupplier.GetTravelAdvice(fromStop, toStop)).ToList();

            if (journeyResult.Disruptions.Any() && journeyResult.Disruptions.Any(disruption => disruption.Type != DisruptionType.Maintenance && disruption.ExpectedEnd > DateTime.Now))
            {
                journeyResult.JourneyExpectation = JourneyExpectation.Nope;
            }
            else if (journeyResult.Disruptions.All(disruption => disruption.Type == DisruptionType.Maintenance) && journeyResult.TravelAdvice.Any(advice => advice.Route.Any(route => route.Cancelled)))
            {
                journeyResult.JourneyExpectation = JourneyExpectation.Maybe;
            }
            else
            {
                journeyResult.JourneyExpectation = JourneyExpectation.Full;
            }
            _logger.LogInformation("Calculating trip from {from} > {to}", fromStop, toStop);

            return journeyResult;
        }

        [HttpGet("stations")]
        public async Task<List<string>> GetStations()
        {
            var data = await _dataSupplier.GetAllStops();
            return data.Keys.ToList();
        }

    }
}
