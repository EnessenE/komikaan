using GTFS.Entities.Enumerations;
using komikaan.Data.Enums;
using komikaan.Data.GTFS;
using komikaan.Data.Models;

namespace komikaan.Extensions
{
    public static class GTFSExtensions
    {
        public static LegType ToLegType(this RouteType type)
        {
            switch (type)
            {
                case RouteType.Tram:
                    {
                        return LegType.Tram;
                    }
                case RouteType.SubwayMetro:
                    {
                        return LegType.Metro;
                    }
                case RouteType.Monorail:
                case RouteType.Rail:
                    {
                        return LegType.Train;
                    }
                case RouteType.Bus:
                    {
                        return LegType.Bus;
                    }
                case RouteType.Ferry:
                    {
                        return LegType.Ferry;
                    }
                default:
                    {
                        return LegType.Unknown;
                    }
            }
        }

        public static SimpleStop ToSimpleStop(this GTFSStop stop)
        {
            return new SimpleStop
            {
                Name = stop.Name
            };
        }
    }
}
