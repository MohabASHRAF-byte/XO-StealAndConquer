using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Core.Storage;
using Microsoft.AspNetCore.SignalR;

namespace Core.Hubs;

public class GameHub(AppDbContext dbContext) : Hub
{
    private readonly AppDbContext _dbContext = dbContext;

    public override async Task OnConnectedAsync()
    {
        var userIdClaim = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                var connectionIds = string.IsNullOrEmpty(user.ConnectionIds)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(user.ConnectionIds)!;
                if (!connectionIds.Contains(Context.ConnectionId))
                {
                    connectionIds.Add(Context.ConnectionId);
                    user.ConnectionIds = JsonSerializer.Serialize(connectionIds);
                    await _dbContext.SaveChangesAsync();
                }
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userIdClaim = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                var connectionIds = string.IsNullOrEmpty(user.ConnectionIds)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(user.ConnectionIds)!;
                connectionIds.Remove(Context.ConnectionId);
                user.ConnectionIds = connectionIds.Any() ? JsonSerializer.Serialize(connectionIds) : null;
                await _dbContext.SaveChangesAsync();
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}