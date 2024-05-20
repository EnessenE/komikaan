using GTFS;

namespace komikaan.Tests.Tests
{
    [TestClass]
    public class GTFS_Test
    {

        [TestInitialize]
        public void Init()
        {

        }

        [TestMethod]
        public void Test()
        {
            string path = "C:\\Users\\maile\\Downloads\\gtfs-nl.zip";

            var reader = new GTFSReader<GTFSFeed>("unit-test");
            var feed = reader.Read(path);


            foreach (var agency in feed.Agencies)
            {
                Console.WriteLine(agency.Name);
            }
            Console.WriteLine($"Found a feed with {feed.Agencies.Count} agencies");
        }
    }
}
