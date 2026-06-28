namespace TrainTicketPlatformAPI.Contracts.Invoices
{
    public class CreateInvoiceRequest
    {
        public string? BuyerName { get; set; }
        public string? BuyerEmail { get; set; }
        public string? BuyerTaxId { get; set; }
        public string? BillingAddress { get; set; }
    }
}
