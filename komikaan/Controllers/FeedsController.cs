using komikaan.Context;
using komikaan.Data.GTFS;
using komikaan.Data.Models;
using komikaan.GTFS.Models.Static.Models;
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

        [HttpGet("names")]
        public async Task<ActionResult<IEnumerable<string>>> GetFeedNamesAsync()
        {
            var names = await _gtfs.GetFeedNamesAsync();
            return Ok(names);
        }

        [HttpGet("realtime-names")]
        public async Task<ActionResult<IEnumerable<string>>> GetRealtimeFeedNamesAsync()
        {
            var names = await _gtfs.GetRealtimeFeedNamesAsync();
            return Ok(names);
        }

        /// <summary>
        /// Returns the converage of every feed
        /// </summary>
        /// <returns></returns>
        [HttpGet("coverage")]

        public IReadOnlyList<CoverageDataPoint> GetCoverage()
        {
            return _gtfs.GetCoverage().ToList();
        }

        [HttpGet("{dataOrigin}/routes")]
        public async Task<List<GTFSRoute>?> GetRoutesAsync(string dataOrigin)
        {
            var feeds = await _gtfs.GetDataOriginRoutesAsync(dataOrigin);
            return feeds?.ToList();
        }

        [HttpGet("{dataOrigin}/agencies")]
        public async Task<List<DatabaseAgency>?> GetAgenciesAsync(string dataOrigin)
        {
            var data = await _gtfs.GetAgenciesAsync(dataOrigin);
            return data?.ToList();
        }

        [HttpGet("{dataOrigin}/stops")]
        public async Task<List<GTFSSearchStop>?> GetStopsAsync(string dataOrigin)
        {
            var feeds = await _gtfs.GetStopsAsync(dataOrigin);
            return feeds?.ToList();
        }

        [HttpGet("{dataOrigin}/shapes")]
        public async Task<List<Shape>?> GetShapesAsync(string dataOrigin)
        {
            var shapes = await _gtfs.GetShapesAsync(dataOrigin);
            return shapes?.ToList();
        }

        [HttpGet("{dataOrigin}/positions")]
        public async Task<List<KomIkaanVehiclePosition>?> GetPositionsAsync(string dataOrigin)
        {
            var feeds = await _gtfs.GetPositionsAsync(dataOrigin);
            return feeds?.ToList();
        }


        [HttpGet("{dataOrigin}/alerts")]
        public async Task<ActionResult<IEnumerable<GTFSAlert>?>> GetAlertsAsync(string dataOrigin, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
        {
            _logger.LogInformation("Fetching alerts for dataOrigin: {DataOrigin}, limit: {Limit}, offset: {Offset}", dataOrigin, limit, offset);
            var alerts = await _gtfs.GetAlertsAsync(dataOrigin, limit, offset);
            if (alerts == null)
            {
                _logger.LogWarning("No alerts found or error fetching for dataOrigin: {DataOrigin}", dataOrigin);
                return NotFound($"Alerts for data origin '{dataOrigin}' not found or an error occurred.");
            }
            return Ok(alerts);
        }

        [HttpGet("delays/top")]
        public async Task<ActionResult<IEnumerable<TopDelayedStop>>> GetTopDelayedStopsAsync([FromQuery] int limit, [FromQuery] string? dataOrigin)
        {
            var safeLimit = limit <= 0 ? 25 : limit;
            _logger.LogInformation("Fetching top delayed stops. limit: {Limit}, dataOrigin: {DataOrigin}", safeLimit, dataOrigin);
            var topStops = await _gtfs.GetTopDelayedStopsAsync(safeLimit, dataOrigin);
            return Ok(topStops);
        }
    }
}
