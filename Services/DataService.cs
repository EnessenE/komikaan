using System.Diagnostics;
using komikthuis.Interfaces;

namespace komikthuis.Services
{
    public class DataService : IHostedService
    {
        private readonly IEnumerable<IDataSupplierContext> _dataSuppliers;
        private readonly ILogger<DataService> _logger;

        public DataService(IEnumerable<IDataSupplierContext> dataSuppliers, ILogger<DataService> logger)
        {
            _dataSuppliers = dataSuppliers;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var dataSupplier in _dataSuppliers)
            {
                _logger.LogInformation("Starting Datasupplier {name}", dataSupplier.GetType().FullName);
                var stopwatch = Stopwatch.StartNew();
                await dataSupplier.StartAsync(cancellationToken);
                _logger.LogInformation("Started Datasupplier {name} in {ms} ms", dataSupplier.GetType().FullName, stopwatch.ElapsedMilliseconds);

            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        { 
            return Task.CompletedTask;
        }
    }
}
