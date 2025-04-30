using Core.Jobs;
using Core.Repositories.User;
using Core.Services;
using Core.UserContext;

namespace API.buildUtils;

public static class RegisterServices
{
    public static void AddRegisterServices(this IServiceCollection services)
    {
        services.AddScoped<GameService>();
        services.AddScoped<AuthService>();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<DatabaseSeeder>();
        services.AddHostedService<StartupJob>();
    }
}