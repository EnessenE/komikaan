using Microsoft.AspNetCore.Mvc;

namespace komikthuis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "Test")]
        public string Get()
        {
            return "hi";
        }
    }
}
