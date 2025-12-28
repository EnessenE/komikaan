using komikaan.Data.API;
using komikaan.Data.GTFS;
using komikaan.GTFS.Models.Static.Enums;
using komikaan.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace komikaan.Controllers
{
    [ApiController]
    [Route("v1/stops")]
    public class StopsController : ControllerBase
    {
        private readonly ILogger<StopsController> _logger;
        private readonly IGTFSContext _dataSupplier;

        public StopsController(ILogger<StopsController> logger, IGTFSContext dataSupplierContext)
        {
            _logger = logger;
            _dataSupplier = dataSupplierContext;
        }

        /// <summary>
        /// Searches through all stops of the datasuppliers
        /// </summary>
        /// <returns></returns>
        [HttpGet("search")]
        public async Task<IEnumerable<GTFSSearchStop>> SearchStopsAsync(string filter, CancellationToken token)
        {
            _logger.LogInformation("Searching for {name}", filter);
            var stops = await _dataSupplier.FindAsync(filter, token);

            return stops;
        }


        [HttpGet("{stopId}/{stopType}")]
        public async Task<ActionResult<GTFSStopData>> GetDeparturesAsync(string stopId, ExtendedRouteType stopType)
        {
            if (!string.IsNullOrWhiteSpace(stopId))
            {
                var result = await _dataSupplier.GetStopAsync(stopId, stopType);

                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            return UnprocessableEntity("Provide a valid stop id");
        }


        [HttpGet("exact/{dataOrigin}/{stopId}")]
        public async Task<ActionResult<GTFSStopData>> GetDeparturesForSpecificStopAsync(string dataOrigin, string stopId)
        {
            if (!string.IsNullOrWhiteSpace(stopId))
            {
                var result = await _dataSupplier.GetStopAsync(dataOrigin, stopId);

                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            return UnprocessableEntity("Provide a valid stop id");
        }

        /// <summary>
        /// Based on coordinates, get all nearby stops 
        /// </summary>
        /// <param name="longitude">Longitude coordinate</param>
        /// <param name="latitude">Latitude coordinate</param>
        /// <returns></returns>
        [HttpGet("nearby")]
        public async Task<NearbyUserData> NearbyStopsAsync(double longitude, double latitude, CancellationToken cancellationToken)
        {
            var data = new NearbyUserData();
            data.Stops = await _dataSupplier.GetNearbyStopsAsync(longitude, latitude, cancellationToken);
            data.Vehicles = await _dataSupplier.GetNearbyVehiclesAsync(longitude, latitude, cancellationToken);
            return data;
        }
    }
}
