using GTFS.Entities.Enumerations;
using komikaan.Data.Enums;

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "AV1500:Member or local function contains too many statements", Justification = "Yep.")]
        public static StopType ConvertStopType(this RouteTypeExtended routeType)
        {
            return routeType switch
            {
                RouteTypeExtended.RailwayService => StopType.Train,
                RouteTypeExtended.RailReplacementBusService => StopType.Train,
                RouteTypeExtended.RailShuttleWithinComplex => StopType.Train,
                RouteTypeExtended.RailTaxiService => StopType.Train,
                RouteTypeExtended.AdditionalRailService => StopType.Train,
                RouteTypeExtended.InterRegionalRailService => StopType.Train,
                RouteTypeExtended.UrbanRailwayService => StopType.Metro,
                RouteTypeExtended.UrbanRailwayServiceDefault => StopType.Metro,
                RouteTypeExtended.HighSpeedRailService => StopType.Train,
                RouteTypeExtended.AllRailServices => StopType.Train,
                RouteTypeExtended.BusService => StopType.Bus,
                RouteTypeExtended.AllBusServices => StopType.Bus,
                RouteTypeExtended.DemandandResponseBusService => StopType.Bus,
                RouteTypeExtended.ExpressBusService => StopType.Bus,
                RouteTypeExtended.LocalBusService => StopType.Bus,
                RouteTypeExtended.NightBusService => StopType.Bus,
                RouteTypeExtended.MobilityBusforRegisteredDisabled => StopType.Bus,
                RouteTypeExtended.PostBusService => StopType.Bus,
                RouteTypeExtended.SightseeingBus => StopType.Bus,
                RouteTypeExtended.InternationalCoachService => StopType.InternationalBus,
                RouteTypeExtended.TramService => StopType.Tram,
                RouteTypeExtended.AllTramServices => StopType.Tram,
                RouteTypeExtended.CityTramService => StopType.Tram,
                RouteTypeExtended.LocalTramService => StopType.Tram,
                RouteTypeExtended.RegionalTramService => StopType.Tram,
                RouteTypeExtended.ShuttleTramService => StopType.Tram,
                RouteTypeExtended.SightseeingTramService => StopType.Tram,
                RouteTypeExtended.WaterTaxiService => StopType.Ferry,
                RouteTypeExtended.WaterTransportService => StopType.Ferry,
                RouteTypeExtended.AllWaterTransportServices => StopType.Ferry,
                RouteTypeExtended.FerryService => StopType.Ferry,
                RouteTypeExtended.AirportLinkFerryService => StopType.Ferry,
                RouteTypeExtended.CarHighSpeedFerryService => StopType.Ferry,
                RouteTypeExtended.InternationalCarFerryService => StopType.Ferry,
                RouteTypeExtended.InternationalPassengerFerryService => StopType.Ferry,
                RouteTypeExtended.LocalCarFerryService => StopType.Ferry,
                RouteTypeExtended.NationalCarFerryService => StopType.Ferry,
                RouteTypeExtended.PassengerHighSpeedFerryService => StopType.Ferry,
                RouteTypeExtended.RegionalCarFerryService => StopType.Ferry,
                RouteTypeExtended.TrainFerryService => StopType.Ferry,
                RouteTypeExtended.ShuttleFerryService => StopType.Ferry,
                RouteTypeExtended.ScheduledFerryService => StopType.Ferry,
                RouteTypeExtended.RoadLinkFerryService => StopType.Ferry,
                RouteTypeExtended.RegionalPassengerFerryService => StopType.Ferry,
                RouteTypeExtended.LongDistanceTrains => StopType.Train,
                RouteTypeExtended.CarTransportRailService => StopType.Train,
                RouteTypeExtended.SleeperRailService => StopType.Train,
                RouteTypeExtended.RegionalRailService => StopType.Train,
                RouteTypeExtended.TouristRailwayService => StopType.Train,
                RouteTypeExtended.SuburbanRailway => StopType.Train,
                RouteTypeExtended.ReplacementRailService => StopType.Train,
                RouteTypeExtended.SpecialRailService => StopType.Train,
                RouteTypeExtended.LorryTransportRailService => StopType.Train,
                RouteTypeExtended.CrossCountryRailService => StopType.Train,
                RouteTypeExtended.VehicleTransportRailService => StopType.Train,
                RouteTypeExtended.RackandPinionRailway => StopType.Train,
                RouteTypeExtended.CoachService => StopType.Bus,
                RouteTypeExtended.NationalCoachService => StopType.Bus,
                RouteTypeExtended.ShuttleCoachService => StopType.Bus,
                RouteTypeExtended.RegionalCoachService => StopType.Bus,
                RouteTypeExtended.SpecialCoachService => StopType.Bus,
                RouteTypeExtended.SightseeingCoachService => StopType.Bus,
                RouteTypeExtended.TouristCoachService => StopType.Bus,
                RouteTypeExtended.CommuterCoachService => StopType.Bus,
                RouteTypeExtended.AllCoachServices => StopType.Bus,
                RouteTypeExtended.SuburbanRailwayService => StopType.Train,
                RouteTypeExtended.MetroCoachService => StopType.Metro,
                RouteTypeExtended.UndergroundService => StopType.Metro,
                RouteTypeExtended.AllUrbanRailwayServices => StopType.Metro,
                RouteTypeExtended.Monorail => StopType.Monorail,
                RouteTypeExtended.MetroService => StopType.Metro,
                RouteTypeExtended.UndergroundMetroService => StopType.Metro,
                RouteTypeExtended.RegionalBusService => StopType.Bus,
                RouteTypeExtended.StoppingBusService => StopType.Bus,
                RouteTypeExtended.SpecialNeedsBus => StopType.Bus,
                RouteTypeExtended.MobilityBusService => StopType.Bus,
                RouteTypeExtended.ShuttleBus => StopType.Bus,
                RouteTypeExtended.SchoolBus => StopType.Bus,
                RouteTypeExtended.SchoolandPublicServiceBus => StopType.Bus,
                RouteTypeExtended.ShareTaxiService => StopType.Bus,
                RouteTypeExtended.TrolleybusService => StopType.Bus,
                RouteTypeExtended.NationalPassengerFerryService => StopType.Ferry,
                RouteTypeExtended.LocalPassengerFerryService => StopType.Ferry,
                RouteTypeExtended.PostBoatService => StopType.Ferry,
                RouteTypeExtended.SightseeingBoatService => StopType.Ferry,
                RouteTypeExtended.SchoolBoat => StopType.Ferry,
                RouteTypeExtended.CableDrawnBoatService => StopType.Ferry,
                RouteTypeExtended.RiverBusService => StopType.Ferry,
                RouteTypeExtended.AirService => StopType.AirTransport,
                RouteTypeExtended.InternationalAirService => StopType.AirTransport,
                RouteTypeExtended.DomesticAirService => StopType.AirTransport,
                RouteTypeExtended.IntercontinentalAirService => StopType.AirTransport,
                RouteTypeExtended.DomesticScheduledAirService => StopType.AirTransport,
                RouteTypeExtended.ShuttleAirService => StopType.AirTransport,
                RouteTypeExtended.IntercontinentalCharterAirService => StopType.AirTransport,
                RouteTypeExtended.InternationalCharterAirService => StopType.AirTransport,
                RouteTypeExtended.RoundTripCharterAirService => StopType.AirTransport,
                RouteTypeExtended.SightseeingAirService => StopType.AirTransport,
                RouteTypeExtended.HelicopterAirService => StopType.AirTransport,
                RouteTypeExtended.DomesticCharterAirService => StopType.AirTransport,
                RouteTypeExtended.SchengenAreaAirService => StopType.AirTransport,
                RouteTypeExtended.AirshipService => StopType.AirTransport,
                RouteTypeExtended.AllAirServices => StopType.AirTransport,
                RouteTypeExtended.TelecabinService => StopType.CableCar,
                RouteTypeExtended.TelecabinServiceDefault => StopType.CableCar,
                RouteTypeExtended.CableCarService => StopType.CableCar,
                RouteTypeExtended.ElevatorService => StopType.CableCar,
                RouteTypeExtended.ChairLiftService => StopType.CableCar,
                RouteTypeExtended.DragLiftService => StopType.CableCar,
                RouteTypeExtended.SmallTelecabinService => StopType.Other,
                RouteTypeExtended.AllTelecabinServices => StopType.CableCar,
                RouteTypeExtended.FunicularService => StopType.CableCar,
                RouteTypeExtended.FunicularServiceDefault => StopType.CableCar,
                RouteTypeExtended.AllFunicularService => StopType.CableCar,
                RouteTypeExtended.TaxiService => StopType.Bus,
                RouteTypeExtended.CommunalTaxiService => StopType.Bus,
                RouteTypeExtended.BikeTaxiService => StopType.Bus,
                RouteTypeExtended.LicensedTaxiService => StopType.Bus,
                RouteTypeExtended.PrivateHireServiceVehicle => StopType.Bus,
                RouteTypeExtended.AllTaxiServices => StopType.Bus,
                RouteTypeExtended.SelfDrive => StopType.Bus,
                RouteTypeExtended.HireCar => StopType.Unknown,
                RouteTypeExtended.HireVan => StopType.Unknown,
                RouteTypeExtended.HireMotorbike => StopType.Unknown,
                RouteTypeExtended.HireCycle => StopType.Unknown,
                RouteTypeExtended.MiscellaneousService => StopType.Other,
                RouteTypeExtended.CableCar => StopType.CableCar,
                RouteTypeExtended.HorsedrawnCarriage => StopType.Bus,
                _ => StopType.Unknown,
            };
        }


    }
}
