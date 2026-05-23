using komikaan.Data.API;
using komikaan.Data.GTFS;
using komikaan.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace komikaan.Controllers
{
    [ApiController]
    [Route("v1/map")]
    public class MapController : ControllerBase
    {
        private readonly ILogger<MapController> _logger;
        private readonly IGTFSContext _gtfs;

        public MapController(ILogger<MapController> logger, IGTFSContext gtfs)
        {
            _logger = logger;
            _gtfs = gtfs;
        }

        [HttpGet("viewport")]
        public async Task<ActionResult<MapViewportData>> GetViewportDataAsync([FromQuery] MapViewportQuery query)
        {
            if (query.MinLatitude > query.MaxLatitude || query.MinLongitude > query.MaxLongitude)
            {
                return UnprocessableEntity("Viewport bounds are invalid.");
            }

            _logger.LogInformation(
                "Fetching map viewport data for lat [{MinLat}, {MaxLat}] lon [{MinLon}, {MaxLon}]",
                query.MinLatitude,
                query.MaxLatitude,
                query.MinLongitude,
                query.MaxLongitude
            );

            var data = await _gtfs.GetMapViewportDataAsync(query);

            return Ok(data);
        }

        [HttpGet("shape-routes")]
        public async Task<ActionResult<IEnumerable<GTFSRoute>>> GetRoutesByShapeAsync(
            [FromQuery] string dataOrigin,
            [FromQuery] string shapeId)
        {
            if (string.IsNullOrWhiteSpace(dataOrigin) || string.IsNullOrWhiteSpace(shapeId))
            {
                return UnprocessableEntity("Provide a valid dataOrigin and shapeId.");
            }

            var routes = await _gtfs.GetRoutesByShapeAsync(dataOrigin, shapeId);
            return Ok(routes);
        }

        [HttpGet("routes-near-point")]
        public async Task<ActionResult<IEnumerable<GTFSRoute>>> GetRoutesNearPointAsync(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] double bboxDegrees = 0.01)
        {
            var clampedBbox = Math.Clamp(bboxDegrees, 0.001, 0.5);
            var routes = await _gtfs.GetRoutesNearPointAsync(latitude, longitude, clampedBbox);
            return Ok(routes);
        }
    }
}
