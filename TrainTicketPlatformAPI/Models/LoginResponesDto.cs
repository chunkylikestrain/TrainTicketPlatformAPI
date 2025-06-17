namespace TrainTicketPlatformAPI.Models
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
