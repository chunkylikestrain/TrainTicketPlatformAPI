namespace TrainTicketPlatformAPI.Contracts.Invoices
{
    public class InvoiceDto
    {
        public int Id { get; set; }
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
        public string Status { get; set; } = string.Empty;
        public DateTime IssuedAtUtc { get; set; }
        public string PdfUrl { get; set; } = string.Empty;
    }
}
