using System.Text.Json.Serialization;

namespace komikaan.Data.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum LegType
    {
        Unknown,
        Train,
        InternationalTrain,
        Bus,
        Feet,
        Bicycle,
        Metro,
        Ferry
    }
}
