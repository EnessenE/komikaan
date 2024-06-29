using komikaan.Data.Models;

namespace komikaan.Interfaces
{
    public interface IStopManagerService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task<IEnumerable<SimpleStop>> FindStopsAsync(string text);
    }
}
