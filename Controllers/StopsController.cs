using komikthuis.Interfaces;
using komikthuis.Models;
using Microsoft.AspNetCore.Mvc;

namespace komikthuis.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
        public async Task<IEnumerable<string>> GetStops()
        {
            var data = await _dataSupplier.GetAllStops();
            return data.Keys;
        }


        [HttpGet("search")]
        public async Task<IEnumerable<string>> SearchStops(string filter)
        {
            var data = await _dataSupplier.GetAllStops();
            var names = data.Keys.ToList();
            var foundResults = names.Where(name => name.Contains(filter, StringComparison.InvariantCultureIgnoreCase));
            return foundResults.Chunk(10).First() ?? Enumerable.Empty<string>();
        }
    }
}
