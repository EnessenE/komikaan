using FluentAssertions;
using komikaan.Data.Enums;
using komikaan.Enums;
using komikaan.Handlers;
using komikaan.Interfaces;
using komikaan.Models;
using komikaan.Tests.Stubs;
using Microsoft.Extensions.Logging.Abstractions;

namespace komikaan.Tests.Tests
{
    [TestClass]
    public class TravelAdviceHandlingTests
    {
        private static TestContext _testContext;

        private ITravelAdviceHandler _travelAdviceHandler;
        private StubContext _stubContext;

        [ClassInitialize]
        public static void SetupTests(TestContext testContext)
        {
            _testContext = testContext;
        }


        [TestInitialize]
        public void Init()
        {
            _stubContext = new StubContext();

            _travelAdviceHandler =
                new TravelAdviceHandler(new NullLogger<TravelAdviceHandler>(), new List<IDataSupplierContext>() { _stubContext });
        }

        [TestMethod]
        public async Task SuccesfullCallWithGoodRoute()
        {
            var stationName1 = "TestStation1";
            var stationName2 = "TestStation2";

            var routes = new List<SimpleRoutePart>();

            routes.Add(new SimpleRoutePart()
            {
                PlannedDeparture = DateTime.UtcNow,
                Cancelled = false,
                DepartureStation = stationName1,
                ArrivalStation = stationName2
            });

            _stubContext.Add(new SimpleDisruption() { Active = true, Type = DisruptionType.Unknown });
            _stubContext.Add(new SimpleTravelAdvice() { ActualDurationInMinutes = 1, PlannedDurationInMinutes = 1, Route = routes, Source = DataSource.NederlandseSpoorwegen });
            var result = await _travelAdviceHandler.GetTravelExpectationAsync("test station1", "test station2");

            Assert.IsNotNull(result, "Some data should return, even if nothing is found");
            result.JourneyExpectation.Should().Be(JourneyExpectation.Full);
        }

        [TestMethod]
        public async Task SuccesfullCallWithCalamity()
        {
            var stationName1 = "TestStation1";
            var stationName2 = "TestStation2";

            var routes = new List<SimpleRoutePart>();

            routes.Add(new SimpleRoutePart()
            {
                PlannedDeparture = DateTime.UtcNow,
                Cancelled = false,
                DepartureStation = stationName1,
                ArrivalStation = stationName2
            });

            _stubContext.Add(new SimpleDisruption() { Active = true, Type = DisruptionType.Calamity });
            _stubContext.Add(new SimpleTravelAdvice() { ActualDurationInMinutes = 1, PlannedDurationInMinutes = 1, Route = routes, Source = DataSource.NederlandseSpoorwegen });
            var result = await _travelAdviceHandler.GetTravelExpectationAsync("test station1", "test station2");

            Assert.IsNotNull(result, "Some data should return, even if nothing is found");
            result.JourneyExpectation.Should().Be(JourneyExpectation.Full, "as a calamity doesn't mean the route is unusable");

        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(10)]
        [DataRow(50)]
        public async Task SuccesfullCallWithNoRoutes(int amountOfRouteParts)
        {
            var stationName1 = "TestStation1";
            var stationName2 = "TestStation2";

            var routes = new List<SimpleRoutePart>();

            for (int i = 0; i < amountOfRouteParts; i++)
            {
                routes.Add(new SimpleRoutePart()
                {
                    PlannedDeparture = DateTime.UtcNow,
                    Cancelled = true,
                    DepartureStation = stationName1,
                    ArrivalStation = stationName2
                });
            }

            _stubContext.Add(new SimpleDisruption() { Active = true, Type = DisruptionType.Calamity });
            _stubContext.Add(new SimpleTravelAdvice() { ActualDurationInMinutes = 1, PlannedDurationInMinutes = 1, Route = routes, Source = DataSource.NederlandseSpoorwegen });
            var result = await _travelAdviceHandler.GetTravelExpectationAsync("test station1", "test station2");

            Assert.IsNotNull(result, "Some data should return, even if nothing is found");
            result.JourneyExpectation.Should().Be(JourneyExpectation.Nope);
        }


