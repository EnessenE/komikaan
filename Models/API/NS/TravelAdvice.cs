namespace komikthuis.Models.API.NS
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Destination
    {
        public string name { get; set; }
        public double lng { get; set; }
        public double lat { get; set; }
        public string countryCode { get; set; }
        public string uicCode { get; set; }
        public string stationCode { get; set; }
        public string type { get; set; }
        public int plannedTimeZoneOffset { get; set; }
        public DateTime plannedDateTime { get; set; }
        public string plannedTrack { get; set; }
        public string actualTrack { get; set; }
        public string exitSide { get; set; }
        public string checkinStatus { get; set; }
        public List<object> notes { get; set; }
        public int varCode { get; set; }
    }

    public class Fare
    {
        public int priceInCents { get; set; }
        public string product { get; set; }
        public string travelClass { get; set; }
        public string discountType { get; set; }
        public int priceInCentsExcludingSupplement { get; set; }
        public int supplementInCents { get; set; }
        public int buyableTicketSupplementPriceInCents { get; set; }
    }

    public class FareLeg
    {
        public Origin origin { get; set; }
        public Destination destination { get; set; }
        public string @operator { get; set; }
        public List<string> productTypes { get; set; }
        public List<Fare> fares { get; set; }
    }

    public class FareOptions
    {
        public bool isInternationalBookable { get; set; }
        public bool isInternational { get; set; }
        public bool isEticketBuyable { get; set; }
        public bool isPossibleWithOvChipkaart { get; set; }
        public bool isTotalPriceUnknown { get; set; }
    }

    public class FareRoute
    {
        public string routeId { get; set; }
        public Origin origin { get; set; }
        public Destination destination { get; set; }
    }

    public class IconNesProperties
    {
        public string color { get; set; }
        public string icon { get; set; }
    }

    public class JourneyDetail
    {
        public string type { get; set; }
        public Link link { get; set; }
    }

    public class Leg
    {
        public string idx { get; set; }
        public string name { get; set; }
        public string travelType { get; set; }
        public string direction { get; set; }
        public bool partCancelled { get; set; }
        public bool cancelled { get; set; }
        public bool changePossible { get; set; }
        public bool alternativeTransport { get; set; }
        public string journeyDetailRef { get; set; }
        public Origin origin { get; set; }
        public Destination destination { get; set; }
        public Product product { get; set; }
        public List<Stop> stops { get; set; }
        public string crowdForecast { get; set; }
        public bool shorterStock { get; set; }
        public List<JourneyDetail> journeyDetail { get; set; }
        public bool reachable { get; set; }
        public int plannedDurationInMinutes { get; set; }
        public NesProperties nesProperties { get; set; }
        public int? bicycleSpotCount { get; set; }
    }

    public class Link
    {
        public string uri { get; set; }
    }

    public class ModalityListItem
    {
        public string name { get; set; }
        public NameNesProperties nameNesProperties { get; set; }
        public IconNesProperties iconNesProperties { get; set; }
        public string actualTrack { get; set; }
        public string accessibilityName { get; set; }
    }

    public class NameNesProperties
    {
        public string color { get; set; }
        public Styles styles { get; set; }
    }

    public class NesProperties
    {
        public string color { get; set; }
        public string scope { get; set; }
        public Styles styles { get; set; }
    }

    public class NsiLink
    {
        public string url { get; set; }
        public bool showInternationalBanner { get; set; }
    }

    public class Origin
    {
        public string name { get; set; }
        public double lng { get; set; }
        public double lat { get; set; }
        public string countryCode { get; set; }
        public string uicCode { get; set; }
        public string stationCode { get; set; }
        public string type { get; set; }
        public int plannedTimeZoneOffset { get; set; }
        public DateTime plannedDateTime { get; set; }
        public string plannedTrack { get; set; }
        public string actualTrack { get; set; }
        public string checkinStatus { get; set; }
        public List<object> notes { get; set; }
        public int varCode { get; set; }
    }

    public class Product
    {
        public string number { get; set; }
        public string categoryCode { get; set; }
        public string shortCategoryName { get; set; }
        public string longCategoryName { get; set; }
        public string operatorCode { get; set; }
        public string operatorName { get; set; }
        public int operatorAdministrativeCode { get; set; }
        public string type { get; set; }
        public string displayName { get; set; }
        public NameNesProperties nameNesProperties { get; set; }
        public IconNesProperties iconNesProperties { get; set; }
        public List<List<object>> notes { get; set; }
    }

    public class ProductFare
    {
        public int priceInCents { get; set; }
        public int priceInCentsExcludingSupplement { get; set; }
        public int buyableTicketPriceInCents { get; set; }
        public int buyableTicketPriceInCentsExcludingSupplement { get; set; }
        public string product { get; set; }
        public string travelClass { get; set; }
        public string discountType { get; set; }
    }

    public class RegisterJourney
    {
        public string url { get; set; }
        public string searchUrl { get; set; }
        public string status { get; set; }
        public bool bicycleReservationRequired { get; set; }
    }

    public class TravelAdvice
    {
        public string source { get; set; }
        public List<Trip> trips { get; set; }
        public string scrollRequestBackwardContext { get; set; }
        public string scrollRequestForwardContext { get; set; }
    }

    public class ShareUrl
    {
        public string uri { get; set; }
    }

    public class Stop
    {
        public string uicCode { get; set; }
        public string name { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public string countryCode { get; set; }
        public List<object> notes { get; set; }
        public int routeIdx { get; set; }
        public DateTime plannedDepartureDateTime { get; set; }
        public int plannedDepartureTimeZoneOffset { get; set; }
        public string actualDepartureTrack { get; set; }
        public string plannedDepartureTrack { get; set; }
        public string plannedArrivalTrack { get; set; }
        public string actualArrivalTrack { get; set; }
        public bool cancelled { get; set; }
        public bool borderStop { get; set; }
        public bool passing { get; set; }
        public DateTime? plannedArrivalDateTime { get; set; }
        public int? plannedArrivalTimeZoneOffset { get; set; }
    }

    public class Styles
    {
        public string type { get; set; }
        public bool dashed { get; set; }
        public bool strikethrough { get; set; }
        public bool bold { get; set; }
    }

    public class Trip
    {
        public int idx { get; set; }
        public string uid { get; set; }
        public string ctxRecon { get; set; }
        public string sourceCtxRecon { get; set; }
        public int plannedDurationInMinutes { get; set; }
        public int actualDurationInMinutes { get; set; }
        public int transfers { get; set; }
        public string status { get; set; }
        public List<object> messages { get; set; }
        public List<Leg> legs { get; set; }
        public string checksum { get; set; }
        public string crowdForecast { get; set; }
        public bool optimal { get; set; }
        public FareRoute fareRoute { get; set; }
        public List<Fare> fares { get; set; }
        public List<FareLeg> fareLegs { get; set; }
        public ProductFare productFare { get; set; }
        public FareOptions fareOptions { get; set; }
        public NsiLink nsiLink { get; set; }
        public string type { get; set; }
        public ShareUrl shareUrl { get; set; }
        public bool realtime { get; set; }
        public string routeId { get; set; }
        public RegisterJourney registerJourney { get; set; }
        public List<ModalityListItem> modalityListItems { get; set; }
    }


}
