namespace TrainTicketPlatformAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public ICollection<Booking> Bookings { get; set; } 
            = new List<Booking>();
    }
}
