using komikthuis.Interfaces;
using komikthuis.Models.API.Disruptions;
using Microsoft.AspNetCore.Mvc;

namespace komikthuis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DisruptionController : ControllerBase
    {
        private readonly ILogger<DisruptionController> _logger;
        private readonly INSApi _nsApi;

        public DisruptionController(ILogger<DisruptionController> logger, INSApi nsApi)
        {
            _logger = logger;
            _nsApi = nsApi;
        }

        [HttpGet(Name = "amicominghome/{fromStation}/{toStation}")]
        public async Task<string> Get(string fromStation, string toStation)
        {
            var allStations = await GetAllStations();
            var fromNSStation = allStations[fromStation];
            var toNSStation = allStations[toStation];

            var allDisruptions = await _nsApi.GetAllDisruptions();

            var relevantFromDisruptions = allDisruptions.Where(disruption =>
                disruption.publicationSections.Any(publicationSection => publicationSection.section.stations.Any(station => station.name.Equals(fromStation, StringComparison.InvariantCultureIgnoreCase))));

            var relevantToDisruptions = allDisruptions.Where(disruption =>
                disruption.publicationSections.Any(publicationSection => publicationSection.section.stations.Any(station => station.name.Equals(fromStation, StringComparison.InvariantCultureIgnoreCase))));

            if (relevantToDisruptions.Any() || relevantToDisruptions.Any())
            {
                return "probarly not";
            }
            else
            {
                return "probarly";
            }
        }

        private async Task<IDictionary<string, Station>> GetAllStations()
        {
            var stations = await _nsApi.GetAllStations();
            var dict = new Dictionary<string, Station>();
            foreach (var station in stations)
            {
                dict.Add(station.namen.lang, station);
            }

            return dict;
        }
    }
}
