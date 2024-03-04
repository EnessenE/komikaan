using komikaan.Data.Enums;
using komikaan.Data.Models;
using komikaan.Models;
using komikaan.Models.API.NS;

namespace komikaan.Interfaces;

public interface IDataSupplierContext
{
    DataSource Supplier { get; }
    Task StartAsync(CancellationToken cancellationToken);
    Task LoadRelevantDataAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Responsible for finding all relevant disruptions on the route
    /// </summary>
    /// <param name="from">Departure station</param>
    /// <param name="to">Arrival station</param>
    /// <returns>A list of disruptions that are active and affect the stations and the route inbetween</returns>
    Task<IEnumerable<SimpleDisruption>> GetDisruptionsAsync(string from, string to);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1564:Parameter in public or internal member is of type bool or bool?", Justification = "We are selecting data for active / inactive disruptions. This is intended")]
    Task<IEnumerable<SimpleDisruption>> GetAllDisruptionsAsync(bool active);
    Task<IEnumerable<SimpleStop>> GetAllStopsAsync();
    Task<IEnumerable<SimpleTravelAdvice>> GetTravelAdviceAsync(string from, string to);
}
