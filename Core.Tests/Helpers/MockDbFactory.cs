using Core.Storage;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Core.Tests.Helpers;

public static class MockDbFactory
{
    public static Mock<AppDbContext> Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb_" + Guid.NewGuid())
            .Options;

        var context = new AppDbContext(options);
        var mock = new Mock<AppDbContext>(options) { CallBase = true };

        return mock;
    }
}