using komikaan.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace komikaan.Controllers
{
    [ApiController]
    [Route("v1/stats")]
    public class StatsController : ControllerBase
    {
        private readonly IEnumerable<IDataSupplierContext> _dataSuppliers;

        public StatsController(IEnumerable<IDataSupplierContext> dataSuppliers)
        {
            _dataSuppliers = dataSuppliers;
        }


        [HttpGet()]
        public async Task<TransportStatistics> GetStopsAsync()
        {
            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var stats = new TransportStatistics();
            foreach (var dataSupplier in _dataSuppliers)
            {
                var data = await dataSupplier.GetAllDisruptionsAsync(true, tokenSource.Token);
                stats.Disruptions.Add(dataSupplier.Supplier, data.Count());
            }
            return stats;
        }
    }
}
