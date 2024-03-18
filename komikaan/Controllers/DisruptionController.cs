using System.Diagnostics;
using komikaan.Enums;
using komikaan.Interfaces;
using komikaan.Models;
using Microsoft.AspNetCore.Mvc;

namespace komikaan.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class DisruptionController : ControllerBase
    {
        private readonly ITravelAdviceHandler _travelAdviceHandler;

        public DisruptionController(ITravelAdviceHandler travelAdviceHandler)
        {
            _travelAdviceHandler = travelAdviceHandler;
        }

        [HttpGet("{fromStop}/{toStop}")]
        public async Task<JourneyResult> GetTravelExpectationAsync(string fromStop, string toStop)
        {
            return await _travelAdviceHandler.GetTravelExpectationAsync(fromStop, toStop);
        }
    }
}
