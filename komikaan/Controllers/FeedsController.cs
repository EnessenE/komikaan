using komikaan.Data.GTFS;
using komikaan.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace komikaan.Controllers
{
    [ApiController]
    [Route("v1/feeds")]
    public class FeedsController : ControllerBase
    {
        private readonly ILogger<FeedsController> _logger;
        private readonly IGTFSContext _gtfs;

        public FeedsController(ILogger<FeedsController> logger, IGTFSContext gtfs)
        {
            _logger = logger;
            _gtfs = gtfs;
        }


        [HttpGet]
        public async Task<List<Feed>?> GetTripAsync()
        {
            var feeds = await _gtfs.GetFeedsAsync();
            return feeds?.ToList();
        }
    }
}