        [DataTestMethod]
        [DataRow(1)]
        [DataRow(10)]
        [DataRow(50)]
        public async Task SuccesfullCallWithAllCancelledRoutes(int amountOfRouteParts)
        {
            var stationName1 = "TestStation1";
            var stationName2 = "TestStation2";

            var routes = new List<SimpleRoutePart>();

            for (int i = 0; i < amountOfRouteParts; i++)
            {
                routes.Add(new SimpleRoutePart()
                {
                    PlannedDeparture = DateTime.UtcNow,
                    Cancelled = true,
                    DepartureStation = stationName1,
                    ArrivalStation = stationName2
                });
            }

            _stubContext.Add(new SimpleTravelAdvice() { ActualDurationInMinutes = 1, PlannedDurationInMinutes = 1, Route = routes, Source = DataSource.NederlandseSpoorwegen });
            var result = await _travelAdviceHandler.GetTravelExpectationAsync("test station1", "test station2");

            Assert.IsNotNull(result, "Some data should return, even if nothing is found");
            result.JourneyExpectation.Should().Be(JourneyExpectation.Nope);
        }



        [DataTestMethod]
        [DataRow(2)]
        [DataRow(10)]
        [DataRow(50)]
        public async Task HalfCancelledRoutes(int amountOfRouteParts)
        {
            var stationName1 = "TestStation1";
            var stationName2 = "TestStation2";

            var routes = new List<SimpleRoutePart>();

            for (int i = 0; i < amountOfRouteParts / 2; i++)
            {
                routes.Add(new SimpleRoutePart()
                {
                    PlannedDeparture = DateTime.UtcNow,
                    Cancelled = false,
                    DepartureStation = stationName1,
                    ArrivalStation = stationName2
                });
            }

            for (int i = 0; i < amountOfRouteParts / 2; i++)
            {
                routes.Add(new SimpleRoutePart()
                {
                    PlannedDeparture = DateTime.UtcNow,
                    Cancelled = true,
                    DepartureStation = stationName1,
                    ArrivalStation = stationName2
                });
            }

            _stubContext.Add(new SimpleTravelAdvice() { ActualDurationInMinutes = 1, PlannedDurationInMinutes = 1, Route = routes, Source = DataSource.NederlandseSpoorwegen });
            var result = await _travelAdviceHandler.GetTravelExpectationAsync("test station1", "test station2");

            Assert.IsNotNull(result, "Some data should return, even if nothing is found");
            result.JourneyExpectation.Should().Be(JourneyExpectation.Nope, "As this ends up having no valid routes");
        }


