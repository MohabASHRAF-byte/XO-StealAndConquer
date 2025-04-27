using Core.Storage;

namespace Core.Dtos;

public class JoinGameRequest
{
    public Role Role { get; set; }
    public Team Team { get; set; }
}