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
        public async Task<ActionResult<IEnumerable<GTFSSearchStop>>> SearchStopsAsync([FromQuery] string? filter, CancellationToken token)
        {
            var searchText = string.IsNullOrWhiteSpace(filter) ? "Amsterdam" : filter;
            _logger.LogInformation("Searching for {name}", searchText);
            var stops = await _dataSupplier.FindAsync(searchText, token);

            return Ok(stops);
        }


        [HttpGet("{stopId}/{stopType}")]
        public async Task<ActionResult<GTFSStopData>> GetDeparturesAsync(string stopId, ExtendedRouteType stopType)
        {
            var result = await _dataSupplier.GetStopAsync(stopId, stopType);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }


        [HttpGet("{stopId}/{stopType}/alerts")]
        public async Task<ActionResult<IEnumerable<GTFSAlert>>> GetAlertsAsync(Guid stopId, ExtendedRouteType stopType)
        {
            var result = await _dataSupplier.GetAlertsForStopAsync(stopId, stopType);

            return Ok(result);
        }


        [HttpGet("exact/{dataOrigin}/{stopId}")]
        public async Task<ActionResult<GTFSStopData>> GetDeparturesForSpecificStopAsync(string dataOrigin, string stopId)
        {
            var result = await _dataSupplier.GetStopAsync(dataOrigin, stopId);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        /// <summary>
        /// Based on coordinates, get all nearby stops 
        /// </summary>
        /// <param name="longitude">Longitude coordinate</param>
        /// <param name="latitude">Latitude coordinate</param>
        /// <returns></returns>
        [HttpGet("nearby")]
        public async Task<ActionResult<NearbyUserData>> NearbyStopsAsync(double longitude, double latitude, CancellationToken cancellationToken)
        {
            var data = new NearbyUserData
            {
                Stops = await _dataSupplier.GetNearbyStopsAsync(longitude, latitude, cancellationToken),
                Vehicles = await _dataSupplier.GetNearbyVehiclesAsync(longitude, latitude, cancellationToken)
            };
            return Ok(data);
        }

        [HttpGet("train-stations")]
        public async Task<ActionResult<IEnumerable<TrainStationPerformance>>> GetTrainStationsAsync([FromQuery] TrainStationViewportQuery query)
        {
            if (query.HasBounds)
            {
                var boundedStations = await _dataSupplier.GetTrainStationPerformanceInBoundsAsync(query);
                return Ok(boundedStations);
            }

            var stations = await _dataSupplier.GetTrainStationPerformanceAsync();
            return Ok(stations);
        }
    }
}
