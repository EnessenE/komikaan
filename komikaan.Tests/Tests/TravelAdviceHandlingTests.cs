using FluentAssertions;
using komikaan.Controllers;
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
                new TravelAdviceHandler(new NullLogger<TravelAdviceHandler>(), new List<IDataSupplierContext>(){_stubContext});
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
            result.JourneyExpectation.Should().Be(JourneyExpectation.Maybe);

        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(10)]
        [DataRow(50)]
        public async Task SuccesfullCallWithNoRoutes(int amountOfRoutes)
        {
            var stationName1 = "TestStation1";
            var stationName2 = "TestStation2";

            var routes = new List<SimpleRoutePart>();

            for (int i = 0; i < amountOfRoutes; i++)
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
            result.JourneyExpectation.Should().Be(JourneyExpectation.Maybe);

        }
    }
}