        [DataTestMethod]
        [DataRow(0, 0, 0, JourneyExpectation.Unknown)]
        [DataRow(1, 0, 1, JourneyExpectation.Full)]
        [DataRow(10, 0, 1, JourneyExpectation.Full)]
        [DataRow(10, 1, 1, JourneyExpectation.Full)]
        [DataRow(10, 3, 1, JourneyExpectation.Full)]
        [DataRow(0, 1, 1, JourneyExpectation.Nope)]
        [DataRow(1, 1, 1, JourneyExpectation.Maybe)]
        [DataRow(1, 1, 2, JourneyExpectation.Maybe)]
        [DataRow(2, 2, 1, JourneyExpectation.Maybe)]
        [DataRow(10, 10, 10, JourneyExpectation.Maybe)]
        [DataRow(50, 50, 50, JourneyExpectation.Maybe)]
        public async Task RouteMixing(int amountOfGoodAdvices, int amountOfBadAdvices, int amountOfRouteParts, JourneyExpectation journeyExpectation)
        {
            var stationName1 = "TestStation1";
            var stationName2 = "TestStation2";

            var badRoutes = new List<SimpleRoutePart>();
            var goodRoutes = new List<SimpleRoutePart>();

            for (int i = 0; i < amountOfRouteParts; i++)
            {
                badRoutes.Add(new SimpleRoutePart()
                {
                    PlannedDeparture = DateTime.UtcNow,
                    Cancelled = false,
                    DepartureStation = stationName1,
                    ArrivalStation = stationName2
                });
                badRoutes.Add(new SimpleRoutePart()
                {
                    PlannedDeparture = DateTime.UtcNow,
                    Cancelled = true,
                    DepartureStation = stationName1,
                    ArrivalStation = stationName2
                });
            }

            for (int i = 0; i < amountOfRouteParts; i++)
            {
                goodRoutes.Add(new SimpleRoutePart()
                {
                    PlannedDeparture = DateTime.UtcNow,
                    Cancelled = false,
                    DepartureStation = stationName1,
                    ArrivalStation = stationName2
                });
                goodRoutes.Add(new SimpleRoutePart()
                {
                    PlannedDeparture = DateTime.UtcNow,
                    Cancelled = false,
                    DepartureStation = stationName1,
                    ArrivalStation = stationName2
                });
            }

            for (int i = 0; i < amountOfGoodAdvices; i++)
            {
                _stubContext.Add(new SimpleTravelAdvice() { ActualDurationInMinutes = 1, PlannedDurationInMinutes = 1, Route = goodRoutes, Source = DataSource.NederlandseSpoorwegen });
            }
            for (int i = 0; i < amountOfBadAdvices; i++)
            {
                _stubContext.Add(new SimpleTravelAdvice() { ActualDurationInMinutes = 1, PlannedDurationInMinutes = 1, Route = badRoutes, Source = DataSource.NederlandseSpoorwegen });
            }
            var result = await _travelAdviceHandler.GetTravelExpectationAsync("test station1", "test station2");

            Assert.IsNotNull(result, "Some data should return, even if nothing is found");
            result.JourneyExpectation.Should().Be(journeyExpectation);
        }



