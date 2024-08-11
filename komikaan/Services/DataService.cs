using System.Diagnostics;
using komikaan.Interfaces;

namespace komikaan.Services
{
    public class DataService : BackgroundService
    {
        private readonly IEnumerable<IGTFSContext> _dataSuppliers;
        private readonly ILogger<DataService> _logger;
        private readonly PeriodicTimer _periodicTimer;

        public DataService(IEnumerable<IGTFSContext> dataSuppliers, ILogger<DataService> logger)
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
                    await LoadAllDataSuppliersAsync();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Reload timer was cancelled");
            }
        }

        private async Task LoadAllDataSuppliersAsync()
        {
            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(25));
            _logger.LogInformation("Reloading all data suppliers");
            foreach (var dataSupplier in _dataSuppliers)
            {
                await LoadDataSupplierAsync(tokenSource.Token, dataSupplier);
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
        private async Task StartDataSupplierAsync(CancellationToken stoppingToken, IGTFSContext dataSupplier)
        {
            _logger.LogInformation("Starting Datasupplier {name}", dataSupplier.GetType().FullName);
            var stopwatch = Stopwatch.StartNew();
            await dataSupplier.StartAsync(stoppingToken);
            _logger.LogInformation("Started Datasupplier {name} in {ms} ms", dataSupplier.GetType().FullName,
                stopwatch.ElapsedMilliseconds);
        }


        private async Task LoadDataSupplierAsync(CancellationToken cancellationToken, IGTFSContext dataSupplier)
        {
            _logger.LogInformation("Reloading Datasupplier {name}", dataSupplier.GetType().FullName);
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await dataSupplier.LoadRelevantDataAsync(cancellationToken);
            }
            catch (TaskCanceledException taskCancelledException)
            {
                _logger.LogError(taskCancelledException, "Timed out while reloading information");
            }
            catch(OperationCanceledException operationCancelledException)
            {
                _logger.LogError(operationCancelledException, "Operation cancelled while reloading info");
            }
            _logger.LogInformation("Reloading Datasupplier {name} in {ms} ms", dataSupplier.GetType().FullName,
                stopwatch.ElapsedMilliseconds);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await StartAllDataSuppliersAsync(cancellationToken);
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
