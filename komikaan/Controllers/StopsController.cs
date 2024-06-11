using komikaan.Context;
using komikaan.Data.GTFS;
using komikaan.Data.Models;
using komikaan.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace komikaan.Controllers
{
    [ApiController]
    [Route("v1/stops")]
    public class StopsController : ControllerBase
    {
        private readonly ILogger<StopsController> _logger;
        private readonly IStopManagerService _stopManager;
        private readonly GTFSContext _gtfs;

        public StopsController(ILogger<StopsController> logger, IStopManagerService stopManager, GTFSContext gtfs)
        {
            _logger = logger;
            _stopManager = stopManager;
            _gtfs = gtfs;
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

        [HttpGet("{stopId}/{stopType}")]
        public async Task<GTFSStopData> GetDeparturesAsync(string stopId, StopType stopType)
        {
            return await _gtfs.GetStopAsync(stopId, stopType);
        }
    }
}
