using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Core.UserContext;

public class UserContext(
    IHttpContextAccessor httpContextAccessor
) : IUserContext
{
    private string _token;

    public CurrentUser GetCurrentUser()
    {
        //todo: for testing and development then will be removed 
        if (!string.IsNullOrEmpty(_token))
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(_token);

            var id1 = int.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
            var username1 = jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value;

            return new CurrentUser(id1, username1);
        }

        var user = httpContextAccessor.HttpContext?.User;

        if (user == null) throw new BadHttpRequestException("UserNotFound");

        if (user.Identity == null || !user.Identity.IsAuthenticated)
            throw new BadHttpRequestException("UserNotFound");

        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = user.FindFirst(ClaimTypes.Name)?.Value;
        int numId;
        int.TryParse(id, out numId);
        return new CurrentUser(
            numId,
            username!
        );
    }

    public void SetToken(string token)
    {
        _token = token;
    }
}