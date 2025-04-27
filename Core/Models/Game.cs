using Core.Repositories.User;
using Core.Storage;

namespace Core.Models;

public class Game
{
    public int RoundDuration = 30;
    public int GameId { get; set; }
    public PlayerDto Judge { get; set; } = new();
    public List<PlayerDto> Team1 { get; set; } = [];
    public List<PlayerDto> Team2 { get; set; } = [];
    public List<PlayerDto> Spectators { get; set; } = [];
    public List<int> Participants { get; set; } = [];
    public List<string> RowLabels { get; set; } = [];
    public List<string> ColumnLabels { get; set; } = [];
    public List<CellStates> CellStates { get; set; } = [];
    public Round Round { get; set; } = Round.Judge;
}