using Core.Dtos;
using Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameController(GameService gameService) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
    {
        var gameId =
            await gameService.CreateGameAsync(request.RowLabels, request.ColumnLabels, request.RoundDuration ?? 30);
        return Ok(new { GameId = gameId });
    }

    [HttpDelete("{gameId}")]
    [Authorize]
    public async Task<IActionResult> EndGame([FromRoute] int gameId)
    {
        await gameService.EndGameAsync(gameId);
        return Ok();
    }

    [HttpGet("{gameId}")]
    [Authorize]
    public IActionResult GetGame([FromRoute] int gameId)
    {
        return Ok(gameService.LoadGameAsync(gameId));
    }

    [HttpPost("{gameId}/join")]
    [Authorize]
    public async Task<IActionResult> JoinGame([FromRoute] int gameId, [FromBody] JoinGameRequest request)
    {
        await gameService.JoinGameAsync(gameId, request.Role, request.Team);
        return Ok(gameService.LoadGameAsync(gameId));
    }

    [HttpGet("load")]
    [Authorize]
    public async Task<IActionResult> LoadGame()
    {
        return Ok(await gameService.LoadGameAsync());
    }

    [HttpPost("{gameId}/change-team")]
    [Authorize]
    [HttpPost("leave")]
    [Authorize]
    public async Task<IActionResult> LeaveGame()
    {
        await gameService.LeaveGameAsync();
        return Ok();
    }
}