namespace TrainTicketPlatformAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public ICollection<Booking> Bookings { get; set; } 
            = new List<Booking>();
    }
}
