using API.buildUtils;
using Core.Hubs;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();
builder.Services.AddCors();
builder.Services.AddRegisterServices();
builder.Services.AddSignalR().AddHubOptions<GameHub>(options => { options.EnableDetailedErrors = true; });
builder.AddAuth();
builder.AddDatabase();
builder.Services.AddHttpContextAccessor();
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration);
});
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Services.AddMigration();
app.UseCors("AllowSpecificOrigins");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<GameHub>("/gameHub").RequireCors("AllowSpecificOrigins");

app.Run();