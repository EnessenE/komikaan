using System.Text.Json.Serialization;

namespace komikaan.Models
{
    public class SimpleTravelAdvice
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DataSource Source { get; set; }

        public List<SimpleRoutePart> Route { get; set; }

        public DateTime PlannedDeparture { get; set; }
        public DateTime ActualDeparture { get; set; }
        public int PlannedDurationInMinutes { get; set; }

        public int ActualDurationInMinutes { get; set; }
    }
}
