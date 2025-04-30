namespace API.buildUtils;

public static class AllowAllCorsOrigin
{
    public static void AllowAllOriginCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins", policy =>
            {
                policy.WithOrigins("http://localhost:5173") // Exact origin, no trailing slash
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetIsOriginAllowedToAllowWildcardSubdomains();
            });
        });
    }
}