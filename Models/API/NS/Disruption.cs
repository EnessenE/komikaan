namespace komikthuis.Models.API.Disruptions
{
    public class AdditionalTravelTime
    {
        public string label { get; set; }
        public string shortLabel { get; set; }
        public int maximumDurationInMinutes { get; set; }
        public int? minimumDurationInMinutes { get; set; }
    }

    public class AlternativeTransport
    {
        public string label { get; set; }
        public string shortLabel { get; set; }
        public List<Location> location { get; set; }
    }

    public class AlternativeTransportTimespan
    {
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public AlternativeTransport alternativeTransport { get; set; }
    }

    public class Cause
    {
        public string label { get; set; }
    }

    public class Consequence
    {
        public Section section { get; set; }
        public string description { get; set; }
        public string level { get; set; }
    }

    public class Coordinate
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class Impact
    {
        public int value { get; set; }
    }

    public class Location
    {
        public Station station { get; set; }
        public string description { get; set; }
    }

    public class PublicationSection
    {
        public Section section { get; set; }
        public Consequence consequence { get; set; }
        public string sectionType { get; set; }
    }

    public class Disruption
    {
        public string id { get; set; }
        public object titleSections { get; set; }
        public bool isActive { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public DateTime registrationTime { get; set; }
        public DateTime releaseTime { get; set; }
        public bool local { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public string period { get; set; }
        public Impact impact { get; set; }
        public List<PublicationSection> publicationSections { get; set; }
        public List<NSTimespan> timespans { get; set; }
        public List<AlternativeTransportTimespan> alternativeTransportTimespans { get; set; }
        public SummaryAdditionalTravelTime summaryAdditionalTravelTime { get; set; }
    }

    public class Section
    {
        public List<DisruptionStation> stations { get; set; }
        public string direction { get; set; }
    }

    public class Situation
    {
        public string label { get; set; }
    }

    public class DisruptionStation
    {
        public Coordinate coordinate { get; set; }
        public string countryCode { get; set; }
        public string name { get; set; }
        public string stationCode { get; set; }
        public string uicCode { get; set; }
    }

    public class Station3
    {
        public Coordinate coordinate { get; set; }
        public string countryCode { get; set; }
        public string name { get; set; }
        public string stationCode { get; set; }
        public string uicCode { get; set; }
    }

    public class SummaryAdditionalTravelTime
    {
        public string label { get; set; }
        public string shortLabel { get; set; }
        public int maximumDurationInMinutes { get; set; }
        public int? minimumDurationInMinutes { get; set; }
    }

    public class NSTimespan
    {
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public string period { get; set; }
        public Situation situation { get; set; }
        public Cause cause { get; set; }
        public AdditionalTravelTime additionalTravelTime { get; set; }
        public AlternativeTransport alternativeTransport { get; set; }
        public List<string> advices { get; set; }
    }
}
