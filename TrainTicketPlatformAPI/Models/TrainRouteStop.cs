namespace TrainTicketPlatformAPI.Models
{
    public class TrainRouteStop
    {
        public int Id { get; set; }
        public int TrainRouteId { get; set; }
        public int StationId { get; set; }
        public int StopOrder { get; set; }
        public int? ArrivalOffsetMinutes { get; set; }
        public int? DepartureOffsetMinutes { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;
        public string StopType { get; set; } = string.Empty;
        public int? ExternalStationId { get; set; }
        public int? ExternalStopTypeId { get; set; }
        public string ExternalStopTypeName { get; set; } = string.Empty;
        public string ExternalArrivalTrainNumber { get; set; } = string.Empty;
        public string ExternalDepartureTrainNumber { get; set; } = string.Empty;
        public int? ArrivalDayOffset { get; set; }
        public int? DepartureDayOffset { get; set; }

        public TrainRoute TrainRoute { get; set; } = null!;
        public Station Station { get; set; } = null!;
    }
}
