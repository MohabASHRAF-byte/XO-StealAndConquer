using System.Security.Claims;
using System.Text.Json;
using Core.Dtos;
using Core.Models;
using Core.Repositories.User;
using Core.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Core.Hubs;

[Authorize]
public class GameHub(AppDbContext dbContext, IUserRepository userRepository) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            var user = await dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                var connectionIds = string.IsNullOrEmpty(user.ConnectionIds)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(user.ConnectionIds)!;
                if (!connectionIds.Contains(Context.ConnectionId))
                {
                    connectionIds.Add(Context.ConnectionId);
                    user.ConnectionIds = JsonSerializer.Serialize(connectionIds);
                    await dbContext.SaveChangesAsync();
                }

                var gameId = await userRepository.CanjoinGame(userId);
                if (gameId.HasValue) await Groups.AddToGroupAsync(Context.ConnectionId, $"game:{gameId.Value}");
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            var user = await dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                var connectionIds = string.IsNullOrEmpty(user.ConnectionIds)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(user.ConnectionIds)!;
                connectionIds.Remove(Context.ConnectionId);
                user.ConnectionIds = connectionIds.Any() ? JsonSerializer.Serialize(connectionIds) : null;
                await dbContext.SaveChangesAsync();

                var gameId = await userRepository.CanjoinGame(userId);
                if (gameId.HasValue) await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"game:{gameId.Value}");
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinGame(int gameId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString());
        await Clients.Group(gameId.ToString()).SendAsync("PlayerJoined", Context.ConnectionId);
    }

    public async Task SelectCell(int gameId, int cellIndex)
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value;

        if (!int.TryParse(userIdClaim, out var userId) || string.IsNullOrWhiteSpace(username)) return;

        var team = GameMemoryStorage.PlayerTeam(gameId, userId);

        if (!GameMemoryStorage.CellSelections.ContainsKey(gameId))
            GameMemoryStorage.CellSelections[gameId] = new Dictionary<int, List<SignalRDtos.CellSelectorDto>>();

        var cellMap = GameMemoryStorage.CellSelections[gameId];

        var clearedCells = new List<int>();

        foreach (var key in cellMap.Keys.ToList())
        {
            var removed = cellMap[key].RemoveAll(sel => sel.UserId == userId);
            if (removed > 0) clearedCells.Add(key);
        }

        foreach (var clearedCell in clearedCells)
            await Clients.Group($"game:{gameId}")
                .SendAsync("CellSelected", clearedCell, cellMap[clearedCell]);

        if (!cellMap.ContainsKey(cellIndex))
            cellMap[cellIndex] = new List<SignalRDtos.CellSelectorDto>();

        cellMap[cellIndex].Add(new SignalRDtos.CellSelectorDto
        {
            UserId = userId,
            Username = username!,
            Team = team
        });

        // Broadcast new selection
        await Clients.Group($"game:{gameId}")
            .SendAsync("CellSelected", cellIndex, cellMap[cellIndex]);
    }

    public async Task StartGame(int gameId, int team)
    {
        var userId = int.Parse(Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        GameMemoryStorage.TryGetGame(gameId, out var game);

        if (game.Judge.Id != userId)
            throw new HubException("Only the judge can start the round.");
        if (game.Round != Round.Notstarted)
            return;
        game.NextTeam = team == 1 ? 2 : 1;
        game.Round = team == 1 ? Round.Team1 : Round.Team2;
        await Clients.Group($"game:{gameId}").SendAsync("StartTimer", new
        {
            Team = team,
            Duration = game.RoundDuration
        });
    }

    public async Task SubmitAnswer(int gameId, int cellIdx, string answer)
    {
        GameMemoryStorage.TryGetGame(gameId, out var game);
        game.Answers.Add(new Answer
            {
                CellIndex = cellIdx,
                Content = answer,
                IsAccepted = false,
                Team = game.Round == Round.Team1 ? Team.Team1 : Team.Team2
            }
        );
        game.Round = Round.Judge;
        game.NextTeam = game.NextTeam == 1 ? 2 : 1;
        await Clients.Group($"game:{gameId}").SendAsync("submitted", game.Answers.Last());
    }

    public async Task Judge(int gameId, bool isCorrect)
    {
        GameMemoryStorage.TryGetGame(gameId, out var game);
        var lastAnswer = game.Answers.Last();

        if (isCorrect)
        {
            lastAnswer.IsAccepted = true;
            game.CellStates[lastAnswer.CellIndex] = lastAnswer.Team == Team.Team1
                ? CellStates.Team1
                : CellStates.Team2;
        }

        game.Round = game.NextTeam == 1 ? Round.Team1 : Round.Team2;
        await Clients.Group($"game:{gameId}")
            .SendAsync("judged", game.Answers.Last());
        var winner = Utils.Utils.CheckWinCondition(game);
        if (winner != CellStates.Empty)
            await Clients.Group($"game:{gameId}")
                .SendAsync("winner", winner);
    }
}