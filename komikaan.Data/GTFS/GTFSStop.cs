﻿namespace komikaan.Data.GTFS
{
    public class GTFSStop
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentStation { get; set; }
        public StopType StopType { get; set; }
    }
}
