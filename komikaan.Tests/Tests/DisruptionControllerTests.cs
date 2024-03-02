using FluentAssertions;
using komikaan.Controllers;
using komikaan.Data.Enums;
using komikaan.Enums;
using komikaan.Models;
using komikaan.Tests.Stubs;

namespace komikaan.Tests.Tests
{
    [TestClass]
    public class DisruptionControllerTests
    {
        private static TestContext _testContext;

        private DisruptionController _disruptionController;
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

            _disruptionController =
                new DisruptionController(new MsTestLogger<DisruptionController>(_testContext), _stubContext);
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
                Name = stationName1
            });
            routes.Add(new SimpleRoutePart()
            {
                PlannedDeparture = DateTime.UtcNow,
                Cancelled = false,
                Name = stationName2
            });

            _stubContext.Add(new SimpleDisruption() { Active = true });
            _stubContext.Add(new SimpleTravelAdvice() { ActualDurationInMinutes = 1, PlannedDurationInMinutes = 1, Route = routes, Source = DataSource.NederlandseSpoorwegen });
            var result = await _disruptionController.GetTravelExpectationAsync("test station1", "test station2");

            Assert.IsNotNull(result, "Some data should return, even if nothing is found");
            result.JourneyExpectation.Should().Be(JourneyExpectation.Full);

        }
    }
}
