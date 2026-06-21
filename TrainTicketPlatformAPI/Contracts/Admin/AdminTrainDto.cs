namespace TrainTicketPlatformAPI.Contracts.Admin
{
    public class AdminTrainDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "InterCity";
        public string Locomotive { get; set; } = string.Empty;
        public int CarriageCount { get; set; }
        public int SeatsPerCarriage { get; set; }
        public string Status { get; set; } = "Active";
        public string DepartureStation { get; set; } = string.Empty;
        public string ArrivalStation { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public List<AdminTrainCarriageDto> Carriages { get; set; } = new();
    }

    public class AdminTrainCarriageDto
    {
        public int Id { get; set; }
        public string Coach { get; set; } = string.Empty;
        public int Position { get; set; }
        public string ClassType { get; set; } = "Class 2";
        public string LayoutType { get; set; } = "OpenSecond";
        public string VehicleType { get; set; } = string.Empty;
        public int SeatCount { get; set; }
        public bool HasBikeSpace { get; set; }
        public bool HasAccessibleSpace { get; set; }
        public bool HasFamilyCompartment { get; set; }
        public bool HasDiningSection { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
