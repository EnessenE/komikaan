using System.Text.Json.Serialization;
using komikaan.Enums;

namespace komikaan.Models;

public class JourneyResult
{
    [JsonConverter(typeof(JsonStringEnumConverter))]

    public JourneyExpectation JourneyExpectation { get; set; }
    public List<SimpleDisruption> Disruptions { get; set; }
    public List<SimpleTravelAdvice> TravelAdvice { get; set; }
}