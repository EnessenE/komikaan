﻿using komikaan.Data.API;
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
        public async Task<IEnumerable<GTFSSearchStop>> SearchStopsAsync(string filter)
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
        public async Task<GTFSStopData?> GetDeparturesAsync(Guid stopId, ExtendedRouteType stopType)
        {
            //TODO: 404
            return await _dataSupplier.GetStopAsync(stopId, stopType) ?? null;
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
