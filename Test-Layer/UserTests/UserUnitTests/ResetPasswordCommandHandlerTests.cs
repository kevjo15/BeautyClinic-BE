using Application_Layer.Commands.UserCommands.ResetPassword;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Domain_Layer.Common;
using FakeItEasy;

namespace Test_Layer.UserTests.UserUnitTests;

[TestFixture]
public class ResetPasswordCommandHandlerTests
{
    private IUserRepository _userRepository = null!;
    private ResetPasswordCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = A.Fake<IUserRepository>();
        _handler = new ResetPasswordCommandHandler(_userRepository);
    }

    private static ResetPasswordCommand Command(
        string email = "karin@example.com",
        string token = "token-123",
        string password = "NyttLösen123!") =>
        new(new ResetPasswordDTO
        {
            Email = email,
            Token = token,
            NewPassword = password,
            ConfirmNewPassword = password,
        });

    [Test]
    public async Task Handle_WhenTokenIsValid_ReturnsSuccess()
    {
        A.CallTo(() => _userRepository.ResetPasswordWithTokenAsync(
                "karin@example.com", "token-123", "NyttLösen123!"))
            .Returns(OperationResult.Success());

        var result = await _handler.Handle(Command(), CancellationToken.None);

        Assert.That(result.Successful, Is.True);
    }

    [Test]
    public async Task Handle_WhenTokenIsInvalid_ReturnsFailure()
    {
        A.CallTo(() => _userRepository.ResetPasswordWithTokenAsync(
                A<string>._, A<string>._, A<string>._))
            .Returns(OperationResult.Failure("Länken är ogiltig eller har gått ut. Begär en ny återställningslänk."));

        var result = await _handler.Handle(Command(token: "bad-token"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.Error, Does.Contain("ogiltig"));
        });
    }
}
