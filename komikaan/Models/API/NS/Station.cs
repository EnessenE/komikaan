namespace komikaan.Models.API.NS
{
    public class Namen
    {
        public string lang { get; set; }
        public string middel { get; set; }
        public string kort { get; set; }
    }

    public class NearbyMeLocationId
    {
        public string value { get; set; }
        public string type { get; set; }
    }

    public class Station
    {
        public string EVACode { get; set; }
        public string UICCode { get; set; }
        public string code { get; set; }
        public string ingangsDatum { get; set; }
        public bool heeftFaciliteiten { get; set; }
        public bool heeftReisassistentie { get; set; }
        public bool heeftVertrektijden { get; set; }
        public string land { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public int radius { get; set; }
        public int naderenRadius { get; set; }
        public Namen namen { get; set; }
        public List<string> synoniemen { get; set; }
        public NearbyMeLocationId nearbyMeLocationId { get; set; }
        public List<object> sporen { get; set; }
        public string stationType { get; set; }
        public int? cdCode { get; set; }
    }

    public class StationRoot
    {
        public List<Station> payload { get; set; }
    }
}
