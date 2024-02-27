using System.Text.Json.Serialization;
using komikthuis.Models.API.NS;

namespace komikthuis.Models;

public class JourneyResult
{
    [JsonConverter(typeof(JsonStringEnumConverter))]

    public JourneyExpectation JourneyExpectation { get; set; }
    public List<SimpleDisruption> Disruptions { get; set; }
    public List<SimpleTravelAdvice> TravelAdvice { get; set; }
}