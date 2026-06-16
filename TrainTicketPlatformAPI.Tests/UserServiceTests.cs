using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
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
        {
            var opts = new DbContextOptionsBuilder<TrainTicketDbContext>()
                .UseInMemoryDatabase(name)
                .Options;

            return new TrainTicketDbContext(opts);
        }

        [Test]
        public async Task RegisterAsync_CreatesUser_WhenEmailUnique()
        {
            var db = NewDb("UserRegsUnique");
            var svc = new UserService(db, _config);

            var dto = new RegisterDto
            {
                Email = "alice@example.com",
                Password = "P@ssw0rd!",
                Phone = "555-1234"
            };

            var user = await svc.RegisterAsync(dto);

            Assert.That(user.Id, Is.GreaterThan(0));
            Assert.That(user.Email, Is.EqualTo(dto.Email));
            Assert.That(user.Phone, Is.EqualTo(dto.Phone));
            Assert.That(user.Role, Is.EqualTo("Passenger"));
            Assert.That(user.PasswordHash, Is.Not.EqualTo(dto.Password));
            Assert.That(BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash), Is.True);

            db.Dispose();
        }

        [Test]
        public void RegisterAsync_Throws_WhenEmailExists()
        {
            var db = NewDb("UserRegsDup");
            // pre-seed a user with that email
            db.Users.Add(new User
            {
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

            Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.RegisterAsync(dto),
                "Email already in use"
            );

            db.Dispose();
        }

        [Test]
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
        public async Task LoginAsync_ReturnsJwtandMetadata_WhenCredentialsValid()
=======
        public async Task LoginAsync_ReturnsLoginResponse_WhenCredentialsValid()
>>>>>>> theirs
=======
        public async Task LoginAsync_ReturnsLoginResponse_WhenCredentialsValid()
>>>>>>> theirs
=======
        public async Task LoginAsync_ReturnsLoginResponse_WhenCredentialsValid()
>>>>>>> theirs
        {
            // arrange a brand-new in-memory database
            var db = NewDb("UserLoginValid");
            var plain = "Secret123!";
            var user = new User
            {
                Email = "carol@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(plain),
                Phone = "555-7777",
                Role = "Passenger"
            };
            // add + save
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            var svc = new UserService(db, _config);
            var dto = new LoginDto
            {
                Email = user.Email,
                Password = plain
            };

<<<<<<< ours
            // act
            var result = await svc.LoginAsync(dto);

            // assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.Not.Null.And.Not.Empty);
            Assert.That(result.UserId, Is.EqualTo(user.Id));
            Assert.That(result.Role, Is.EqualTo(user.Role));

            // check JWT format
            var parts = result.Token.Split('.');
=======
            // Act
            var response = await svc.LoginAsync(dto);

            // Assert
            Assert.That(response.Token, Is.Not.Null.And.Not.Empty);
            Assert.That(response.UserId, Is.EqualTo(1));
            Assert.That(response.Role, Is.EqualTo("Passenger"));

            // basic sanity: must be in JWT format
            var parts = response.Token.Split('.');
<<<<<<< ours
<<<<<<< ours
>>>>>>> theirs
=======
>>>>>>> theirs
=======
>>>>>>> theirs
            Assert.That(parts.Length, Is.EqualTo(3));

            // check issuer/audience/subject
            var handler = new JwtSecurityTokenHandler();
<<<<<<< ours
<<<<<<< ours
<<<<<<< ours
            var jwt = handler.ReadJwtToken(result.Token);
=======
            var jwt = handler.ReadJwtToken(response.Token);
>>>>>>> theirs
=======
            var jwt = handler.ReadJwtToken(response.Token);
>>>>>>> theirs
=======
            var jwt = handler.ReadJwtToken(response.Token);
>>>>>>> theirs
            Assert.That(jwt.Issuer, Is.EqualTo("TestIssuer"));
            Assert.That(jwt.Audiences, Does.Contain("TestAudience"));
            Assert.That(jwt.Subject, Is.EqualTo(user.Id.ToString()));

            db.Dispose();
        }

        [Test]
        public void LoginAsync_Throws_WhenEmailNotFound()
        {
            var db = NewDb("UserLoginNoEmail");
            var svc = new UserService(db, _config);
            var dto = new LoginDto
            {
                Email = "doesnotexist@example.com",
                Password = "irrelevant"
            };

            Assert.ThrowsAsync<KeyNotFoundException>(
                () => svc.LoginAsync(dto)
            );

            db.Dispose();
        }

        [Test]
        public void LoginAsync_Throws_WhenPasswordMismatch()
        {
            var db = NewDb("UserLoginBadPass");
            db.Users.Add(new User
            {
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

            Assert.ThrowsAsync<KeyNotFoundException>(
                () => svc.LoginAsync(dto)
            );

            db.Dispose();
        }
    }
}

