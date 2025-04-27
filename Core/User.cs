public class User(int id, string username, string passwordHash, string? connectionIds)
{
    public int Id { get; set; } = id;
    public string Username { get; set; } = username;
    public string PasswordHash { get; set; } = passwordHash;
    public string? ConnectionIds { get; set; } = connectionIds; // JSON array of SignalR connection IDs
}