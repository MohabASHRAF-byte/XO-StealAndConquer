using Core.Dtos;
using Core.Services;
using Core.Storage;
using Core.UserContext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core.Jobs;

public class StartupJob(
    ILogger<StartupJob> logger,
    IServiceProvider serviceProvider
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Startup job is running...");
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var authService = scope.ServiceProvider.GetRequiredService<AuthService>();
        var gameService = scope.ServiceProvider.GetRequiredService<GameService>();

        // 1. Delete all tables
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        // var users = await dbContext.Users.ToListAsync(cancellationToken);
        // dbContext.Users.RemoveRange(users);
        // await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Database cleaned and recreated.");

        // 2. Add users Mohab0 to Mohab9
        var userTokens = new Dictionary<string, string>();
        for (var i = 0; i < 10; i++)
        {
            var username = $"Mohab{i}";
            var password = "test";

            await authService.RegisterAsync(new RegisterRequest(username, password));

            var token = await authService.LoginAsync(new LoginRequest(username, password));
            userTokens[username] = token;
            logger.LogInformation($"User {username} created and logged in.");
        }

        // 3. Use Mohab0 to create a game
        var userContext = scope.ServiceProvider.GetRequiredService<IUserContext>();
        userContext.SetToken(userTokens["Mohab0"]); // Set context to Mohab0

        var gameId = await gameService.CreateGameAsync(
            new List<string> { "Man City", "Tottenham", "Liverpool" },
            new List<string> { "Egyptian", "English", "Brazilian" }
        );
        logger.LogInformation($"Game created by Mohab0 with ID: {gameId}");

        // 4. Use Mohab1 to join Team1
        userContext.SetToken(userTokens["Mohab1"]);
        await gameService.JoinGameAsync(gameId, Role.Player, Team.Team1);
        logger.LogInformation("Mohab1 joined Team1.");

        // 5. Use Mohab2 to join Team2
        userContext.SetToken(userTokens["Mohab2"]);
        await gameService.JoinGameAsync(gameId, Role.Player, Team.Team2);
        logger.LogInformation("Mohab2 joined Team2.");

        // 6. Use Mohab3 to join as Spectator
        userContext.SetToken(userTokens["Mohab3"]);
        await gameService.JoinGameAsync(gameId, Role.Spectator, Team.Spectator);
        logger.LogInformation("Mohab3 joined as Spectator.");

        logger.LogInformation("Startup job finished.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}