using Microsoft.EntityFrameworkCore;

namespace TrainTicketPlatformAPI.Models
{
    public class DiscountRule
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        [Precision(5, 2)]
        public decimal Percent { get; set; }

        public string EligibleClass { get; set; } = "Class 2 only";
        public string DocumentHint { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
    }
}
