using System.Security.Claims;
using Core.Hubs;
using Core.Repositories.User;
using Core.Tests.Helpers;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Core.Tests;

public class GameHubTests
{
    private readonly TestUtils _testUtility;

    public GameHubTests()
    {
        // Initialize TestUtility to provide seeded database and mocks
        _testUtility = new TestUtils();
    }

    [Fact]
    public async Task JoinGame_ShouldAddUserToGroup_AndBroadcastPlayerJoined()
    {
        // --------- Arrange ---------
        // Seed the database with test data (10 users, game ID 101)
        await _testUtility.SeedDatabaseAsync();
    }
}