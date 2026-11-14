using komikaan.Data.GTFS;
using komikaan.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace komikaan.Controllers
{
    [ApiController]
    [Route("v1/trips")]
    public class TripsController : ControllerBase
    {
        private readonly ILogger<TripsController> _logger;
        private readonly IGTFSContext _gtfs;

        public TripsController(ILogger<TripsController> logger, IGTFSContext gtfs)
        {
            _logger = logger;
            _gtfs = gtfs;
        }


        [HttpGet("{tripid}")]
        public async Task<ActionResult<GTFSTrip>> GetTripAsync(Guid tripId)
        {
            var trip = await _gtfs.GetTripAsync(tripId, DateTimeOffset.UtcNow);
            if (trip == null)
            {
                return NotFound();
            }
            return Ok(trip);
        }
    }
}
