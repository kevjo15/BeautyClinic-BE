using Application_Layer.Commands.UserCommands.ForgotPassword;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Domain_Layer.Models;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Test_Layer.UserTests.UserUnitTests;

[TestFixture]
public class ForgotPasswordCommandHandlerTests
{
    private IUserRepository _userRepository = null!;
    private IPasswordResetEmailService _resetEmailService = null!;
    private ForgotPasswordCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = A.Fake<IUserRepository>();
        _resetEmailService = A.Fake<IPasswordResetEmailService>();
        _handler = new ForgotPasswordCommandHandler(
            _userRepository,
            _resetEmailService,
            NullLogger<ForgotPasswordCommandHandler>.Instance);
    }

    private static ForgotPasswordCommand Command(string email) =>
        new(new ForgotPasswordDTO { Email = email });

    [Test]
    public async Task Handle_WhenEmailIsUnknown_ReturnsSuccessWithoutSendingEmail()
    {
        A.CallTo(() => _userRepository.GeneratePasswordResetTokenAsync("unknown@example.com"))
            .Returns((string?)null);

        var result = await _handler.Handle(Command("unknown@example.com"), CancellationToken.None);

        Assert.That(result.Successful, Is.True);
        A.CallTo(() => _resetEmailService.SendResetLinkAsync(
                A<string>._, A<string?>._, A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenEmailIsKnown_SendsResetLinkWithToken()
    {
        A.CallTo(() => _userRepository.GeneratePasswordResetTokenAsync("karin@example.com"))
            .Returns("reset-token-123");
        A.CallTo(() => _userRepository.FindByEmailAsync("karin@example.com"))
            .Returns(new UserModel { Id = "user-1", Email = "karin@example.com", FirstName = "Karin" });

        var result = await _handler.Handle(Command("karin@example.com"), CancellationToken.None);

        Assert.That(result.Successful, Is.True);
        A.CallTo(() => _resetEmailService.SendResetLinkAsync(
                "karin@example.com", "Karin", "reset-token-123", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task Handle_WhenEmailSendingFails_StillReturnsSuccess()
    {
        A.CallTo(() => _userRepository.GeneratePasswordResetTokenAsync("karin@example.com"))
            .Returns("reset-token-123");
        A.CallTo(() => _resetEmailService.SendResetLinkAsync(
                A<string>._, A<string?>._, A<string>._, A<CancellationToken>._))
            .Throws(new InvalidOperationException("SMTP down"));

        var result = await _handler.Handle(Command("karin@example.com"), CancellationToken.None);

        Assert.That(result.Successful, Is.True);
    }

    [Test]
    public async Task Handle_TrimsEmailBeforeLookup()
    {
        A.CallTo(() => _userRepository.GeneratePasswordResetTokenAsync("karin@example.com"))
            .Returns((string?)null);

        await _handler.Handle(Command("  karin@example.com  "), CancellationToken.None);

        A.CallTo(() => _userRepository.GeneratePasswordResetTokenAsync("karin@example.com"))
            .MustHaveHappenedOnceExactly();
    }
}
