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

        /// <summary>
        /// Searches through all stops of the datasuppliers
        /// </summary>
        /// <returns></returns>
        [HttpGet("search")]
        public async Task<IEnumerable<SimpleStop>> SearchStopsAsync(string filter)
        {
            _logger.LogInformation("Searching for {name}", filter);
            var stops = await _stopManager.FindStopsAsync(filter);

            foreach (var found in stops)
            {
                _logger.LogInformation("Found {id} {name}", found.Ids, found.Name);
            }

            return stops;
        }

        [HttpGet("{stopId}/departures")]
        public async Task<IEnumerable<string>> GetDeparturesAsync(string stopId)
        {
            return await Task.FromResult(new List<string>() { "ap", "ba", "darfwet" });
        }
    }
}
