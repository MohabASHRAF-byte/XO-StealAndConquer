using Core.Storage;
using Microsoft.EntityFrameworkCore;

namespace API.buildUtils;

public static class Database
{
    public static void AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
        
    }
}