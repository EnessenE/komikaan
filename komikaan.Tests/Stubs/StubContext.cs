using komikaan.Data.Enums;
using komikaan.Data.Models;
using komikaan.Interfaces;
using komikaan.Models;
using komikaan.Models.API.NS;

namespace komikaan.Tests.Stubs
{
    public class StubContext : IDataSupplierContext
    {
        public DataSource Supplier { get; } = DataSource.NederlandseSpoorwegen;

        private IList<SimpleDisruption> _disruptions;
        private IList<SimpleTravelAdvice> _travelAdvices;

        public StubContext()
        {
            _disruptions = new List<SimpleDisruption>();
            _travelAdvices = new List<SimpleTravelAdvice>();
        }

        public void Add(SimpleDisruption disruption)
        {
            _disruptions.Add(disruption);
        }

        public void Remove(SimpleDisruption disruption)
        {
            _disruptions.Remove(disruption);
        }

        public void Add(SimpleTravelAdvice advice)
        {
            _travelAdvices.Add(advice);
        }

        public void Remove(SimpleTravelAdvice advice)
        {
            _travelAdvices.Remove(advice);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task LoadRelevantDataAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<SimpleDisruption>> GetDisruptionsAsync(string from, string to)
        {
            return Task.FromResult(_disruptions.AsEnumerable());
        }

        public Task<IEnumerable<SimpleDisruption>> GetAllDisruptionsAsync(bool active)
        {
            return Task.FromResult(_disruptions.AsEnumerable());
        }

        public Task<IEnumerable<SimpleStop>> GetAllStopsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<string, Station>> GetAllStops()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<SimpleTravelAdvice>> GetTravelAdviceAsync(string from, string to)
        {
            return Task.FromResult(_travelAdvices.AsEnumerable());
        }
    }
}
