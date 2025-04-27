using Core.Storage;
using Microsoft.EntityFrameworkCore;

namespace Core.Repositories.User;

public class UserRepository(
    AppDbContext dbContext
) : IUserRepository
{
    public async Task<int?> CanjoinGame(int userId)
    {
        var user = await GetUser(userId);
        return user.GameId;
    }

    public async Task AddPlayerToGame(int userId, int gameId)
    {
        var user = await GetUser(userId);
        user.GameId = gameId;
        await dbContext.SaveChangesAsync();
    }

    public async Task SetUsersFree(List<int> userIds)
    {
        var users = await dbContext.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();
        foreach (var user in users) user.GameId = null;

        await dbContext.SaveChangesAsync();
    }

    private async Task<Models.User> GetUser(int id)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user == null)
            throw new KeyNotFoundException("user not found");
        return user;
    }
}