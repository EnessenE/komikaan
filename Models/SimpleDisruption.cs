using System.Text.Json.Serialization;

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
    public IEnumerable<string> Advices { get; set; }
}