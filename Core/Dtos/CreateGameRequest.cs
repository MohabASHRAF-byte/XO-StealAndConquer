namespace Core.Dtos;

public class CreateGameRequest
{
    public List<string> RowLabels { get; set; } = [];
    public List<string> ColumnLabels { get; set; } = [];
    public int? RoundDuration { get; set; }
}