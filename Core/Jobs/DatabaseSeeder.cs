using Core.Dtos;
using Core.Services;
using Core.Storage;
using Core.UserContext;
using Microsoft.Extensions.Logging;

namespace Core.Jobs;

public class DatabaseSeeder
{
    private readonly AuthService _authService;
    private readonly AppDbContext _dbContext;
    private readonly GameService _gameService;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly IUserContext _userContext;

    public DatabaseSeeder(
        AppDbContext dbContext,
        AuthService authService,
        GameService gameService,
        IUserContext userContext,
        ILogger<DatabaseSeeder> logger)
    {
        _dbContext = dbContext;
        _authService = authService;
        _gameService = gameService;
        _userContext = userContext;
        _logger = logger;
    }

    public Dictionary<string, string> UserTokens { get; } = new();

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Seeding database...");

        await _dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await _dbContext.Database.EnsureCreatedAsync(cancellationToken);

        // 1. Register 10 users
        for (var i = 0; i < 10; i++)
        {
            var username = $"Mohab{i}";
            var password = "test";

            await _authService.RegisterAsync(new RegisterRequest(username, password));
            var token = await _authService.LoginAsync(new LoginRequest(username, password));
            UserTokens[username] = token;

            _logger.LogInformation($"Created and logged in {username}");
        }

        // 2. Create game with Mohab0
        _userContext.SetToken(UserTokens["Mohab0"]);
        var gameId = await _gameService.CreateGameAsync(
            new List<string> { "Man City", "Tottenham", "Liverpool" },
            new List<string> { "Egyptian", "English", "Brazilian" }
        );
        _logger.LogInformation($"Game created by Mohab0 (ID: {gameId})");

        // 3. Add players
        _userContext.SetToken(UserTokens["Mohab1"]);
        await _gameService.JoinGameAsync(gameId, Role.Player, Team.Team1);

        _userContext.SetToken(UserTokens["Mohab2"]);
        await _gameService.JoinGameAsync(gameId, Role.Player, Team.Team2);

        _userContext.SetToken(UserTokens["Mohab3"]);
        await _gameService.JoinGameAsync(gameId, Role.Spectator, Team.Spectator);
    }
}