using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class AuthService(AppDbContext dbContext, IConfiguration configuration)
{
    public async Task<int> RegisterAsync(RegisterRequest request)
    {
        // Check if username exists
        if (await dbContext.Users.AnyAsync(u => u.Username == request.Username))
            throw new InvalidOperationException("Username already exists.");

        // Create user
        var user = new User(
            0, // ID will be auto-incremented by SQLite
            request.Username,
            BCrypt.Net.BCrypt.HashPassword(request.Password),
            null
        );

        // Store in SQLite
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return user.Id;
    }

    public async Task<string> LoginAsync(LoginRequest request)
    {
        // Find user
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid username or password.");

        // Generate JWT (no role initially)
        return GenerateJwtToken(user, null, null);
    }

    public async Task<string> GenerateGameTokenAsync(int userId, int gameId, Role role)
    {
        // Find user
        var user = await dbContext.Users.FindAsync(userId);
        if (user == null)
            throw new UnauthorizedAccessException("User not found.");

        // Generate JWT with role and gameId
        return GenerateJwtToken(user, role, gameId);
    }

    private string GenerateJwtToken(User user, Role? role, int? gameId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (role.HasValue)
            claims.Add(new Claim(ClaimTypes.Role, role.Value.ToString()));
        if (gameId.HasValue)
            claims.Add(new Claim("gameId", gameId.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}