using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(int userId);
        Task<User> CreateUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task DeleteUserAsync(int userId);
        Task<User> RegisterAsync(RegisterDto dto);
        Task<string> LoginAsync(LoginDto dto);
        

    }

}
