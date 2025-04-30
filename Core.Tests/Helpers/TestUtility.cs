using Core.Dtos;
using Core.Services;
using Core.Storage;
using Core.UserContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Jobs;
using Core.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Core.Tests.Helpers
{
    public class TestUtils
    {
        // Mocked objects
        public Mock<AppDbContext> DbContext { get; private set; }
        public AuthService AuthService { get; private set; }  // We will instantiate this manually
        public Mock<GameService> GameService { get; private set; }
        public Mock<IUserContext> UserContext { get; private set; }
        public Mock<ILogger<DatabaseSeeder>> Logger { get; private set; }
        public Dictionary<string, string> UserTokens { get; private set; }

        public TestUtils()
        {
            // Step 1: Initialize in-memory DbContext
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            DbContext = new Mock<AppDbContext>(dbContextOptions);
            DbContext.CallBase = true;

            // Step 2: Mock DbSet<User> and AppDbContext.Users property
            var mockUsersDbSet = new Mock<DbSet<User>>();
            DbContext.Setup(db => db.Users).Returns(mockUsersDbSet.Object);

            // Step 3: Mock IConfiguration for AuthService
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c["Jwt:Key"]).Returns("fake_jwt_key");
            configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("fake_issuer");
            configurationMock.Setup(c => c["Jwt:Audience"]).Returns("fake_audience");

            // Step 4: Instantiate AuthService with the necessary dependencies
            AuthService = new AuthService(DbContext.Object, configurationMock.Object);

            // Step 5: Mock GameService
            GameService = new Mock<GameService>(DbContext.Object, AuthService);

            // Step 6: Mock IUserContext
            UserContext = new Mock<IUserContext>();

            // Step 7: Mock ILogger<DatabaseSeeder>
            Logger = new Mock<ILogger<DatabaseSeeder>>();

            // Initialize UserTokens
            UserTokens = new Dictionary<string, string>();
        }

        // Step 3: Seed the database with test data
        public async Task SeedDatabaseAsync()
        {
            // Register and log in 10 users
            for (var i = 0; i < 10; i++)
            {
                var username = $"Mohab{i}";
                var password = "test";

                // Mock RegisterAsync and LoginAsync to return a Task.CompletedTask and mock login token respectively
                var mockToken = "mock-token";
                var userId = i + 1; // Mock user ID as a simple increment

                var user = new User(
                    userId, // Mock user ID
                    username,
                    BCrypt.Net.BCrypt.HashPassword(password),
                    null
                );

                // Mock the DbSet's behavior for adding users
                var mockUsersDbSet = new Mock<DbSet<User>>();

                // Create a mock EntityEntry to return for AddAsync
                var mockEntityEntry = new Mock<EntityEntry<User>>();
                mockEntityEntry.Setup(entry => entry.Entity).Returns(user);

                mockUsersDbSet.Setup(m => m.AddAsync(It.IsAny<User>(), It.IsAny<System.Threading.CancellationToken>()))
                    .ReturnsAsync(mockEntityEntry.Object);

                DbContext.Setup(db => db.Users).Returns(mockUsersDbSet.Object);

                // Mock the RegisterAsync and LoginAsync methods
                var registerRequest = new RegisterRequest(username, password);
                var loginRequest = new LoginRequest(username, password);

                // Here we assume the login always succeeds and returns a mocked token
                await AuthService.RegisterAsync(registerRequest);
                var token = await AuthService.LoginAsync(loginRequest);
                UserTokens[username] = token;
            }

            // Create a game with Mohab0
            UserContext.Setup(u => u.SetToken(UserTokens["Mohab0"]));
            var gameId = await GameService.Object.CreateGameAsync(
                new() { "Man City", "Tottenham", "Liverpool" },
                new() { "Egyptian", "English", "Brazilian" }
            );

            // Add players to the game
            for (var i = 1; i < 3; i++)
            {
                UserContext.Setup(u => u.SetToken(UserTokens[$"Mohab{i}"]));
                await GameService.Object.JoinGameAsync(gameId, Role.Player, Team.Team1);
            }

            UserContext.Setup(u => u.SetToken(UserTokens["Mohab3"]));
            await GameService.Object.JoinGameAsync(gameId, Role.Spectator, Team.Spectator);
        }
    }
}
