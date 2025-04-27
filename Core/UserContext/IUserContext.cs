namespace Core.UserContext;

public interface IUserContext
{
    public CurrentUser GetCurrentUser();
}