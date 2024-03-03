using GTFS;
using GTFS.IO;

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
            string path = "C:\\Users\\maile\\Downloads\\gtfs-nl";

            var reader = new GTFSReader<GTFSFeed>();
            var feed = reader.Read(path);

            foreach (var agency in feed.Agencies)
            {
                Console.WriteLine($"{agency.Name} - {agency.LanguageCode}");
            }


        }
    }
}
