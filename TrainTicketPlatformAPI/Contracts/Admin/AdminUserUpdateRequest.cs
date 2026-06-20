namespace TrainTicketPlatformAPI.Contracts.Admin
{
    public class AdminUserUpdateRequest
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = "Passenger";
        public string Status { get; set; } = "Active";
    }
}
