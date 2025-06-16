using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Tests
{
    [TestFixture]
    public class UserServiceTests
    {
        private IConfiguration _config;

        [SetUp]
        public void Setup()
        {
            // minimal in-memory JWT settings
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Jwt:Key"] = "0123456789ABCDEF0123456789ABCDEF",
                    ["Jwt:Issuer"] = "TestIssuer",
                    ["Jwt:Audience"] = "TestAudience"
                })
                .Build();
        }

        private TrainTicketDbContext NewDb(string name)
            => TestHelpers.GetInMemoryDb(name);

        [Test]
        public async Task RegisterAsync_CreatesUser_WhenEmailUnique()
        {
            // Arrange
            var db = NewDb("UserRegsUnique");
            var svc = new UserService(db, _config);
            var dto = new RegisterDto
            {
                Email = "alice@example.com",
                Password = "P@ssw0rd!",
                Phone = "555-1234"
            };

            // Act
            var user = await svc.RegisterAsync(dto);

            // Assert
            Assert.That(user.Id, Is.GreaterThan(0));
            Assert.That(user.Email, Is.EqualTo(dto.Email));
            Assert.That(user.Phone, Is.EqualTo(dto.Phone));
            Assert.That(user.Role, Is.EqualTo("Passenger"));

            // stored hash should not equal the plain password
            Assert.That(user.PasswordHash, Is.Not.EqualTo(dto.Password));
            // and should verify correctly
            Assert.That(BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash), Is.True);
        }

        [Test]
        public void RegisterAsync_Throws_WhenEmailExists()
        {
            // Arrange
            var db = NewDb("UserRegsDup");
            db.Users.Add(new User
            {
                Id = 1,
                Email = "bob@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("whatever"),
                Phone = "555-0000",
                Role = "Passenger"
            });
            db.SaveChanges();

            var svc = new UserService(db, _config);
            var dto = new RegisterDto
            {
                Email = "bob@example.com",
                Password = "NewPass1",
                Phone = "555-1111"
            };

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.RegisterAsync(dto),
                "Email already in use"
            );
        }

        [Test]
        public async Task LoginAsync_ReturnsJwtToken_WhenCredentialsValid()
        {
            // Arrange
            var db = NewDb("UserLoginValid");
            var plain = "Secret123!";
            var hash = BCrypt.Net.BCrypt.HashPassword(plain);

            db.Users.Add(new User
            {
                Id = 1,
                Email = "carol@example.com",
                PasswordHash = hash,
                Phone = "555-2222",
                Role = "Passenger"
            });
            await db.SaveChangesAsync();

            var svc = new UserService(db, _config);
            var dto = new LoginDto
            {
                Email = "carol@example.com",
                Password = plain
            };

            // Act
            var token = await svc.LoginAsync(dto);

            // Assert
            Assert.That(token, Is.Not.Null.And.Not.Empty);
            // basic sanity: must be in JWT format
            var parts = token.Split('.');
            Assert.That(parts.Length, Is.EqualTo(3));

            // can even parse it
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            Assert.That(jwt.Issuer, Is.EqualTo("TestIssuer"));
            Assert.That(jwt.Audiences, Does.Contain("TestAudience"));
            // the sub claim should be the user's id:
            Assert.That(jwt.Subject, Is.EqualTo("1"));
        }

        [Test]
        public void LoginAsync_Throws_WhenEmailNotFound()
        {
            // Arrange
            var db = NewDb("UserLoginNoEmail");
            var svc = new UserService(db, _config);
            var dto = new LoginDto
            {
                Email = "doesnotexist@example.com",
                Password = "irrelevant"
            };

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                () => svc.LoginAsync(dto)
            );
        }

        [Test]
        public void LoginAsync_Throws_WhenPasswordMismatch()
        {
            // Arrange
            var db = NewDb("UserLoginBadPass");
            db.Users.Add(new User
            {
                Id = 1,
                Email = "dave@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("RightPass!"),
                Phone = "555-3333",
                Role = "Passenger"
            });
            db.SaveChanges();

            var svc = new UserService(db, _config);
            var dto = new LoginDto
            {
                Email = "dave@example.com",
                Password = "WrongPass!"
            };

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                () => svc.LoginAsync(dto)
            );
        }
    }
}
