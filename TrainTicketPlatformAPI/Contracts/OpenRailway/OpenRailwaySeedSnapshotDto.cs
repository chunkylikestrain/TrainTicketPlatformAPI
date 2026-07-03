namespace TrainTicketPlatformAPI.Contracts.OpenRailway
{
    public class OpenRailwaySeedSnapshotDto
    {
        public string ExternalSource { get; set; } = "PLK";
        public DateTime ExportedAtUtc { get; set; }
        public List<OpenRailwaySeedStationDto> Stations { get; set; } = [];
        public List<OpenRailwaySeedTrainDto> Trains { get; set; } = [];
        public List<OpenRailwaySeedRouteDto> Routes { get; set; } = [];
        public List<OpenRailwaySeedTripDto> Trips { get; set; } = [];
    }

    public class OpenRailwaySeedStationDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int? ExternalStationId { get; set; }
    }

    public class OpenRailwaySeedTrainDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int CarriageCount { get; set; }
        public int SeatsPerCarriage { get; set; }
        public string Status { get; set; } = "Active";
        public string DepartureStation { get; set; } = string.Empty;
        public string ArrivalStation { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string ExternalCarrierCode { get; set; } = string.Empty;
        public string ExternalCommercialCategorySymbol { get; set; } = string.Empty;
        public string ExternalNationalNumber { get; set; } = string.Empty;
        public string ExternalInternationalArrivalNumber { get; set; } = string.Empty;
        public string ExternalInternationalDepartureNumber { get; set; } = string.Empty;
        public List<OpenRailwaySeedCarriageDto> Carriages { get; set; } = [];
    }

    public class OpenRailwaySeedCarriageDto
    {
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
    }

    public class OpenRailwaySeedRouteDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AdminDisplayName { get; set; } = string.Empty;
        public string RouteFingerprint { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public string OperatingDays { get; set; } = "Imported";
        public string IntermediateStops { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int? ExternalScheduleId { get; set; }
        public int? ExternalOrderId { get; set; }
        public int? ExternalTrainOrderId { get; set; }
        public DateOnly? ExternalOperatingDate { get; set; }
        public int? DepartureExternalStationId { get; set; }
        public int? ArrivalExternalStationId { get; set; }
        public List<OpenRailwaySeedRouteStopDto> Stops { get; set; } = [];
    }

    public class OpenRailwaySeedRouteStopDto
    {
        public int? ExternalStationId { get; set; }
        public string StationCode { get; set; } = string.Empty;
        public int StopOrder { get; set; }
        public int? ArrivalOffsetMinutes { get; set; }
        public int? DepartureOffsetMinutes { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;
        public string StopType { get; set; } = string.Empty;
        public int? ExternalStopTypeId { get; set; }
        public string ExternalStopTypeName { get; set; } = string.Empty;
        public string ExternalArrivalTrainNumber { get; set; } = string.Empty;
        public string ExternalDepartureTrainNumber { get; set; } = string.Empty;
        public int? ArrivalDayOffset { get; set; }
        public int? DepartureDayOffset { get; set; }
    }

    public class OpenRailwaySeedTripDto
    {
        public string TrainCode { get; set; } = string.Empty;
        public string RouteCode { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled";
        public int DelayMinutes { get; set; }
        public string CancellationReason { get; set; } = string.Empty;
        public string OriginalPlatform { get; set; } = string.Empty;
        public string OriginalTrack { get; set; } = string.Empty;
        public string DisruptionMessage { get; set; } = string.Empty;
        public string DisruptionSeverity { get; set; } = string.Empty;
        public int? ExternalScheduleId { get; set; }
        public int? ExternalOrderId { get; set; }
        public int? ExternalTrainOrderId { get; set; }
        public DateOnly? ExternalOperatingDate { get; set; }
        public string ExternalRawVersion { get; set; } = string.Empty;
        public List<OpenRailwaySeedFareDto> Fares { get; set; } = [];
    }

    public class OpenRailwaySeedFareDto
    {
        public string ClassType { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "PLN";
    }
}
