namespace Core.Dtos;

public class SignalRDtos
{
    public class CellSelectorDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Team { get; set; }
    }
}