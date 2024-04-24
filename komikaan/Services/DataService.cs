using System.Diagnostics;
using komikaan.Interfaces;

namespace komikaan.Services
{
    public class DataService : BackgroundService
    {
        private readonly IEnumerable<IDataSupplierContext> _dataSuppliers;
        private readonly ILogger<DataService> _logger;
        private readonly PeriodicTimer _periodicTimer;
        private readonly IStopManagerService _stopManagerService;

        public DataService(IEnumerable<IDataSupplierContext> dataSuppliers, ILogger<DataService> logger, IStopManagerService stopManagerService)
        {
            _dataSuppliers = dataSuppliers;
            _logger = logger;
            _stopManagerService = stopManagerService;
            _periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (await _periodicTimer.WaitForNextTickAsync(stoppingToken))
                {
                    await LoadAllDataSuppliersAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Reload timer was cancelled");
            }
        }

        private async Task LoadAllDataSuppliersAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reloading all data suppliers");
            foreach (var dataSupplier in _dataSuppliers)
            {
                await LoadDataSupplierAsync(stoppingToken, dataSupplier);
            }

            _logger.LogInformation("Finished reloading all data suppliers");
        }

        private async Task StartAllDataSuppliersAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting data suppliers");
            foreach (var dataSupplier in _dataSuppliers)
            {
                await StartDataSupplierAsync(cancellationToken, dataSupplier);
            }

            _logger.LogInformation("Finished starting data suppliers");
        }
        private async Task StartDataSupplierAsync(CancellationToken stoppingToken, IDataSupplierContext dataSupplier)
        {
            _logger.LogInformation("Starting Datasupplier {name}", dataSupplier.GetType().FullName);
            var stopwatch = Stopwatch.StartNew();
            await dataSupplier.StartAsync(stoppingToken);
            _logger.LogInformation("Started Datasupplier {name} in {ms} ms", dataSupplier.GetType().FullName,
                stopwatch.ElapsedMilliseconds);
        }


        private async Task LoadDataSupplierAsync(CancellationToken stoppingToken, IDataSupplierContext dataSupplier)
        {
            _logger.LogInformation("Reloading Datasupplier {name}", dataSupplier.GetType().FullName);
            var stopwatch = Stopwatch.StartNew();
            await dataSupplier.LoadRelevantDataAsync(stoppingToken);
            _logger.LogInformation("Reloading Datasupplier {name} in {ms} ms", dataSupplier.GetType().FullName,
                stopwatch.ElapsedMilliseconds);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await StartAllDataSuppliersAsync(cancellationToken);
            await _stopManagerService.StartAsync(cancellationToken);
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
