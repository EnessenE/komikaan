using System.Diagnostics;
using System.Threading;
using komikaan.Interfaces;

namespace komikaan.Services
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
                    await LoadAllDataSuppliers(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Reload timer was cancelled");
            }
        }

        private async Task LoadAllDataSuppliers(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reloading all data suppliers");
            foreach (var dataSupplier in _dataSuppliers)
            {
                await LoadDataSupplier(stoppingToken, dataSupplier);
            }

            _logger.LogInformation("Finished reloading all data suppliers");
        }

        private async Task LoadDataSupplier(CancellationToken stoppingToken, IDataSupplierContext dataSupplier)
        {
            _logger.LogInformation("Reloading Datasupplier {name}", dataSupplier.GetType().FullName);
            var stopwatch = Stopwatch.StartNew();
            await dataSupplier.LoadRelevantData(stoppingToken);
            _logger.LogInformation("Reloading Datasupplier {name} in {ms} ms", dataSupplier.GetType().FullName,
                stopwatch.ElapsedMilliseconds);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await LoadAllDataSuppliers(cancellationToken);

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
