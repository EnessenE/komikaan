using komikthuis.Models.API.Disruptions;
using Refit;

namespace komikthuis.Interfaces
{
    public interface INSApi
    {
        [Get("/disruptions/v3/")]
        Task<IEnumerable<Disruption>> GetAllDisruptions();

        [Get("/reisinformatie-api/api/v2/stations")]
        Task<IEnumerable<Station>> GetAllStations();
    }
}
