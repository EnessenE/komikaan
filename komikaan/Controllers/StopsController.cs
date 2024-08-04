using komikaan.Context;
using komikaan.Data.GTFS;
using komikaan.Data.Models;
using komikaan.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.Geometries;

namespace komikaan.Controllers
{
    [ApiController]
    [Route("v1/stops")]
    public class StopsController : ControllerBase
    {
        private readonly ILogger<StopsController> _logger;
        private readonly IDataSupplierContext _dataSupplier;
        private readonly GTFSContext _gtfs;

        public StopsController(ILogger<StopsController> logger, IDataSupplierContext dataSupplierContext, GTFSContext gtfs)
        {
            _logger = logger;
            _dataSupplier = dataSupplierContext;
            _gtfs = gtfs;
        }

        /// <summary>
        /// Searches through all stops of the datasuppliers
        /// </summary>
        /// <returns></returns>
        [HttpGet("search")]
        public async Task<IEnumerable<GTFSStop>> SearchStopsAsync(string filter)
        {
            _logger.LogInformation("Searching for {name}", filter);
            var stops = await _dataSupplier.FindAsync(filter, CancellationToken.None);

            foreach (var found in stops)
            {
                _logger.LogInformation("Found {id} {name}", found.Id, found.Name);
            }

            return stops;
        }

        [HttpGet("{stopId}/{stopType}")]
        public async Task<GTFSStopData> GetDeparturesAsync(Guid stopId, StopType stopType)
        {
            return await _gtfs.GetStopAsync(stopId, stopType);
        }

        /// <summary>
        /// Based on coordinates, get all nearby stops 
        /// </summary>
        /// <param name="longitude">Longitude coordinate</param>
        /// <param name="latitude">Latitude coordinate</param>
        /// <returns></returns>
        [HttpGet("nearby")]
        public async Task<IEnumerable<GTFSStop>> NearbyStopsAsync(double longitude, double latitude)
        {
            return await _dataSupplier.GetNearbyStopsAsync(longitude, latitude, CancellationToken.None);
        }
    }
}
