using komikaan.Models;

namespace komikaan.Interfaces
{
    public interface ITravelAdviceHandler
    {
        Task<JourneyResult> GetTravelExpectationAsync(string fromStop, string toStop);
    }
}
