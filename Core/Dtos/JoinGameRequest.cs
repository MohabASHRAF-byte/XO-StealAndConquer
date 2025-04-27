using Core.Storage;

namespace Core.Dtos;

public class JoinGameRequest(int gameId, Role role)
{
    public int GameId { get; set; } = gameId;
    public Role Role { get; set; } = role;
}