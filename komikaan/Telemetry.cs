using System.Diagnostics;

namespace komikaan
{
    public static class Telemetry
    {
        /// <summary>
        /// ActivitySource instance, only one should exist per application
        /// </summary>
        public static readonly ActivitySource Activity = new("komikaan.api");
    }
}
