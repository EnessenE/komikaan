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

            _stubContext.Add(new SimpleDisruption() { Active = true });
            _stubContext.Add(new SimpleTravelAdvice() { ActualDurationInMinutes = 1, PlannedDurationInMinutes = 1, Route = routes, Source = DataSource.NederlandseSpoorwegen });
            var result = await _travelAdviceHandler.GetTravelExpectationAsync("test station1", "test station2");

            Assert.IsNotNull(result, "Some data should return, even if nothing is found");
            result.JourneyExpectation.Should().Be(JourneyExpectation.Full);

        }
    }
}
