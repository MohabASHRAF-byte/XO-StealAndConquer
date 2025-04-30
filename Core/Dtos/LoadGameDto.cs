using Core.Models;

namespace Core.Dtos;

public class LoadGameDto(Game? game = null)
{
    public bool InGame { get; set; } = false;
    public bool IsJudge { get; set; } = false;
    public Game? Game { get; set; } = game;
}