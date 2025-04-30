using Core.Storage;

namespace Core.Services;

public class StartUpService(
    AppDbContext dbContext)
{
    private void CleanDatabase()
    {
    }
}