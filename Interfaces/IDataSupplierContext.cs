using komikthuis.Models;
using komikthuis.Models.API.Disruptions;

namespace komikthuis.Interfaces;

public interface IDataSupplierContext
{
    Task StartAsync(CancellationToken cancellationToken);
    Task<IEnumerable<SimpleDisruption>> GetDisruptions(string from, string to);
    Task<IDictionary<string, Station>> GetAllStops();
    Task<IEnumerable<SimpleTravelAdvice>> GetTravelAdvice(string from, string to);
}