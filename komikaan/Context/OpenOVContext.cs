using GTFS;
using GTFS.IO;
using komikaan.Data.Enums;
using komikaan.Data.Models;
using komikaan.Interfaces;
using komikaan.Models;
using komikaan.Models.API.NS;

namespace komikaan.Context
{
    public class OpenOVContext : IDataSupplierContext
    {
        private ILogger<OpenOVContext> _logger;
        private GTFSFeed _feed;

        private IList<SimplifiedStop> _stops;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "<Pending>")]
        public OpenOVContext(ILogger<OpenOVContext> logger)
        {
            _logger = logger;
            _stops = new List<SimplifiedStop>();
        }

        public DataSource Supplier { get; } = DataSource.OpenOV;
        public Task StartAsync(CancellationToken cancellationToken)
        {
            string path = "C:\\Users\\maile\\Downloads\\gtfs-nl";

            var reader = new GTFSReader<GTFSFeed>();
            var feed = reader.Read(path);

            _feed = feed;

            GenerateStops(feed);
            
            return Task.CompletedTask;
        }

        private void GenerateStops(GTFSFeed feed)
        {
            foreach (var stop in feed.Stops)
            {
                var simpleStop = new SimplifiedStop();
                var existingStop =
                    _stops.FirstOrDefault(existingStop => existingStop.Name.Equals(stop.Name, StringComparison.InvariantCultureIgnoreCase));
                if (existingStop != null)
                {
                    existingStop.Id.Add(stop.Id);
                    existingStop.Code.Add(stop.Code);
                }
                else
                {
                    simpleStop.Name = string.Intern(stop.Name);
                    simpleStop.Id = new List<string>() { stop.Id };
                    simpleStop.Code = new List<string>() { stop.Code };
                    _stops.Add(simpleStop);
                }
            }
        }

        public Task LoadRelevantDataAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("No data to reload");
            return Task.CompletedTask;
        }

        public Task<IEnumerable<SimpleDisruption>> GetDisruptionsAsync(string from, string to)
        {
            return Task.FromResult(Enumerable.Empty<SimpleDisruption>());
        }

        public Task<IEnumerable<SimpleDisruption>> GetAllDisruptions(bool active)
        {
            return Task.FromResult(Enumerable.Empty<SimpleDisruption>());
        }

        public Task<IEnumerable<SimplifiedStop>> GetAllStops()
        {
            return Task.FromResult(_stops.AsEnumerable());
        }


        public Task<IEnumerable<SimpleTravelAdvice>> GetTravelAdviceAsync(string from, string to)
        {
            return Task.FromResult(Enumerable.Empty<SimpleTravelAdvice>());
        }
    }
}
