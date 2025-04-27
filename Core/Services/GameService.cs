using System.Collections.Concurrent;
using Core.Dtos;
using Core.Models;
using Core.Storage;

namespace Core.Services;

public class GameService
{
    public readonly AuthService _authService;
    private readonly ConcurrentDictionary<int, Game> _games = new();
    private int _gameIdCounter = 1;

    public GameService(AuthService authService)
    {
        _authService = authService;
    }

    public Task<int> CreateGameAsync(int judgeId)
    {
        var gameId = Interlocked.Increment(ref _gameIdCounter);
        var game = new Game(
            gameId,
            judgeId,
            new List<Participant> { new(judgeId, Role.Judge) },
            true
        );

        if (!_games.TryAdd(gameId, game))
            throw new InvalidOperationException("Failed to create game.");

        return Task.FromResult(gameId);
    }

    public async Task<string> JoinGameAsync(int userId, JoinGameRequest request)
    {
        // Get game
        if (!_games.TryGetValue(request.GameId, out var game))
            throw new InvalidOperationException("Game not found.");

        // Check if user is already in the game
        if (game.Participants.Any(p => p.UserId == userId))
            throw new InvalidOperationException("User already in game.");

        // Add participant
        game.Participants.Add(new Participant(userId, request.Role));

        // Generate new JWT with role and gameId
        return await _authService.GenerateGameTokenAsync(userId, request.GameId, request.Role);
    }

    public Task<Game?> GetGameAsync(int gameId)
    {
        _games.TryGetValue(gameId, out var game);
        return Task.FromResult(game);
    }

    public Task<Role?> GetUserRoleInGameAsync(int gameId, int userId)
    {
        if (!_games.TryGetValue(gameId, out var game))
            return Task.FromResult<Role?>(null);

        var participant = game.Participants.FirstOrDefault(p => p.UserId == userId);
        return Task.FromResult(participant?.Role);
    }
}