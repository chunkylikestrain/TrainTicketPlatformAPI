using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
namespace TrainTicketPlatformAPI.Services
{
    public class UserService : IUserService
    {
        private readonly TrainTicketDbContext _db;
        public UserService(TrainTicketDbContext db) => _db = db;

        public async Task<IEnumerable<User>> GetAllUsersAsync() =>
            await _db.Users.ToListAsync();

        public async Task<User> GetUserByIdAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");
            return user;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            // business rule: email must be unique
            bool exists = await _db.Users.AnyAsync(u => u.Email == user.Email);
            if (exists)
                throw new InvalidOperationException("Email already in use");

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            var existing = await _db.Users.FindAsync(user.Id)
                           ?? throw new KeyNotFoundException("User not found");

            // if email changed, ensure new email is not taken
            if (existing.Email != user.Email)
            {
                bool taken = await _db.Users.AnyAsync(u => u.Email == user.Email);
                if (taken)
                    throw new InvalidOperationException("Email already in use");
                existing.Email = user.Email;
            }

            // update other allowed fields
            existing.Phone = user.Phone;
            existing.Role = user.Role;

            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId)
                      ?? throw new KeyNotFoundException("User not found");

            // business rule: cannot delete user with existing bookings
            bool hasBookings = await _db.Bookings.AnyAsync(b => b.UserId == userId);
            if (hasBookings)
                throw new InvalidOperationException("Cannot delete user with active bookings");

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
        }
        private readonly IConfiguration _config;

        // Inject IConfiguration in addition to DbContext
        public UserService(TrainTicketDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // New: Register
        public async Task<User> RegisterAsync(RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("Email already in use");

            // Hash the password
            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = hash,
                Phone = dto.Phone,
                Role = "Passenger"   // default role
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        // Authenticate / Login
        public async Task<string> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new KeyNotFoundException("Invalid credentials");

            // 1) Create claims with a "sub" claim:
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

            // 2) Create the signing key & credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 3) Create the token
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            // 4) Return the serialized token
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
