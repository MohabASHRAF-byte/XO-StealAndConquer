using Core.Dtos;
using Core.Services;
using Core.Storage;
using Core.UserContext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core.Jobs;

public class StartupJob(
    ILogger<StartupJob> logger,
    IServiceProvider serviceProvider
) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Startup job is running...");
        logger.LogInformation("Running StartupJob...");

        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync(cancellationToken);

        logger.LogInformation("Startup seeding complete.");
        logger.LogInformation("Startup job finished.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}