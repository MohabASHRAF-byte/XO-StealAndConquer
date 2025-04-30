using Core.Storage;

namespace Core.Dtos;

public class CreateGameRequest
{
    public List<string> RowLabels { get; set; } = [];
    public List<string> ColumnLabels { get; set; } = [];
    public int? RoundDuration { get; set; }
}

public class JoinGameRequest
{
    public Role Role { get; set; }
    public Team Team { get; set; }
}

public class ChangeTeamRequest
{
    public Team Team { get; set; }
}

public class SubmitAnswerRequest
{
    public int CellIndex { get; set; } // 0-8
    public string Content { get; set; } = string.Empty;
}

public class JudgeAnswerRequest
{
    public int CellIndex { get; set; } // 0-8
    public bool Accept { get; set; }
}

public class StartRoundRequest
{
    public int RoundNumber { get; set; }
}