using komikaan.Data.Models;
using komikaan.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace komikaan.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class StopsController : ControllerBase
    {
        private readonly ILogger<StopsController> _logger;
        private readonly IStopManagerService _stopManager;

        public StopsController(ILogger<StopsController> logger, IStopManagerService stopManager)
        {
            _logger = logger;
            _stopManager = stopManager;
        }


        [HttpGet()]
        public async Task<IEnumerable<SimpleStop>> GetStopsAsync()
        {
            var stops = await _stopManager.GetAllStopsAsync();

            return stops;
        }

        /// <summary>
        /// Searches through all stops of the datasuppliers
        /// </summary>
        /// <returns></returns>
        [HttpGet("search")]
        public async Task<IEnumerable<SimpleStop>> SearchStopsAsync(string filter)
        {
            _logger.LogInformation("Searching for {name}", filter);
            var stops = await _stopManager.GetAllStopsAsync();

            var foundStops = stops.Where(stop => stop.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase)).Take(10).ToList();

            foreach (var found in foundStops)
            {
                _logger.LogInformation("Found {id} {name}", found.Ids, found.Name);
            }

            return foundStops;
        }
    }
}
