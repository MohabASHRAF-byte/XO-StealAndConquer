using Core.Models;
using Core.Repositories.User;
using Core.Storage;
using Core.UserContext;
using Microsoft.AspNetCore.Http;

namespace Core.Services;

public class GameService(IUserContext userContext, IUserRepository userRepository)
{
    public async Task<int> CreateGameAsync(List<string> rowLabels, List<string> colLabels, int roundDuration = 30)
    {
        // Check if user can create a game
        var user = userContext.GetCurrentUser();
        var canCreateGame = await userRepository.CanjoinGame(user.Id);
        if (canCreateGame != null)
            throw new BadHttpRequestException("User Already on a game leave it first!");

        // Check if the labels are valid
        if (rowLabels.Count != 3 || colLabels.Count != 3)
            throw new BadHttpRequestException("Invalid number of row/col labels");

        // Check if the time duration is correct
        if (roundDuration <= 0)
            roundDuration = 30;

        var gameId = GenerateUniqueGameId();
        var judge = new PlayerDto
        {
            Id = user.Id,
            Username = user.UserName
        };

        var game = new Game
        {
            Judge = judge,
            Participants = new List<int> { user.Id },
            GameId = gameId,
            RowLabels = rowLabels,
            ColumnLabels = colLabels,
            CellStates = Enumerable.Repeat(CellStates.Empty, 9).ToList(),
            Round = Round.Judge,
            RoundDuration = roundDuration
        };

        // Add the game to memory storage
        if (!GameMemoryStorage.TryAddGame(gameId, game))
            throw new InvalidOperationException("Failed to create game.");
        await userRepository.AddPlayerToGame(user.Id, gameId);
        return gameId;
    }

    public async Task EndGameAsync(int gameId)
    {
        var user = userContext.GetCurrentUser();

        // Get the game from memory storage
        if (!GameMemoryStorage.TryGetGame(gameId, out var game))
            throw new InvalidOperationException("Game not found.");

        // Ensure only the judge can end the game
        if (user.Id != game.Judge.Id)
            throw new InvalidOperationException("Only the Game Judge can end the game.");

        await userRepository.SetUsersFree(game.Participants);
        // Remove the game from memory storage
        GameMemoryStorage.TryRemoveGame(gameId);
    }

    public Game LoadGameAsync(int gameId)
    {
        var user = userContext.GetCurrentUser();

        if (!GameMemoryStorage.TryGetGame(gameId, out var game))
            throw new InvalidOperationException("Game not found.");

        // Check if the user is a participant
        if (!game.Participants.Contains(user.Id))
            throw new InvalidOperationException("User should join the game to watch it.");

        return game;
    }

    public async Task JoinGameAsync(int gameId, Role newRole, Team team)
    {
        var user = userContext.GetCurrentUser();
        var canJoinGame = await userRepository.CanjoinGame(user.Id);
        if (canJoinGame != null)
            throw new BadHttpRequestException($"User Already on a game with id {canJoinGame} leave it first!");
        if (!GameMemoryStorage.TryGetGame(gameId, out var game))
            throw new InvalidOperationException("Game not found.");
        if (newRole == Role.Spectator)
        {
            game.Participants.Add(user.Id);
            game.Spectators.Add(new PlayerDto
            {
                Id = user.Id,
                Username = user.UserName
            });
        }
        else if (newRole == Role.Judge)
        {
            throw new BadHttpRequestException("You cannot join a judge.");
        }
        else
        {
            if (game.Participants.Contains(user.Id))
            {
                RemovePlayerFromTeam(game.Spectators, user.Id);
                game.Participants.Remove(user.Id);
            }

            await userRepository.AddPlayerToGame(user.Id, game.GameId);
            game.Participants.Add(user.Id);
            if (team == Team.Team1)
                game.Team1.Add(new PlayerDto
                {
                    Id = user.Id,
                    Username = user.UserName
                });
            else
                game.Team2.Add(new PlayerDto
                {
                    Id = user.Id,
                    Username = user.UserName
                });
        }
    }

    public async Task LeaveGameAsync()
    {
        var user = userContext.GetCurrentUser();

        var gameId = await userRepository.CanjoinGame(user.Id);
        if (!gameId.HasValue || !GameMemoryStorage.TryGetGame(gameId.Value, out var game))
            throw new InvalidOperationException("Game not found.");

        await userRepository.SetUsersFree([user.Id]);

        game.Participants.Remove(user.Id);

        RemovePlayerFromTeam(game.Team1, user.Id);
        RemovePlayerFromTeam(game.Team2, user.Id);

        if (game.Judge.Id == user.Id) await EndGameAsync(gameId.Value);
    }

// Helper method to remove a player from a team
    private void RemovePlayerFromTeam(List<PlayerDto> team, int userId)
    {
        var player = team.FirstOrDefault(p => p.Id == userId);
        if (player != null) team.Remove(player);
    }

    private int GenerateUniqueGameId()
    {
        int gameId;
        Random random = new();
        do
        {
            lock (random)
            {
                gameId = random.Next(100000, 1000000);
            }
        } while (GameMemoryStorage.GetAllGames().ContainsKey(gameId));

        return gameId;
    }
}