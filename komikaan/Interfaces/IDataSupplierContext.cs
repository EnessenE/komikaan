using komikaan.Models;
using komikaan.Models.API.NS;

namespace komikaan.Interfaces;

public interface IDataSupplierContext
{
    Task LoadRelevantData(CancellationToken cancellationToken);
    Task<IEnumerable<SimpleDisruption>> GetDisruptions(string from, string to);
    Task<IDictionary<string, Station>> GetAllStops();
    Task<IEnumerable<SimpleTravelAdvice>> GetTravelAdvice(string from, string to);
}