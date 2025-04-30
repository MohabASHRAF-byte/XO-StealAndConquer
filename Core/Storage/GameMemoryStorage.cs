using System.Collections.Concurrent;
using Core.Dtos;
using Core.Models;

namespace Core.Storage;

public static class GameMemoryStorage
{
    // In-memory storage for games
    private static readonly ConcurrentDictionary<int, Game> _games = new();
    public static Dictionary<int, Dictionary<int, List<SignalRDtos.CellSelectorDto>>> CellSelections { get; } = new();

    // Method to add a game
    public static bool TryAddGame(int gameId, Game game)
    {
        return _games.TryAdd(gameId, game);
    }

    // Method to get a game
    public static bool TryGetGame(int gameId, out Game game)
    {
        return _games.TryGetValue(gameId, out game);
    }

    // Method to remove a game
    public static bool TryRemoveGame(int gameId)
    {
        return _games.TryRemove(gameId, out _);
    }

    // Method to get all games (optional)
    public static ConcurrentDictionary<int, Game> GetAllGames()
    {
        return _games;
    }

    public static int PlayerTeam(int gameId, int playerId)
    {
        TryGetGame(gameId, out var game);
        foreach (var p in game.Team1)
            if (p.Id == playerId)
                return 1;
        foreach (var p in game.Team2)
            if (p.Id == playerId)
                return 2;
        return 0;
    }
}