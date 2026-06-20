namespace TrainTicketPlatformAPI.Contracts.Admin
{
    public class AdminDiscountDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Percent { get; set; }
        public string EligibleClass { get; set; } = "Class 2 only";
        public string DocumentHint { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
    }
}
