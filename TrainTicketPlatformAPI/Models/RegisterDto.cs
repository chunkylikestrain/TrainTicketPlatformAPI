using System.ComponentModel.DataAnnotations;

namespace TrainTicketPlatformAPI.Models
{
    public class RegisterDto
    {
        [Required] public string Email { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
        [Required] public string Phone { get; set; } = string.Empty;
    }
}


