namespace TrainTicketPlatformAPI.Contracts.Admin
{
    public class AdminRevenueReportDto
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal Refunds { get; set; }
        public decimal NetRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int PaidBookings { get; set; }
        public int RefundedBookings { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<AdminRevenueDailyPointDto> DailyRevenue { get; set; } = new();
        public List<AdminRevenueRouteDto> RouteBreakdown { get; set; } = new();
        public List<AdminRevenueActivityDto> RecentActivity { get; set; } = new();
    }

    public class AdminRevenueDailyPointDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public decimal Refunds { get; set; }
        public int Bookings { get; set; }
    }

    public class AdminRevenueRouteDto
    {
        public string Route { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int PaidBookings { get; set; }
    }

    public class AdminRevenueActivityDto
    {
        public string BookingReference { get; set; } = string.Empty;
        public string TicketNumber { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
