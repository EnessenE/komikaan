using System.Text.Json.Serialization;
using komikthuis.Enums;

namespace komikthuis.Models;

public class SimpleDisruption
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DataSource Source { get; set; }
    public string Title { get; set; }
    public DateTime ExpectedEnd { get; set; }
    public IEnumerable<string> Descriptions { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DisruptionType Type { get; set; }
    public DisruptionStage Stage { get; set; }
    public IEnumerable<string> Advices { get; set; }
}

public enum DisruptionStage
{
    Finished,
    Ongoing
}