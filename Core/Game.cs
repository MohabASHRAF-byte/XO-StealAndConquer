using Core;

public class Game(int id, int judgeId, List<Participant> participants, bool isActive)
{
    public int Id { get; set; } = id;
    public int JudgeId { get; set; } = judgeId;
    public List<Participant> Participants { get; set; } = participants;
    public bool IsActive { get; set; } = isActive;
}

public class Participant(int userId, Role role)
{
    public int UserId { get; set; } = userId;
    public Role Role { get; set; } = role;
}