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
        private readonly IConfiguration _config;

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
            user.Email = user.Email.Trim();
            user.NormalizedEmail = NormalizeEmail(user.Email);

            bool exists = await _db.Users.AnyAsync(u =>
                u.NormalizedEmail == user.NormalizedEmail ||
                u.Email.ToUpper() == user.NormalizedEmail);
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

            var requestedEmail = user.Email.Trim();
            var requestedNormalizedEmail = NormalizeEmail(requestedEmail);

            if (existing.NormalizedEmail != requestedNormalizedEmail)
            {
                bool taken = await _db.Users.AnyAsync(u =>
                    u.Id != user.Id &&
                    (u.NormalizedEmail == requestedNormalizedEmail ||
                     u.Email.ToUpper() == requestedNormalizedEmail));
                if (taken)
                    throw new InvalidOperationException("Email already in use");
                existing.Email = requestedEmail;
                existing.NormalizedEmail = requestedNormalizedEmail;
            }

            // update other allowed fields
            existing.Phone = user.Phone;
            existing.Role = user.Role;
            existing.DisplayName = user.DisplayName;
            existing.Status = user.Status;

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
        // Inject IConfiguration in addition to DbContext
        public UserService(TrainTicketDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // New: Register
        public async Task<User> RegisterAsync(RegisterDto dto)
        {
            var email = dto.Email.Trim();
            var normalizedEmail = NormalizeEmail(email);

            if (await _db.Users.AnyAsync(u =>
                    u.NormalizedEmail == normalizedEmail ||
                    u.Email.ToUpper() == normalizedEmail))
                throw new InvalidOperationException("Email already in use");

            // Hash the password
            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Email = email,
                NormalizedEmail = normalizedEmail,
                PasswordHash = hash,
                Phone = dto.Phone,
                Role = "Passenger"   // default role
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        // Authenticate / Login
        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            var normalizedEmail = NormalizeEmail(dto.Email);

            var user = await _db.Users
                .FirstOrDefaultAsync(u =>
                    u.NormalizedEmail == normalizedEmail ||
                    u.Email.ToUpper() == normalizedEmail);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new KeyNotFoundException("Invalid credentials");

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

            var jwtKey = _config["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is not configured");
            var jwtIssuer = _config["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer is not configured");
            var jwtAudience = _config["Jwt:Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience is not configured");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            var serializedToken = new JwtSecurityTokenHandler().WriteToken(token);

            return new LoginResponseDto
            {
                Token = serializedToken,
                UserId = user.Id,
                Role = user.Role
            };
        }

        private static string NormalizeEmail(string email)
            => email.Trim().ToUpperInvariant();
    }
}
