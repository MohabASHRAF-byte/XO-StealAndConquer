using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Core.UserContext;

public class UserContext(
    IHttpContextAccessor httpContextAccessor
) : IUserContext
{
    public CurrentUser GetCurrentUser()
    {
        var user = httpContextAccessor.HttpContext?.User;

        if (user == null) throw new BadHttpRequestException("UserNotFound");

        if (user.Identity == null || !user.Identity.IsAuthenticated) throw new BadHttpRequestException("UserNotFound");

        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = user.FindFirst(ClaimTypes.Name)?.Value;
        int numId;
        int.TryParse(id, out numId);
        return new CurrentUser(
            numId,
            username!
        );
    }
}