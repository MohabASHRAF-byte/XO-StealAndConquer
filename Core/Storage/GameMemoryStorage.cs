using System.Collections.Concurrent;
using Core.Models;

namespace Core.Storage;

public static class GameMemoryStorage
{
    // In-memory storage for games
    private static readonly ConcurrentDictionary<int, Game> _games = new();

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
}