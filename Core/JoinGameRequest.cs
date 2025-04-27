namespace Core;

public class JoinGameRequest(int gameId, Role role)
{
    public int GameId { get; set; } = gameId;
    public Role Role { get; set; } = role;
}