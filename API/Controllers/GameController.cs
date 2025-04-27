using System.IdentityModel.Tokens.Jwt;
using Core.Dtos;
using Core.Services;
using Core.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GameController(GameService gameService) : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateGame()
    {
        var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "0");
        if (userId == 0)
            return Unauthorized();

        var gameId = await gameService.CreateGameAsync(userId);
        // Generate new JWT with Judge role
        var token = await gameService._authService.GenerateGameTokenAsync(userId, gameId, Role.Judge);
        return Ok(new { GameId = gameId, Token = token });
    }

    [HttpPost("join")]
    [Authorize]
    public async Task<IActionResult> JoinGame([FromBody] JoinGameRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "0");
            if (userId == 0)
                return Unauthorized();

            var token = await gameService.JoinGameAsync(userId, request);
            return Ok(new { Token = token });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("test-judge")]
    [Authorize(Roles = nameof(Role.Judge))]
    public IActionResult TestJudge()
    {
        return Ok("Only Judges can access this!");
    }
}