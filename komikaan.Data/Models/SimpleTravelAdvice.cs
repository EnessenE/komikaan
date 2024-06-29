using System.Text.Json.Serialization;
using komikaan.Data.Enums;

namespace komikaan.Models
{
    public class SimpleTravelAdvice
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DataSource Source { get; set; }

        public List<SimpleRoutePart> Route { get; set; }

        public double PlannedDurationInMinutes { get; set; }

        public double? ActualDurationInMinutes { get; set; }
    }
}
