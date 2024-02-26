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

        [HttpGet("{fromStation}/{toStation}")]
        public async Task<JourneyResult> Get(string fromStation, string toStation)
        {
            var journeyResult = new JourneyResult();
            var allStations = await GetAllStations();
            var fromNSStation = allStations[fromStation];
            var toNSStation = allStations[toStation];

            var allDisruptions = await _nsApi.GetAllDisruptions();

            var relevantFromDisruptions = allDisruptions.Where(disruption =>
                disruption.publicationSections.Any(publicationSection => publicationSection.section.stations.Any(station => station.name.Equals(fromStation, StringComparison.InvariantCultureIgnoreCase)))).ToList();

            var relevantToDisruptions = allDisruptions.Where(disruption =>
                disruption.publicationSections.Any(publicationSection => publicationSection.section.stations.Any(station => station.name.Equals(toStation, StringComparison.InvariantCultureIgnoreCase)))).ToList();

            var disruptions = relevantToDisruptions;
            disruptions.AddRange(relevantFromDisruptions);
            disruptions = disruptions.FindAll(disruption => disruption.isActive).ToList();
            journeyResult.Disruptions = new List<SimpleDisruption>();
            foreach (var disruption in disruptions)
            {
                _logger.LogInformation("Relevant: {name}, active {act}", disruption.title, disruption.isActive);
                var simpleDisruption = new SimpleDisruption();
                simpleDisruption.Title = disruption.title;
                var simpleText = disruption.timespans.Select(x => x.situation);
                simpleDisruption.Descriptions = simpleText.Select(situation => situation.label);
                var advices = disruption.timespans.Select(x => x.advices);

                simpleDisruption.Advices = advices;
                simpleDisruption.ExpectedEnd = disruption.end;
                simpleDisruption.Type = disruption.type;
                journeyResult.Disruptions.Add(simpleDisruption);
            }


            if (!disruptions.Any())
            {
                journeyResult.Completable = true;
            }

            return journeyResult;
        }

        [HttpGet("stations")]
        public async Task<List<string>> GetStations()
        {
            var data = await GetAllStations();
            return data.Keys.ToList();
        }

        private async Task<IDictionary<string, Station>> GetAllStations()
        {
            var stationRoot = await _nsApi.GetAllStations();
            var dict = new Dictionary<string, Station>();
            foreach (var station in stationRoot.payload)
            {
                dict.Add(station.namen.lang, station);
            }

            return dict;
        }
    }

    public class JourneyResult
    {
        public bool Completable { get; set; }
        public List<SimpleDisruption> Disruptions { get; set; }
    }

    public class SimpleDisruption
    {
        public string Title { get; set; }
        public DateTime ExpectedEnd { get; set; }
        public IEnumerable<string> Descriptions { get; set; }
        public string Type { get; set; }
        public IEnumerable<List<string>> Advices { get; set; }
    }
}
