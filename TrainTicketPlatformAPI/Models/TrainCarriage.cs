namespace TrainTicketPlatformAPI.Models
{
    public class TrainCarriage
    {
        public int Id { get; set; }
        public int TrainId { get; set; }
        public string Coach { get; set; } = string.Empty;
        public int Position { get; set; }
        public string ClassType { get; set; } = string.Empty;
        public string LayoutType { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public int SeatCount { get; set; }
        public bool HasBikeSpace { get; set; }
        public bool HasAccessibleSpace { get; set; }
        public bool HasFamilyCompartment { get; set; }
        public bool HasDiningSection { get; set; }
        public string Notes { get; set; } = string.Empty;

        public Train Train { get; set; } = null!;
    }
}
