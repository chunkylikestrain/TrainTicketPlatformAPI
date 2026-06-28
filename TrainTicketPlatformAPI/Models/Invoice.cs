namespace TrainTicketPlatformAPI.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? BookingId { get; set; }
        public int? BookingOrderId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string BuyerName { get; set; } = string.Empty;
        public string BuyerEmail { get; set; } = string.Empty;
        public string BuyerTaxId { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public decimal NetAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "PLN";
        public string Status { get; set; } = "Issued";
        public DateTime IssuedAtUtc { get; set; }

        public User? User { get; set; }
        public Booking? Booking { get; set; }
        public BookingOrder? BookingOrder { get; set; }
    }
}
