using Core.Dtos;
using Core.Hubs;
using Core.Models;
using Core.Repositories.User;
using Core.Storage;
using Core.UserContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace Core.Services;

public class GameService(IUserContext userContext, IUserRepository userRepository, IHubContext<GameHub> hubContext)
{
    public async Task<int> CreateGameAsync(List<string> rowLabels, List<string> colLabels, int roundDuration = 30)
    {
        var user = userContext.GetCurrentUser();
        var canCreateGame = await userRepository.CanjoinGame(user.Id);
        if (canCreateGame != null)
            throw new BadHttpRequestException("User already in a game; leave it first!");

        if (rowLabels.Count != 3 || colLabels.Count != 3)
            throw new BadHttpRequestException("Invalid number of row/col labels");

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

        if (!GameMemoryStorage.TryAddGame(gameId, game))
            throw new InvalidOperationException("Failed to create game.");

        await userRepository.AddPlayerToGame(user.Id, gameId);

        await hubContext.Clients.Group($"game:{gameId}")
            .SendAsync("GameCreated", new { GameId = gameId, Judge = judge });

        return gameId;
    }

    public async Task EndGameAsync(int gameId)
    {
        var user = userContext.GetCurrentUser();
        if (!GameMemoryStorage.TryGetGame(gameId, out var game))
            throw new InvalidOperationException("Game not found.");

        if (user.Id != game.Judge.Id)
            throw new InvalidOperationException("Only the Game Judge can end the game.");

        await userRepository.SetUsersFree(game.Participants);
        GameMemoryStorage.TryRemoveGame(gameId);

        await hubContext.Clients.Group($"game:{gameId}").SendAsync("GameEnded", new { GameId = gameId });
    }

    public Game LoadGameAsync(int gameId)
    {
        var user = userContext.GetCurrentUser();
        if (!GameMemoryStorage.TryGetGame(gameId, out var game))
            throw new InvalidOperationException("Game not found.");

        if (!game.Participants.Contains(user.Id))
            throw new InvalidOperationException("User must join the game to watch it.");

        return game;
    }

    public async Task<LoadGameDto> LoadGameAsync()
    {
        var user = userContext.GetCurrentUser();
        var inGame = await userRepository.CanjoinGame(user.Id);
        if (inGame == null) return new LoadGameDto();

        var gameId = (int)inGame;
        if (!GameMemoryStorage.TryGetGame(gameId, out var game))
            throw new InvalidOperationException("Game not found.");

        if (!game.Participants.Contains(user.Id))
            throw new InvalidOperationException("User must join the game to watch it.");

        return new LoadGameDto(game)
        {
            InGame = true,
            IsJudge = game.Judge.Id == user.Id
        };
    }

    public async Task JoinGameAsync(int gameId, Role newRole, Team team)
    {
        var user = userContext.GetCurrentUser();
        var canJoinGame = await userRepository.CanjoinGame(user.Id);
        if (canJoinGame != null)
            return;
        if (!GameMemoryStorage.TryGetGame(gameId, out var game))
            throw new InvalidOperationException("Game not found.");

        var player = new PlayerDto
        {
            Id = user.Id,
            Username = user.UserName
        };

        if (newRole == Role.Spectator)
        {
            game.Participants.Add(user.Id);
            game.Spectators.Add(player);
        }
        else if (newRole == Role.Judge)
        {
            throw new BadHttpRequestException("Cannot join as Judge.");
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
                game.Team1.Add(player);
            else
                game.Team2.Add(player);
        }

        await hubContext.Clients.Group($"game:{gameId}").SendAsync("PlayerJoined", new
        {
            Player = player,
            Role = newRole.ToString(),
            Team = team.ToString(),
            GameId = gameId
        });
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
        RemovePlayerFromTeam(game.Spectators, user.Id);

        await hubContext.Clients.Group($"game:{gameId.Value}").SendAsync("PlayerLeft", new
        {
            PlayerId = user.Id,
            GameId = gameId.Value
        });

        if (game.Judge.Id == user.Id)
            await EndGameAsync(gameId.Value);
    }

