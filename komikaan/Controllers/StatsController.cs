using komikaan.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace komikaan.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class StatsController : ControllerBase
    {
        private readonly IEnumerable<IDataSupplierContext> _dataSuppliers;

        public StatsController(IEnumerable<IDataSupplierContext> dataSuppliers)
        {
            _dataSuppliers = dataSuppliers;
        }


        [HttpGet()]
        public async Task<TransportStatistics> GetStops()
        {
            var stats = new TransportStatistics();
            foreach (var dataSupplier in _dataSuppliers)
            {
                var data = await dataSupplier.GetAllDisruptions(true);
                stats.Disruptions.Add(dataSupplier.Supplier, data.Count());
            }
            return stats;
        }
    }
}
