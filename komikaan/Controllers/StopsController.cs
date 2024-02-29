using komikaan.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace komikaan.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class StopsController : ControllerBase
    {
        private readonly ILogger<StopsController> _logger;
        private readonly IDataSupplierContext _dataSupplier;

        public StopsController(ILogger<StopsController> logger, IDataSupplierContext dataSupplier)
        {
            _logger = logger;
            _dataSupplier = dataSupplier;
        }


        [HttpGet()]
        public async Task<IEnumerable<string>> GetStopsAsync()
        {
            var data = await _dataSupplier.GetAllStops();
            return data.Keys;
        }

        /// <summary>
        /// Searches through all stops of the datasuppliers
        /// </summary>
        /// <returns></returns>
        [HttpGet("search")]
        public async Task<IEnumerable<string>> SearchStopsAsync(string filter)
        {
            _logger.LogInformation("Searching for {name}", filter);
            var data = await _dataSupplier.GetAllStops();
            var names = data.Keys.ToList();
            var foundResults = names.Where(name => name.Contains(filter, StringComparison.InvariantCultureIgnoreCase)).ToList();

            if (foundResults.Any())
            {
                foundResults = foundResults.Chunk(10).First().ToList();
            }

            return foundResults;
        }
    }
}