    public async Task ChangeTeamAsync(int gameId, Team newTeam)
    {
        var user = userContext.GetCurrentUser();
        if (!GameMemoryStorage.TryGetGame(gameId, out var game))
            throw new InvalidOperationException("Game not found.");

        if (!game.Participants.Contains(user.Id))
            throw new InvalidOperationException("User is not in the game.");

        if (game.Judge.Id == user.Id)
            throw new InvalidOperationException("Judge cannot change teams.");

        RemovePlayerFromTeam(game.Team1, user.Id);
        RemovePlayerFromTeam(game.Team2, user.Id);
        RemovePlayerFromTeam(game.Spectators, user.Id);

        var player = new PlayerDto
        {
            Id = user.Id,
            Username = user.UserName
        };

        if (newTeam == Team.Spectator)
            game.Spectators.Add(player);
        else if (newTeam == Team.Team1)
            game.Team1.Add(player);
        else
            game.Team2.Add(player);

        await hubContext.Clients.Group($"game:{gameId}").SendAsync("TeamChanged", new
        {
            PlayerId = user.Id,
            NewTeam = newTeam.ToString(),
            GameId = gameId
        });
    }

    public async Task StartRoundAsync(int gameId, int roundNumber)
    {
        var user = userContext.GetCurrentUser();
        if (!GameMemoryStorage.TryGetGame(gameId, out var game))
            throw new InvalidOperationException("Game not found.");

        if (user.Id != game.Judge.Id)
            throw new InvalidOperationException("Only the Judge can start a round.");

        if (game.Round != Round.Judge)
            throw new InvalidOperationException("Cannot start round; not in Judge phase.");

        game.CurrentRoundNumber = roundNumber;
        game.Round = roundNumber % 2 == 1 ? Round.Team1 : Round.Team2;

        await hubContext.Clients.Group($"game:{gameId}").SendAsync("RoundStarted", new
        {
            GameId = gameId,
            RoundNumber = roundNumber,
            CurrentTeam = game.Round.ToString(),
            Duration = game.RoundDuration
        });
    }


    public async Task JudgeAnswerAsync(int gameId, int cellIndex, bool accept)
    {
        var user = userContext.GetCurrentUser();
        if (!GameMemoryStorage.TryGetGame(gameId, out var game))
            throw new InvalidOperationException("Game not found.");

        if (user.Id != game.Judge.Id)
            throw new InvalidOperationException("Only the Judge can judge answers.");

        if (game.Round != Round.Judge)
            throw new InvalidOperationException("Not in Judge phase.");


        var answer = game.Answers.LastOrDefault();
        if (answer == null)
            throw new InvalidOperationException("Answer not found.");

        answer.IsAccepted = accept;
        if (accept)
            game.CellStates[cellIndex] = answer.Team == Team.Team1 ? CellStates.Team1 : CellStates.Team2;

        game.Round = Round.Finished;

        await hubContext.Clients.Group($"game:{gameId}").SendAsync("AnswerJudged", new
        {
            GameId = gameId,
            CellIndex = cellIndex,
            Accepted = accept,
            game.CellStates
        });

        if (Utils.Utils.CheckWinCondition(game) != CellStates.Empty)
        {
            await EndGameAsync(gameId);
        }
        else
        {
            game.Round = Round.Judge;
            await hubContext.Clients.Group($"game:{gameId}").SendAsync("RoundFinished", new
            {
                GameId = gameId,
                NextRound = game.CurrentRoundNumber + 1
            });
        }
    }

    public async Task UpdateGridAsync(int gameId, List<CellStates> newCellStates)
    {
        var user = userContext.GetCurrentUser();
        if (!GameMemoryStorage.TryGetGame(gameId, out var game))
            throw new InvalidOperationException("Game not found.");

        if (user.Id != game.Judge.Id)
            throw new InvalidOperationException("Only the Judge can update the grid.");

        if (newCellStates.Count != 9)
            throw new InvalidOperationException("Grid must have 9 cells.");

        game.CellStates = newCellStates;

        await hubContext.Clients.Group($"game:{gameId}").SendAsync("GridUpdated", new
        {
            GameId = gameId,
            CellStates = newCellStates
        });
    }

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