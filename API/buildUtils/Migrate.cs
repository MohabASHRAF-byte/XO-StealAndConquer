using Core.Storage;
using Microsoft.EntityFrameworkCore;

namespace API.buildUtils;

public static class Migrate
{
    public static void AddMigration(this IServiceProvider services)
    {
        using (var scope = services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (dbContext.Database.HasPendingModelChanges())
                dbContext.Database.Migrate();
        }
    }
}