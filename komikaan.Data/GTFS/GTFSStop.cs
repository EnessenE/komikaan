using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace komikaan.Data.GTFS
{
    public class GTFSStop
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentStation { get; set; }

        public List<string> Operators { get; set; }
        public List<string> DataSuppliers { get; set; }
        public List<GTFSStopTime> Departures { get; set; }
    }
}
