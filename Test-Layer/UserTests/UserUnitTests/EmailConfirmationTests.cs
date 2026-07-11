using Application_Layer.Commands.UserCommands.ConfirmEmail;
using Application_Layer.Commands.UserCommands.ResendEmailConfirmation;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Domain_Layer.Common;
using Domain_Layer.Models;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Test_Layer.UserTests.UserUnitTests;

[TestFixture]
public class EmailConfirmationTests
{
    private IUserRepository _userRepository = null!;
    private IEmailConfirmationEmailService _confirmationEmailService = null!;
    private ConfirmEmailCommandHandler _confirmHandler = null!;
    private ResendEmailConfirmationCommandHandler _resendHandler = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = A.Fake<IUserRepository>();
        _confirmationEmailService = A.Fake<IEmailConfirmationEmailService>();
        _confirmHandler = new ConfirmEmailCommandHandler(_userRepository);
        _resendHandler = new ResendEmailConfirmationCommandHandler(
            _userRepository,
            _confirmationEmailService,
            NullLogger<ResendEmailConfirmationCommandHandler>.Instance);
    }

    [Test]
    public async Task Confirm_WhenTokenIsValid_ReturnsSuccess()
    {
        A.CallTo(() => _userRepository.ConfirmEmailAsync("user-1", "token-123"))
            .Returns(OperationResult.Success());

        var result = await _confirmHandler.Handle(
            new ConfirmEmailCommand(new ConfirmEmailDTO { UserId = "user-1", Token = "token-123" }),
            CancellationToken.None);

        Assert.That(result.Successful, Is.True);
    }

    [Test]
    public async Task Confirm_WhenTokenIsInvalid_ReturnsFailure()
    {
        A.CallTo(() => _userRepository.ConfirmEmailAsync(A<string>._, A<string>._))
            .Returns(OperationResult.Failure("Länken är ogiltig eller har gått ut. Begär ett nytt bekräftelsemejl."));

        var result = await _confirmHandler.Handle(
            new ConfirmEmailCommand(new ConfirmEmailDTO { UserId = "user-1", Token = "bad" }),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.Error, Does.Contain("ogiltig"));
        });
    }

    [Test]
    public async Task Resend_WhenAddressIsUnknownOrConfirmed_ReturnsSuccessWithoutEmail()
    {
        A.CallTo(() => _userRepository.GenerateEmailConfirmationTokenAsync("done@example.com"))
            .Returns(((string, string)?)null);

        var result = await _resendHandler.Handle(
            new ResendEmailConfirmationCommand(new ResendConfirmationDTO { Email = "done@example.com" }),
            CancellationToken.None);

        Assert.That(result.Successful, Is.True);
        A.CallTo(() => _confirmationEmailService.SendConfirmationLinkAsync(
                A<string>._, A<string?>._, A<string>._, A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task Resend_WhenAddressIsUnconfirmed_SendsNewLink()
    {
        A.CallTo(() => _userRepository.GenerateEmailConfirmationTokenAsync("nils@example.com"))
            .Returns(("user-9", "fresh-token"));
        A.CallTo(() => _userRepository.FindByEmailAsync("nils@example.com"))
            .Returns(new UserModel { Id = "user-9", FirstName = "Nils" });

        var result = await _resendHandler.Handle(
            new ResendEmailConfirmationCommand(new ResendConfirmationDTO { Email = "nils@example.com" }),
            CancellationToken.None);

        Assert.That(result.Successful, Is.True);
        A.CallTo(() => _confirmationEmailService.SendConfirmationLinkAsync(
                "nils@example.com", "Nils", "user-9", "fresh-token", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task Resend_WhenEmailSendingFails_StillReturnsSuccess()
    {
        A.CallTo(() => _userRepository.GenerateEmailConfirmationTokenAsync(A<string>._))
            .Returns(("user-9", "fresh-token"));
        A.CallTo(() => _confirmationEmailService.SendConfirmationLinkAsync(
                A<string>._, A<string?>._, A<string>._, A<string>._, A<CancellationToken>._))
            .Throws(new InvalidOperationException("ACS down"));

        var result = await _resendHandler.Handle(
            new ResendEmailConfirmationCommand(new ResendConfirmationDTO { Email = "nils@example.com" }),
            CancellationToken.None);

        Assert.That(result.Successful, Is.True);
    }
}
