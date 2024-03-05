using System.Text.Json.Serialization;
using komikaan.Data.Enums;
using komikaan.Enums;

namespace komikaan.Models;

public class SimpleDisruption
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DataSource Source { get; set; }
    public string Title { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? ExpectedEnd { get; set; }
    public IEnumerable<string> Descriptions { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DisruptionType Type { get; set; }
    public DisruptionStage Stage { get; set; }
    public IEnumerable<string>? Advices { get; set; }
    public bool Active { get; set; }
    public List<string> AffectedStops { get; set; }

    /// <summary>
    /// URL with possibly more info
    /// </summary>
    public string? Url { get; set; }
}
