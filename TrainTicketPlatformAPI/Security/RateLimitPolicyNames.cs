namespace TrainTicketPlatformAPI.Security
{
    public static class RateLimitPolicyNames
    {
        public const string Auth = "auth";
        public const string PublicRead = "public-read";
        public const string PublicSearch = "public-search";
        public const string BookingWrite = "booking-write";
        public const string TicketAccess = "ticket-access";
        public const string Payment = "payment";
        public const string AdminImport = "admin-import";
    }
}
