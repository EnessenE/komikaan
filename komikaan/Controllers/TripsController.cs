using komikaan.Context;
using komikaan.Data.GTFS;
using komikaan.Data.Models;
using komikaan.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace komikaan.Controllers
{
    [ApiController]
    [Route("v1/trips")]
    public class TripsController : ControllerBase
    {
        private readonly ILogger<TripsController> _logger;
        private readonly GTFSContext _gtfs;

        public TripsController(ILogger<TripsController> logger, GTFSContext gtfs)
        {
            _logger = logger;
            _gtfs = gtfs;
        }


        [HttpGet("{tripid}/")]
        public async Task<IEnumerable<GTFSTripStop>> GetTripAsync(string tripId)
        {
            return await _gtfs.GetTripAsync(tripId);
        }
    }
}
