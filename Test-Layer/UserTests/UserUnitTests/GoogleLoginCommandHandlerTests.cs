using Application_Layer.Commands.UserCommands.GoogleLogin;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Jwt;
using Domain_Layer.Common;
using Domain_Layer.Models;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Test_Layer.UserTests.UserUnitTests;

[TestFixture]
public class GoogleLoginCommandHandlerTests
{
    private IGoogleIdTokenValidator _googleValidator = null!;
    private IUserRepository _userRepository = null!;
    private IJwtTokenGenerator _jwtTokenGenerator = null!;
    private IRefreshTokenService _refreshTokenService = null!;
    private GoogleLoginCommandHandler _handler = null!;

    private static readonly GoogleUserInfo VerifiedGoogleUser =
        new("google-sub-123", "anna@example.com", EmailVerified: true, "Anna", "Andersson");

    [SetUp]
    public void SetUp()
    {
        _googleValidator = A.Fake<IGoogleIdTokenValidator>();
        _userRepository = A.Fake<IUserRepository>();
        _jwtTokenGenerator = A.Fake<IJwtTokenGenerator>();
        _refreshTokenService = A.Fake<IRefreshTokenService>();
        _handler = new GoogleLoginCommandHandler(
            _googleValidator, _userRepository, _jwtTokenGenerator, _refreshTokenService,
            NullLogger<GoogleLoginCommandHandler>.Instance);

        A.CallTo(() => _refreshTokenService.GenerateRefreshTokenAsync(A<string>._, A<string>._, A<string>._))
            .Returns(("raw_refresh", new UserRefreshToken()));
        A.CallTo(() => _jwtTokenGenerator.GenerateToken(A<string>._, A<string>._, A<IEnumerable<string>>._))
            .Returns("access_token");
    }

    private static GoogleLoginCommand Command() => new("some-id-token", "127.0.0.1", "test-agent");

    [Test]
    public async Task Handle_WhenTokenIsInvalid_FailsWithoutUserLookup()
    {
        A.CallTo(() => _googleValidator.ValidateAsync(A<string>._, A<CancellationToken>._))
            .Returns((GoogleUserInfo?)null);

        var result = await _handler.Handle(Command(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.Error, Does.Contain("verifieras"));
        });
        A.CallTo(() => _userRepository.FindByExternalLoginAsync(A<string>._, A<string>._)).MustNotHaveHappened();
        A.CallTo(() => _refreshTokenService.GenerateRefreshTokenAsync(A<string>._, A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenEmailIsUnverified_IsRejected()
    {
        A.CallTo(() => _googleValidator.ValidateAsync(A<string>._, A<CancellationToken>._))
            .Returns(VerifiedGoogleUser with { EmailVerified = false });

        var result = await _handler.Handle(Command(), CancellationToken.None);

        Assert.That(result.Successful, Is.False);
        A.CallTo(() => _userRepository.FindByEmailAsync(A<string>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenUserIsAlreadyLinked_IssuesTokens()
    {
        var user = new UserModel { Id = "user-1", Email = "anna@example.com" };
        A.CallTo(() => _googleValidator.ValidateAsync(A<string>._, A<CancellationToken>._))
            .Returns(VerifiedGoogleUser);
        A.CallTo(() => _userRepository.FindByExternalLoginAsync("Google", "google-sub-123"))
            .Returns(user);

        var result = await _handler.Handle(Command(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.True);
            Assert.That(result.Data!.AccessToken, Is.EqualTo("access_token"));
            Assert.That(result.Data.RefreshToken, Is.EqualTo("raw_refresh"));
        });
        // Redan kopplad — ingen länkning eller nyregistrering ska ske.
        A.CallTo(() => _userRepository.AddExternalLoginAsync(A<string>._, A<string>._, A<string>._))
            .MustNotHaveHappened();
        A.CallTo(() => _userRepository.RegisterExternalUserAsync(A<UserModel>._, A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenEmailMatchesExistingAccount_LinksGoogleAndIssuesTokens()
    {
        var existing = new UserModel { Id = "user-2", Email = "anna@example.com" };
        A.CallTo(() => _googleValidator.ValidateAsync(A<string>._, A<CancellationToken>._))
            .Returns(VerifiedGoogleUser);
        A.CallTo(() => _userRepository.FindByExternalLoginAsync(A<string>._, A<string>._))
            .Returns((UserModel?)null);
        A.CallTo(() => _userRepository.FindByEmailAsync("anna@example.com")).Returns(existing);
        A.CallTo(() => _userRepository.AddExternalLoginAsync("user-2", "Google", "google-sub-123"))
            .Returns(OperationResult.Success());

        var result = await _handler.Handle(Command(), CancellationToken.None);

        Assert.That(result.Successful, Is.True);
        A.CallTo(() => _userRepository.AddExternalLoginAsync("user-2", "Google", "google-sub-123"))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _userRepository.RegisterExternalUserAsync(A<UserModel>._, A<string>._, A<string>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenUserIsNew_ProvisionsAccountAndIssuesTokens()
    {
        A.CallTo(() => _googleValidator.ValidateAsync(A<string>._, A<CancellationToken>._))
            .Returns(VerifiedGoogleUser);
        A.CallTo(() => _userRepository.FindByExternalLoginAsync(A<string>._, A<string>._))
            .Returns((UserModel?)null);
        A.CallTo(() => _userRepository.FindByEmailAsync(A<string>._)).Returns((UserModel?)null);
        A.CallTo(() => _userRepository.RegisterExternalUserAsync(
                A<UserModel>.That.Matches(u =>
                    u.Email == "anna@example.com" &&
                    u.UserName == "anna@example.com" &&
                    u.FirstName == "Anna" &&
                    u.LastName == "Andersson"),
                "Google", "google-sub-123"))
            .Invokes((UserModel u, string _, string _) => u.Id = "new-user-id")
            .Returns(OperationResult.Success());

        var result = await _handler.Handle(Command(), CancellationToken.None);

        Assert.That(result.Successful, Is.True);
        A.CallTo(() => _refreshTokenService.GenerateRefreshTokenAsync("new-user-id", A<string>._, A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task Handle_WhenLinkedUserIsDeleted_IsRejectedWithoutTokens()
    {
        var deleted = new UserModel { Id = "user-3", Email = "anna@example.com", IsDeleted = true };
        A.CallTo(() => _googleValidator.ValidateAsync(A<string>._, A<CancellationToken>._))
            .Returns(VerifiedGoogleUser);
        A.CallTo(() => _userRepository.FindByExternalLoginAsync(A<string>._, A<string>._))
            .Returns(deleted);

        var result = await _handler.Handle(Command(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.Error, Does.Contain("borttaget"));
        });
        A.CallTo(() => _refreshTokenService.GenerateRefreshTokenAsync(A<string>._, A<string>._, A<string>._))
            .MustNotHaveHappened();
    }
}
