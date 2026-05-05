using Application_Layer.Commands.UserCommands.RevokeRefreshToken;
using Application_Layer.Interfaces;
using Domain_Layer.Common;
using FakeItEasy;

namespace Test_Layer.UserTests.UserUnitTests;

[TestFixture]
public class RevokeRefreshTokenCommandHandlerTests
{
    private IRefreshTokenService _refreshTokenService = null!;
    private RevokeRefreshTokenCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _refreshTokenService = A.Fake<IRefreshTokenService>();
        _handler = new RevokeRefreshTokenCommandHandler(_refreshTokenService);
    }

    [Test]
    public async Task Handle_WithSpecificRefreshToken_ShouldRevokeOnlyThatToken()
    {
        var command = new RevokeRefreshTokenCommand(
            refreshToken: "token-1",
            ipAddress: "127.0.0.1",
            reason: "User logout");

        A.CallTo(() => _refreshTokenService.RevokeRefreshTokenAsync("token-1", "127.0.0.1", "User logout"))
            .Returns(OperationResult.Success());

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.Successful, Is.True);
        A.CallTo(() => _refreshTokenService.RevokeRefreshTokenAsync("token-1", "127.0.0.1", "User logout"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _refreshTokenService.RevokeAllUserTokensAsync(A<string>._, A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WithUserIdOnly_ShouldRevokeAllUserTokens()
    {
        var command = new RevokeRefreshTokenCommand(
            userId: "user-123",
            ipAddress: "127.0.0.1",
            reason: "All sessions terminated");

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.Successful, Is.True);
        A.CallTo(() => _refreshTokenService.RevokeAllUserTokensAsync("user-123", "127.0.0.1", "All sessions terminated"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _refreshTokenService.RevokeRefreshTokenAsync(A<string>._, A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WithNeitherRefreshTokenNorUserId_ShouldReturnFailure()
    {
        var command = new RevokeRefreshTokenCommand();

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.That(result.Successful, Is.False);
        Assert.That(result.Error, Is.EqualTo("Either a refresh token or user ID is required."));
    }
}
