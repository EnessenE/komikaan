using System.Diagnostics;
using System.Threading;
using komikthuis.Interfaces;

namespace komikthuis.Services
{
    public class DataService : BackgroundService
    {
        private readonly IEnumerable<IDataSupplierContext> _dataSuppliers;
        private readonly ILogger<DataService> _logger;
        private readonly PeriodicTimer _periodicTimer;

        public DataService(IEnumerable<IDataSupplierContext> dataSuppliers, ILogger<DataService> logger)
        {
            _dataSuppliers = dataSuppliers;
            _logger = logger;
            _periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (await _periodicTimer.WaitForNextTickAsync(stoppingToken))
                {
                    _logger.LogInformation("Reloading all data suppliers");
                    foreach (var dataSupplier in _dataSuppliers)
                    {
                        _logger.LogInformation("Reloading Datasupplier {name}", dataSupplier.GetType().FullName);
                        var stopwatch = Stopwatch.StartNew();
                        await dataSupplier.LoadRelevantData(stoppingToken);
                        _logger.LogInformation("Reloading Datasupplier {name} in {ms} ms", dataSupplier.GetType().FullName, stopwatch.ElapsedMilliseconds);
                    }

                    _logger.LogInformation("Finished reloading all data suppliers");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Reload timer was cancelled");
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var dataSupplier in _dataSuppliers)
            {
                _logger.LogInformation("Starting Datasupplier {name}", dataSupplier.GetType().FullName);
                var stopwatch = Stopwatch.StartNew();
                await dataSupplier.LoadRelevantData(cancellationToken);
                _logger.LogInformation("Started Datasupplier {name} in {ms} ms", dataSupplier.GetType().FullName, stopwatch.ElapsedMilliseconds);
            }

            await base.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
