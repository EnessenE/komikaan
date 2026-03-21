using komikaan.Data.GTFS;
using komikaan.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace komikaan.Controllers
{
    [ApiController]
    [Route("v1/routes")]
    public class RoutesController : ControllerBase
    {
        private readonly ILogger<RoutesController> _logger;
        private readonly IGTFSContext _gtfs;

        public RoutesController(ILogger<RoutesController> logger, IGTFSContext gtfs)
        {
            _logger = logger;
            _gtfs = gtfs;
        }

        [HttpGet("{dataOrigin}/{routeId}")]
        public async Task<ActionResult<GTFSRouteDetails>> GetRouteAsync(string dataOrigin, string routeId)
        {
            _logger.LogInformation("Fetching route {RouteId} for {DataOrigin}", routeId, dataOrigin);

            if (string.IsNullOrWhiteSpace(dataOrigin) || string.IsNullOrWhiteSpace(routeId))
            {
                return UnprocessableEntity("Provide a valid data origin and route id");
            }

            var route = await _gtfs.GetRouteAsync(dataOrigin, routeId);
            if (route == null)
            {
                return NotFound();
            }

            return Ok(route);
        }

        [HttpGet("{dataOrigin}/{routeId}/timetable")]
        public async Task<ActionResult<IEnumerable<GTFSRouteTimetableRow>>> GetTimetableAsync(
            string dataOrigin, string routeId, [FromQuery] DateOnly? date = null)
        {
            _logger.LogInformation("Fetching timetable for {RouteId} / {DataOrigin} on {Date}", routeId, dataOrigin, date);

            if (string.IsNullOrWhiteSpace(dataOrigin) || string.IsNullOrWhiteSpace(routeId))
                return UnprocessableEntity("Provide a valid data origin and route id");

            var rows = await _gtfs.GetTimetableAsync(dataOrigin, routeId, date);
            var list = rows.ToList();
            if (!list.Any()) return NotFound();
            return Ok(list);
        }
    }
}