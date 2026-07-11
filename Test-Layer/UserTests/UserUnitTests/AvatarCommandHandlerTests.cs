using Application_Layer.Commands.UserCommands.Avatar.DeleteMyAvatar;
using Application_Layer.Commands.UserCommands.Avatar.UploadMyAvatar;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Domain_Layer.Common;
using Domain_Layer.Models;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Test_Layer.UserTests.UserUnitTests;

[TestFixture]
public class AvatarCommandHandlerTests
{
    private IUserRepository _userRepository = null!;
    private IFileService _fileService = null!;
    private UploadMyAvatarCommandHandler _uploadHandler = null!;
    private DeleteMyAvatarCommandHandler _deleteHandler = null!;

    private static readonly FileUploadRequest ValidFile =
        new("avatar.png", "image/png", [1, 2, 3]);

    [SetUp]
    public void SetUp()
    {
        _userRepository = A.Fake<IUserRepository>();
        _fileService = A.Fake<IFileService>();
        _uploadHandler = new UploadMyAvatarCommandHandler(_userRepository, _fileService);
        _deleteHandler = new DeleteMyAvatarCommandHandler(
            _userRepository, _fileService, NullLogger<DeleteMyAvatarCommandHandler>.Instance);
    }

    [Test]
    public async Task Upload_WhenUserExists_SavesBlobPathAndReturnsSignedUrl()
    {
        var user = new UserModel { Id = "user-1" };
        A.CallTo(() => _userRepository.FindByIdAsync("user-1")).Returns(user);
        A.CallTo(() => _fileService.UploadUserAvatarAsync("user-1", ValidFile, A<CancellationToken>._))
            .Returns(("images/avatars/user-1", "https://signed-avatar-url"));
        A.CallTo(() => _userRepository.UpdateUserAsync(user)).Returns(OperationResult.Success());

        var result = await _uploadHandler.Handle(
            new UploadMyAvatarCommand("user-1", ValidFile), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.True);
            Assert.That(result.Data!.AvatarUrl, Is.EqualTo("https://signed-avatar-url"));
            Assert.That(user.AvatarUrl, Is.EqualTo("images/avatars/user-1"));
        });
    }

    [Test]
    public async Task Upload_WhenUserIsMissing_ReturnsNotFound()
    {
        A.CallTo(() => _userRepository.FindByIdAsync("missing")).Returns((UserModel?)null);

        var result = await _uploadHandler.Handle(
            new UploadMyAvatarCommand("missing", ValidFile), CancellationToken.None);

        Assert.That(result.Successful, Is.False);
        A.CallTo(() => _fileService.UploadUserAvatarAsync(A<string>._, A<FileUploadRequest>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task Delete_WhenUserHasAvatar_DeletesBlobAndClearsField()
    {
        var user = new UserModel { Id = "user-1", AvatarUrl = "images/avatars/user-1" };
        A.CallTo(() => _userRepository.FindByIdAsync("user-1")).Returns(user);
        A.CallTo(() => _userRepository.UpdateUserAsync(user)).Returns(OperationResult.Success());

        var result = await _deleteHandler.Handle(new DeleteMyAvatarCommand("user-1"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.True);
            Assert.That(user.AvatarUrl, Is.Null);
        });
        A.CallTo(() => _fileService.DeleteAsync("images/avatars/user-1", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task Delete_WhenBlobDeletionFails_StillClearsField()
    {
        var user = new UserModel { Id = "user-1", AvatarUrl = "images/avatars/user-1" };
        A.CallTo(() => _userRepository.FindByIdAsync("user-1")).Returns(user);
        A.CallTo(() => _fileService.DeleteAsync(A<string>._, A<CancellationToken>._))
            .Throws(new InvalidOperationException("storage down"));
        A.CallTo(() => _userRepository.UpdateUserAsync(user)).Returns(OperationResult.Success());

        var result = await _deleteHandler.Handle(new DeleteMyAvatarCommand("user-1"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.True);
            Assert.That(user.AvatarUrl, Is.Null);
        });
    }

    [Test]
    public async Task Delete_WhenUserHasNoAvatar_IsNoOp()
    {
        var user = new UserModel { Id = "user-1", AvatarUrl = null };
        A.CallTo(() => _userRepository.FindByIdAsync("user-1")).Returns(user);

        var result = await _deleteHandler.Handle(new DeleteMyAvatarCommand("user-1"), CancellationToken.None);

        Assert.That(result.Successful, Is.True);
        A.CallTo(() => _fileService.DeleteAsync(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
