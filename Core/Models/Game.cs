using Core.Repositories.User;
using Core.Storage;

namespace Core.Models;

public class Game
{
    public int RoundDuration { get; set; } = 30;
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
    public int CurrentRoundNumber { get; set; } = 0;
    public List<Answer> Answers { get; set; } = []; // Round number -> List of answers
    public int NextTeam { set; get; } = 0;
}

public class Answer
{
    public int CellIndex { get; set; } // 0-8 (3x3 grid)
    public Team Team { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsAccepted { get; set; }
}

public enum CellStates
{
    Empty,
    Team1,
    Team2
}

public enum Round
{
    Notstarted,
    Judge,
    Team1,
    Team2,
    Finished
}