        [DataTestMethod]
        [DataRow(0, 0, 0, 0, JourneyExpectation.Unknown, DisruptionType.Maintenance)]
        [DataRow(1, 0, 1, 1, JourneyExpectation.Full, DisruptionType.Maintenance)]
        [DataRow(10, 0, 1, 1, JourneyExpectation.Full, DisruptionType.Maintenance)]
        [DataRow(10, 1, 1, 1, JourneyExpectation.Full, DisruptionType.Maintenance)]
        [DataRow(10, 3, 1, 1, JourneyExpectation.Full, DisruptionType.Maintenance)]
        [DataRow(0, 1, 1, 1, JourneyExpectation.Nope, DisruptionType.Maintenance)]
        [DataRow(1, 1, 1, 1, JourneyExpectation.Maybe, DisruptionType.Maintenance)]
        [DataRow(1, 1, 2, 1, JourneyExpectation.Maybe, DisruptionType.Maintenance)]
        [DataRow(2, 2, 1, 1, JourneyExpectation.Maybe, DisruptionType.Maintenance)]
        [DataRow(10, 10, 10, 1, JourneyExpectation.Maybe, DisruptionType.Maintenance)]
        [DataRow(50, 50, 50, 1, JourneyExpectation.Maybe, DisruptionType.Maintenance)]
        [DataRow(0, 0, 0, 0, JourneyExpectation.Unknown, DisruptionType.Disruption)]
        [DataRow(1, 0, 1, 1, JourneyExpectation.Maybe, DisruptionType.Disruption)]
        [DataRow(10, 0, 1, 1, JourneyExpectation.Maybe, DisruptionType.Disruption)]
        [DataRow(10, 1, 1, 1, JourneyExpectation.Maybe, DisruptionType.Disruption)]
        [DataRow(10, 3, 1, 1, JourneyExpectation.Maybe, DisruptionType.Disruption)]
        [DataRow(0, 1, 1, 1, JourneyExpectation.Nope, DisruptionType.Disruption)]
        [DataRow(1, 1, 1, 1, JourneyExpectation.Maybe, DisruptionType.Disruption)]
        [DataRow(1, 1, 2, 1, JourneyExpectation.Maybe, DisruptionType.Disruption)]
        [DataRow(2, 2, 1, 1, JourneyExpectation.Maybe, DisruptionType.Disruption)]
        [DataRow(10, 10, 10, 1, JourneyExpectation.Maybe, DisruptionType.Disruption)]
        [DataRow(50, 50, 50, 1, JourneyExpectation.Maybe, DisruptionType.Disruption)]
        [DataRow(0, 0, 0, 0, JourneyExpectation.Unknown, DisruptionType.Calamity)]
        [DataRow(1, 0, 1, 1, JourneyExpectation.Full, DisruptionType.Calamity)]
        [DataRow(10, 0, 1, 1, JourneyExpectation.Full, DisruptionType.Calamity)]
        [DataRow(10, 1, 1, 1, JourneyExpectation.Full, DisruptionType.Calamity)]
        [DataRow(10, 3, 1, 1, JourneyExpectation.Full, DisruptionType.Calamity)]
        [DataRow(0, 1, 1, 1, JourneyExpectation.Nope, DisruptionType.Calamity)]
        [DataRow(1, 1, 1, 1, JourneyExpectation.Maybe, DisruptionType.Calamity)]
        [DataRow(1, 1, 2, 1, JourneyExpectation.Maybe, DisruptionType.Calamity)]
        [DataRow(2, 2, 1, 1, JourneyExpectation.Maybe, DisruptionType.Calamity)]
        [DataRow(10, 10, 10, 1, JourneyExpectation.Maybe, DisruptionType.Calamity)]
        [DataRow(50, 50, 50, 1, JourneyExpectation.Maybe, DisruptionType.Calamity)]
        public async Task DisruptionsAndRouteMixing(int amountOfGoodAdvices, int amountOfBadAdvices, int amountOfRouteParts, int amountOfDisruptions, JourneyExpectation journeyExpectation, DisruptionType type)
        {
            var stationName1 = "TestStation1";
            var stationName2 = "TestStation2";

            var badRoutes = new List<SimpleRoutePart>();
            var goodRoutes = new List<SimpleRoutePart>();

            for (int i = 0; i < amountOfRouteParts; i++)
            {
                badRoutes.Add(new SimpleRoutePart()
                {
                    PlannedDeparture = DateTime.UtcNow,
                    Cancelled = false,
                    DepartureStation = stationName1,
                    ArrivalStation = stationName2
                });
                badRoutes.Add(new SimpleRoutePart()
                {
                    PlannedDeparture = DateTime.UtcNow,
                    Cancelled = true,
                    DepartureStation = stationName1,
                    ArrivalStation = stationName2
                });
            }

            for (int i = 0; i < amountOfRouteParts; i++)
            {
                goodRoutes.Add(new SimpleRoutePart()
                {
                    PlannedDeparture = DateTime.UtcNow,
                    Cancelled = false,
                    DepartureStation = stationName1,
                    ArrivalStation = stationName2
                });
                goodRoutes.Add(new SimpleRoutePart()
                {
                    PlannedDeparture = DateTime.UtcNow,
                    Cancelled = false,
                    DepartureStation = stationName1,
                    ArrivalStation = stationName2
                });
            }

            for (int i = 0; i < amountOfDisruptions; i++)
            {
                _stubContext.Add(new SimpleDisruption() { Active = true, ExpectedEnd = DateTime.UtcNow.AddDays(1), Type = type });
            }

            for (int i = 0; i < amountOfGoodAdvices; i++)
            {
                _stubContext.Add(new SimpleTravelAdvice() { ActualDurationInMinutes = 1, PlannedDurationInMinutes = 1, Route = goodRoutes, Source = DataSource.NederlandseSpoorwegen });
            }
            for (int i = 0; i < amountOfBadAdvices; i++)
            {
                _stubContext.Add(new SimpleTravelAdvice() { ActualDurationInMinutes = 1, PlannedDurationInMinutes = 1, Route = badRoutes, Source = DataSource.NederlandseSpoorwegen });
            }
            var result = await _travelAdviceHandler.GetTravelExpectationAsync("test station1", "test station2");

            Assert.IsNotNull(result, "Some data should return, even if nothing is found");
            result.JourneyExpectation.Should().Be(journeyExpectation);
        }
    }
}
