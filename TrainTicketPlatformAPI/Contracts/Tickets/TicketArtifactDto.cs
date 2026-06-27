namespace TrainTicketPlatformAPI.Contracts.Tickets
{
    public class TicketArtifactDto
    {
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string TicketNumber { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public string TrainName { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string SeatLabel { get; set; } = string.Empty;
        public string JourneyDirection { get; set; } = string.Empty;
        public int JourneySegmentIndex { get; set; }
        public DateTime TravelDate { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public DateTime IssuedAtUtc { get; set; }
        public string QrPayload { get; set; } = string.Empty;
        public string QrSvgUrl { get; set; } = string.Empty;
        public string PdfUrl { get; set; } = string.Empty;
        public string EmailDeliveryStatus { get; set; } = string.Empty;
        public DateTime? EmailSentAtUtc { get; set; }
    }
}
