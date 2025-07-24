using komikaan.Data.Enums;
using komikaan.GTFS.Models.Static.Enums;

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
                case RouteType.Subway:
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
     }
}
