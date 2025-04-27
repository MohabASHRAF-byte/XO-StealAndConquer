namespace Core.Repositories.User;

public interface IUserRepository
{
    public Task<int?> CanjoinGame(int userId);
    public Task AddPlayerToGame(int userId, int gameId);
    public Task SetUsersFree(List<int> userIds);
}