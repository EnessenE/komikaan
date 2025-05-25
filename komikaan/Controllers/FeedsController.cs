using GTFS.Entities;
using komikaan.Data.GTFS;
using komikaan.Data.Models;
using komikaan.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace komikaan.Controllers
{
    [ApiController]
    [Route("v1/feeds")]
    public class FeedsController : ControllerBase
    {
        private readonly ILogger<FeedsController> _logger;
        private readonly IGTFSContext _gtfs;

        public FeedsController(ILogger<FeedsController> logger, IGTFSContext gtfs)
        {
            _logger = logger;
            _gtfs = gtfs;
        }


        [HttpGet]
        public async Task<List<Feed>?> GetFeedsAsync()
        {
            var feeds = await _gtfs.GetFeedsAsync();
            return feeds?.ToList();
        }

        [HttpGet("{dataOrigin}/routes")]
        public async Task<List<GTFSRoute>?> GetRoutesAsync(string dataOrigin)
        {
            var feeds = await _gtfs.GetRoutesAsync(dataOrigin);
            return feeds?.ToList();
        }

        [HttpGet("{dataOrigin}/stops")]
        public async Task<List<GTFSSearchStop>?> GetStopsAsync(string dataOrigin)
        {
            var feeds = await _gtfs.GetStopsAsync(dataOrigin);
            return feeds?.ToList();
        }

        [HttpGet("{dataOrigin}/positions")]
        public async Task<List<VehiclePosition>?> GetPositionsAsync(string dataOrigin)
        {
            var feeds = await _gtfs.GetPositionsAsync(dataOrigin);
            return feeds?.ToList();
        }


        [HttpGet("{dataOrigin}/alerts")]
        public async Task<ActionResult<IEnumerable<GTFSAlert>?>> GetAlertsAsync(string dataOrigin)
        {
            _logger.LogInformation("Fetching alerts for dataOrigin: {DataOrigin}", dataOrigin);
            var alerts = await _gtfs.GetAlertsAsync(dataOrigin);
            if (alerts == null)
            {
                _logger.LogWarning("No alerts found or error fetching for dataOrigin: {DataOrigin}", dataOrigin);
                return NotFound($"Alerts for data origin '{dataOrigin}' not found or an error occurred.");
            }
            return Ok(alerts);
        }
    }
}
