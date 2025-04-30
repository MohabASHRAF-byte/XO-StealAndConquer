using System.Security.Claims;
using Core.Hubs;
using Core.Repositories.User;
using Core.Tests.Helpers;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace Core.Tests;

public class GameHubTests
{
    [Fact]
    public async Task JoinGame_ShouldAddUserToGroup_AndBroadcastPlayerJoined()
    {
        // --------- Arrange ---------
        var testGameId = 101;
        var testUserId = 1;
        var connectionId = "conn-123";

        // Mock user claims (ID + Username)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, testUserId.ToString()),
            new Claim(ClaimTypes.Name, "TestUser")
        };
        var identity = new ClaimsIdentity(claims, "mock");
        var userPrincipal = new ClaimsPrincipal(identity);

        // Mock HubCallerContext
        var mockContext = new Mock<HubCallerContext>();
        mockContext.Setup(c => c.User).Returns(userPrincipal);
        mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

        // Mock Clients and ClientProxy
        var mockClientProxy = new Mock<IClientProxy>();
        var mockClients = new Mock<IHubCallerClients>();
        mockClients.Setup(c => c.Group(testGameId.ToString())).Returns(mockClientProxy.Object);

        // Mock Group Manager
        var mockGroups = new Mock<IGroupManager>();
        mockGroups.Setup(g => g.AddToGroupAsync(connectionId, testGameId.ToString(), default))
            .Returns(Task.CompletedTask);

        // Mock EF Core DB and User Repository
        var mockDb = MockDbFactory.Create();
        var mockUserRepo = new Mock<IUserRepository>();

        // Return true game for user (optional depending on GameHub logic)
        mockUserRepo.Setup(r => r.CanjoinGame(testUserId)).ReturnsAsync(testGameId);

        // Create hub instance with mocks
        var hub = new GameHub(mockDb.Object, mockUserRepo.Object)
        {
            Context = mockContext.Object,
            Clients = mockClients.Object,
            Groups = mockGroups.Object
        };

        // --------- Act ---------
        await hub.JoinGame(testGameId);

        // --------- Assert ---------
        mockGroups.Verify(g => g.AddToGroupAsync(connectionId, testGameId.ToString(), default), Times.Once);
        mockClients.Verify(c => c.Group(testGameId.ToString()), Times.Once);
        mockClientProxy.Verify(proxy =>
                proxy.SendCoreAsync("PlayerJoined", It.Is<object[]>(args =>
                    args.Length == 1 && args[0]!.ToString() == connectionId
                ), default),
            Times.Once);
    }
}