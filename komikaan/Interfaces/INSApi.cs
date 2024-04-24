using komikaan.Models.API.NS;
using Refit;

namespace komikaan.Interfaces
{
    public interface INSApi
    {
        [Get("/disruptions/v3")]
        Task<List<Disruption>> GetAllDisruptions(CancellationToken cancellationToken);

        [Get("/reisinformatie-api/api/v2/stations")]
        Task<StationRoot> GetAllStations(CancellationToken cancellationToken);

        [Get("/reisinformatie-api/api/v3/trips?fromStation={fromStationCode}&toStation={toStationCode}&originWalk=false&originBike=false&originCar=false&destinationWalk=false&destinationBike=false&destinationCar=false&shorterChange=false&travelAssistance=false&searchForAccessibleTrip=false&localTrainsOnly=false&excludeHighSpeedTrains=false&excludeTrainsWithReservationRequired=false")]
        Task<TravelAdvice> GetRouteAdvice(string fromStationCode, string toStationCode, CancellationToken cancellationToken);
    }
}
