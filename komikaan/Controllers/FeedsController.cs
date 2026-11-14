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
        public async Task<ActionResult<IEnumerable<Feed>>> GetFeedsAsync()
        {
            var feeds = await _gtfs.GetFeedsAsync();
            return Ok(feeds);
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
        /// Returns the coverage of every feed
        /// </summary>
        /// <returns></returns>
        [HttpGet("coverage")]
        public ActionResult<IReadOnlyList<CoverageDataPoint>> GetCoverage()
        {
            return Ok(_gtfs.GetCoverage()?.ToList() ?? (IReadOnlyList<CoverageDataPoint>)[]);
        }

        [HttpGet("{dataOrigin}/routes")]
        public async Task<ActionResult<IEnumerable<GTFSRoute>>> GetRoutesAsync(string dataOrigin)
        {
            var routes = await _gtfs.GetDataOriginRoutesAsync(dataOrigin);
            return Ok(routes ?? Enumerable.Empty<GTFSRoute>());
        }

        [HttpGet("{dataOrigin}/agencies")]
        public async Task<ActionResult<IEnumerable<DatabaseAgency>>> GetAgenciesAsync(string dataOrigin)
        {
            var data = await _gtfs.GetAgenciesAsync(dataOrigin);
            return Ok(data ?? Enumerable.Empty<DatabaseAgency>());
        }

        [HttpGet("{dataOrigin}/stops")]
        public async Task<ActionResult<IEnumerable<GTFSSearchStop>>> GetStopsAsync(string dataOrigin)
        {
            var stops = await _gtfs.GetStopsAsync(dataOrigin);
            return Ok(stops ?? Enumerable.Empty<GTFSSearchStop>());
        }

        [HttpGet("{dataOrigin}/shapes")]
        public async Task<ActionResult<IEnumerable<Shape>>> GetShapesAsync(string dataOrigin)
        {
            var shapes = await _gtfs.GetShapesAsync(dataOrigin);
            return Ok(shapes ?? Enumerable.Empty<Shape>());
        }

        [HttpGet("{dataOrigin}/positions")]
        public async Task<ActionResult<IEnumerable<KomIkaanVehiclePosition>>> GetPositionsAsync(string dataOrigin)
        {
            var positions = await _gtfs.GetPositionsAsync(dataOrigin);
            return Ok(positions ?? Enumerable.Empty<KomIkaanVehiclePosition>());
        }


        [HttpGet("{dataOrigin}/alerts")]
        public async Task<ActionResult<IEnumerable<GTFSAlert>>> GetAlertsAsync(string dataOrigin, [FromQuery] int limit = 100, [FromQuery] int offset = 0)
        {
            _logger.LogInformation("Fetching alerts for dataOrigin: {DataOrigin}, limit: {Limit}, offset: {Offset}", dataOrigin, limit, offset);
            var alerts = await _gtfs.GetAlertsAsync(dataOrigin, limit, offset);
            return Ok(alerts ?? Enumerable.Empty<GTFSAlert>());
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
