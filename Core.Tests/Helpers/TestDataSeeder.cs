using Core.Dtos;
using Core.Services;
using Core.Storage;
using Core.UserContext;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace Core.Tests.Helpers;

public static class TestDataSeeder
{
    public static (Mock<AppDbContext> dbContext, Mock<AuthService> authService, Mock<GameService> gameService, Mock<IUserContext> userContext, Mock<IHubCallerClients> mockClients, Mock<IGroupManager> mockGroups, Dictionary<string, string> userTokens) CreateMocksAndSeedData()
    {
        // Step 1: Mocking DbContext
        var dbContext = MockDbFactory.Create();

        // Step 2: Mock AuthService (Manually provide its dependencies)
        var authServiceMock = new Mock<AuthService>(dbContext.Object);

        // Step 3: Mock GameService
        var gameService = new Mock<GameService>(dbContext.Object, authServiceMock.Object);

        // Step 4: Mock IUserContext
        var userContext = new Mock<IUserContext>();

        // Step 5: Mock Clients and Group Manager for Hub
        var mockClients = new Mock<IHubCallerClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        var mockGroups = new Mock<IGroupManager>();

        // Step 6: Create a dictionary to store user tokens
        var userTokens = new Dictionary<string, string>();

        // Step 7: Seed the database with users and games
        Task.Run(async () =>
        {
            // Register and log in users
            for (var i = 0; i < 10; i++)
            {
                var username = $"Mohab{i}";
                var password = "test";
                await authServiceMock.Object.RegisterAsync(new RegisterRequest(username, password));
                var token = await authServiceMock.Object.LoginAsync(new LoginRequest(username, password));
                userTokens[username] = token;
            }

            // Create a game with Mohab0
            userContext.Setup(u => u.SetToken(userTokens["Mohab0"]));
            var gameId = await gameService.Object.CreateGameAsync(
                new() { "Man City", "Tottenham", "Liverpool" },
                new() { "Egyptian", "English", "Brazilian" }
            );

            // Add players to the game
            for (var i = 1; i < 3; i++)
            {
                userContext.Setup(u => u.SetToken(userTokens[$"Mohab{i}"]));
                await gameService.Object.JoinGameAsync(gameId, Role.Player, Team.Team1);
            }

            userContext.Setup(u => u.SetToken(userTokens["Mohab3"]));
            await gameService.Object.JoinGameAsync(gameId, Role.Spectator, Team.Spectator);
        }).Wait();  // Make sure database seeding completes before tests run

        // Return mocked objects
        return (dbContext, authServiceMock, gameService, userContext, mockClients, mockGroups, userTokens);
    }
}
