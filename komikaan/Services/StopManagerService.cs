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

        private List<SimpleStop> _stops;

        public StopManagerService(IEnumerable<IDataSupplierContext> dataSuppliers, ILogger<StopManagerService> logger)
        {
            _dataSuppliers = dataSuppliers;
            _logger = logger;
        }

        public Task<IEnumerable<SimpleStop>> GetAllStopsAsync()
        {
            return Task.FromResult(_stops.AsEnumerable());
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var stops = new List<SimpleStop>();

            _logger.LogInformation("Starting merging stops that are the same from different suppliers");
            foreach (var supplier in _dataSuppliers)
            {
                var newStops = await supplier.GetAllStopsAsync();
                ProcessStopsPerSupplier(newStops, stops, supplier);
            }

            _stops = stops;
            _logger.LogInformation("Loaded {item} stops",  _stops.Count);
        }

        private void ProcessStopsPerSupplier(IEnumerable<SimpleStop> newStops, List<SimpleStop> stops, IDataSupplierContext supplier)
        {
            foreach (var stop in newStops)
            {
                var existingStop =
                    stops.FirstOrDefault(item => item.Name.Equals(stop.Name, StringComparison.InvariantCultureIgnoreCase));

                if (existingStop != null)
                {
                    ModifyExistingStop(supplier, stop, existingStop);
                }
                else
                {
                    stops.Add(stop);
                }
            }
        }

        private void ModifyExistingStop(IDataSupplierContext supplier, SimpleStop stop, SimpleStop existingStop)
        {
            if (stop.Codes.Any())
            {
                if (!existingStop.Codes.ContainsKey(supplier.Supplier))
                {
                    existingStop.Codes.Add(supplier.Supplier, stop.Codes.First().Value);
                }
                else
                {
                    _logger.LogInformation("Merging {name} codes from same supplier", stop.Name);
                    existingStop.Codes[supplier.Supplier].AddRange(stop.Codes.First().Value);
                }
            }

            if (stop.Ids.Any())
            {
                if (!existingStop.Ids.ContainsKey(supplier.Supplier))
                {
                    existingStop.Ids.Add(supplier.Supplier, stop.Ids.First().Value);
                }
                else
                {
                    existingStop.Ids[supplier.Supplier].AddRange(stop.Ids.First().Value);
                    _logger.LogInformation("Merging {name} codes from same supplier", stop.Name);
                }
            }
        }
    }
}
