using komikthuis.Models.API.Disruptions;
using Refit;

namespace komikthuis.Interfaces
{
    public interface INSApi
    {
        [Get("/reisinformatie-api/api/v3/disruptions")]
        Task<List<Disruption>> GetAllDisruptions();

        [Get("/reisinformatie-api/api/v2/stations")]
        Task<StationRoot> GetAllStations();
    }
}