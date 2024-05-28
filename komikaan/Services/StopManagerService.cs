using System.Threading.Tasks;
using komikaan.Data.Models;
using komikaan.Interfaces;
using komikaan.Models;

namespace komikaan.Services
{
    //TODO: This name makes no sense
    public class StopManagerService : IStopManagerService
    {
        private readonly IEnumerable<IDataSupplierContext> _dataSuppliers;
        private readonly ILogger<StopManagerService> _logger;

        public StopManagerService(IEnumerable<IDataSupplierContext> dataSuppliers, ILogger<StopManagerService> logger)
        {
            _dataSuppliers = dataSuppliers;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Started stop manager");
            await Task.CompletedTask;

        }
         
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements", Justification = "TODO")]
        public async Task<IEnumerable<SimpleStop>> FindStopsAsync(string text)
        {
            var stops = new List<SimpleStop>();
            var tasks = new List<Task>();
            foreach (var supplier in _dataSuppliers)
            {
                tasks.Add(supplier.FindAsync(text, stops, CancellationToken.None));
            }

            Task searchTask = Task.WhenAll(tasks);

            await searchTask.WaitAsync(TimeSpan.FromSeconds(5));

            if (searchTask.Status == TaskStatus.RanToCompletion)
            { 
                _logger.LogInformation("All tasks succeeded. Found {total} stops", stops.Count); 
            }
            else if (searchTask.Status == TaskStatus.Faulted)
            {
                _logger.LogWarning("Atleast some tasks failed"); 
            }
            else
            {
                _logger.LogWarning("Task ended in an unexpected state {state}", searchTask.Status);
            }
            return stops;
        }
    }
}
