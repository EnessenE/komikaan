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
        private readonly IEnumerable<IDataSupplierContext> _dataSuppliers;

        public StopsController(ILogger<StopsController> logger, IEnumerable<IDataSupplierContext> dataSuppliers)
        {
            _logger = logger;
            _dataSuppliers = dataSuppliers;
        }


        [HttpGet()]
        public async Task<IEnumerable<string>> GetStopsAsync()
        {
            var stops = new List<SimplifiedStop>();

            foreach (var supplier in _dataSuppliers)
            {
                var data = await supplier.GetAllStops();
                stops.AddRange(data);
            }

            return stops.Select(stop => stop.Name);
        }

        /// <summary>
        /// Searches through all stops of the datasuppliers
        /// </summary>
        /// <returns></returns>
        [HttpGet("search")]
        public async Task<IEnumerable<SimplifiedStop>> SearchStopsAsync(string filter)
        {
            _logger.LogInformation("Searching for {name}", filter);
            var stops = new List<SimplifiedStop>();

            foreach (var supplier in _dataSuppliers)
            {
                var data = await supplier.GetAllStops();
                stops.AddRange(data);
            }
            var foundStops = stops.Where(stop => stop.Name.Contains(filter, StringComparison.InvariantCultureIgnoreCase)).Take(10).ToList();

            foreach (var found in foundStops)
            {
                _logger.LogInformation("Found {id} {name}", found.Id, found.Name);
            }

            return foundStops;
        }
    }
}
