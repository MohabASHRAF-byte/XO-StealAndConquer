using System.Security.Claims;
using System.Text.Json;
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

    public async Task SelectCell(int gameId, int cellIndex, string team)
    {
        await Clients.Group(gameId.ToString()).SendAsync("CellSelected", cellIndex, team);
    }
}