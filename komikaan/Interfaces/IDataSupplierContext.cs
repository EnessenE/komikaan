using komikaan.Data.Enums;
using komikaan.Models;
using komikaan.Models.API.NS;

namespace komikaan.Interfaces;

public interface IDataSupplierContext
{
    DataSource Supplier { get; }
    Task LoadRelevantData(CancellationToken cancellationToken);
    Task<IEnumerable<SimpleDisruption>> GetDisruptions(string from, string to);
    Task<IEnumerable<SimpleDisruption>> GetAllDisruptions(bool active);
    Task<IDictionary<string, Station>> GetAllStops();
    Task<IEnumerable<SimpleTravelAdvice>> GetTravelAdvice(string from, string to);
}