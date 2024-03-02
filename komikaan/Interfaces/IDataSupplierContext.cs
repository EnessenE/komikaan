using komikaan.Data.Enums;
using komikaan.Models;
using komikaan.Models.API.NS;

namespace komikaan.Interfaces;

public interface IDataSupplierContext
{
    DataSource Supplier { get; }
    Task StartAsync(CancellationToken cancellationToken);
    Task LoadRelevantDataAsync(CancellationToken cancellationToken);
    Task<IEnumerable<SimpleDisruption>> GetDisruptionsAsync(string from, string to);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1564:Parameter in public or internal member is of type bool or bool?", Justification = "We are selecting data for active / inactive disruptions. This is intended")]
    Task<IEnumerable<SimpleDisruption>> GetAllDisruptions(bool active);
    Task<IDictionary<string, Station>> GetAllStops();
    Task<IEnumerable<SimpleTravelAdvice>> GetTravelAdviceAsync(string from, string to);
}
