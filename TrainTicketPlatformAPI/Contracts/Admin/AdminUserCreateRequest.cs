using System.ComponentModel.DataAnnotations;

namespace TrainTicketPlatformAPI.Contracts.Admin
{
    public class AdminUserCreateRequest
    {
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Role { get; set; } = "Passenger";

        public string Status { get; set; } = "Active";

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }
}
