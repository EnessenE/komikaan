using komikthuis.Models.API.Disruptions;
using komikthuis.Models.API.NS;
using Refit;

namespace komikthuis.Interfaces
{
    public interface INSApi
    {
        [Get("/reisinformatie-api/api/v3/disruptions")]
        Task<List<Disruption>> GetAllDisruptions();

        [Get("/reisinformatie-api/api/v2/stations")]
        Task<StationRoot> GetAllStations();

        [Get("/reisinformatie-api/api/v3/trips?fromStation={fromStationCode}&toStation={toStationCode}&originWalk=false&originBike=false&originCar=false&destinationWalk=false&destinationBike=false&destinationCar=false&shorterChange=false&travelAssistance=false&searchForAccessibleTrip=false&localTrainsOnly=false&excludeHighSpeedTrains=false&excludeTrainsWithReservationRequired=false")]
        Task<TravelAdvice> GetRouteAdvice(string fromStationCode, string toStationCode);
    }
}