namespace komikaan.Data.API
{
    public class TrainStationViewportQuery
    {
        public double? MinLatitude { get; set; }
        public double? MaxLatitude { get; set; }
        public double? MinLongitude { get; set; }
        public double? MaxLongitude { get; set; }
        public int Limit { get; set; } = 800;

        public bool HasBounds => MinLatitude != null && MaxLatitude != null && MinLongitude != null && MaxLongitude != null;
    }

    public class MapViewportQuery
    {
        public double MinLatitude { get; set; }
        public double MaxLatitude { get; set; }
        public double MinLongitude { get; set; }
        public double MaxLongitude { get; set; }
        public int Zoom { get; set; } = 8;
        public int ShapeLimit { get; set; } = 200;
        public int StopLimit { get; set; } = 5000;
        public bool FullShapes { get; set; } = false;
    }
}