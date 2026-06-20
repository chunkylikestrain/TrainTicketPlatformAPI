namespace TrainTicketPlatformAPI.Models
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
    }
}
