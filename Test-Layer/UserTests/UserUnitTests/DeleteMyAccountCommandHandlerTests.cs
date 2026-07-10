using Application_Layer.Commands.UserCommands.DeleteMyAccount;
using Application_Layer.Interfaces;
using Domain_Layer.Common;
using Domain_Layer.Models;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Test_Layer.UserTests.UserUnitTests;

[TestFixture]
public class DeleteMyAccountCommandHandlerTests
{
    private IUserRepository _userRepository = null!;
    private IFileService _fileService = null!;
    private IRefreshTokenService _refreshTokenService = null!;
    private DeleteMyAccountCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = A.Fake<IUserRepository>();
        _fileService = A.Fake<IFileService>();
        _refreshTokenService = A.Fake<IRefreshTokenService>();
        _handler = new DeleteMyAccountCommandHandler(
            _userRepository, _fileService, _refreshTokenService,
            NullLogger<DeleteMyAccountCommandHandler>.Instance);
    }

    [Test]
    public async Task Handle_RevokesSessions_DeletesAvatar_AndAnonymizes()
    {
        var user = new UserModel { Id = "user-1", AvatarUrl = "images/avatars/user-1" };
        A.CallTo(() => _userRepository.FindByIdAsync("user-1")).Returns(user);
        A.CallTo(() => _userRepository.AnonymizeAndDeactivateAsync("user-1"))
            .Returns(OperationResult.Success());

        var result = await _handler.Handle(new DeleteMyAccountCommand("user-1"), CancellationToken.None);

        Assert.That(result.Successful, Is.True);
        A.CallTo(() => _fileService.DeleteAsync("images/avatars/user-1", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _refreshTokenService.RevokeAllUserTokensAsync("user-1", A<string>._, A<string>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.AnonymizeAndDeactivateAsync("user-1"))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task Handle_WhenUserMissing_ReturnsNotFound_AndDoesNothing()
    {
        A.CallTo(() => _userRepository.FindByIdAsync("missing")).Returns((UserModel?)null);

        var result = await _handler.Handle(new DeleteMyAccountCommand("missing"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.FailureType, Is.EqualTo(OperationFailureType.NotFound));
        });
        A.CallTo(() => _refreshTokenService.RevokeAllUserTokensAsync(A<string>._, A<string>._, A<string>._))
            .MustNotHaveHappened();
        A.CallTo(() => _userRepository.AnonymizeAndDeactivateAsync(A<string>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenAvatarDeletionFails_StillRevokesAndAnonymizes()
    {
        var user = new UserModel { Id = "user-1", AvatarUrl = "images/avatars/user-1" };
        A.CallTo(() => _userRepository.FindByIdAsync("user-1")).Returns(user);
        A.CallTo(() => _fileService.DeleteAsync(A<string>._, A<CancellationToken>._))
            .Throws(new InvalidOperationException("storage down"));
        A.CallTo(() => _userRepository.AnonymizeAndDeactivateAsync("user-1"))
            .Returns(OperationResult.Success());

        var result = await _handler.Handle(new DeleteMyAccountCommand("user-1"), CancellationToken.None);

        Assert.That(result.Successful, Is.True, "en trasig blob-radering får inte fälla kontoraderingen");
        A.CallTo(() => _refreshTokenService.RevokeAllUserTokensAsync("user-1", A<string>._, A<string>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.AnonymizeAndDeactivateAsync("user-1"))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task Handle_WhenNoAvatar_SkipsFileDeletion()
    {
        var user = new UserModel { Id = "user-1", AvatarUrl = null };
        A.CallTo(() => _userRepository.FindByIdAsync("user-1")).Returns(user);
        A.CallTo(() => _userRepository.AnonymizeAndDeactivateAsync("user-1"))
            .Returns(OperationResult.Success());

        var result = await _handler.Handle(new DeleteMyAccountCommand("user-1"), CancellationToken.None);

        Assert.That(result.Successful, Is.True);
        A.CallTo(() => _fileService.DeleteAsync(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _userRepository.AnonymizeAndDeactivateAsync("user-1")).MustHaveHappenedOnceExactly();
    }
}
