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
        public string Code { get; set; }
        public string Name { get; set; }
        public string ParentStation { get; set; }
    }
}
