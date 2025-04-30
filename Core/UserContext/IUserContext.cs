namespace Core.UserContext;

public interface IUserContext
{
    public CurrentUser GetCurrentUser();
    void SetToken(string token);
}