using Application_Layer.Commands.UserCommands.ResetPassword;
using Application_Layer.DTOs;

namespace Test_Layer.UserTests.UserUnitTests;

[TestFixture]
public class ResetPasswordCommandValidatorTests
{
    private ResetPasswordCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new ResetPasswordCommandValidator();
    }

    private static ResetPasswordCommand Command(
        string email = "karin@example.com",
        string token = "token-123",
        string newPassword = "NyttLösen123!",
        string? confirm = null) =>
        new(new ResetPasswordDTO
        {
            Email = email,
            Token = token,
            NewPassword = newPassword,
            ConfirmNewPassword = confirm ?? newPassword,
        });

    [Test]
    public void Validate_WithValidInput_Passes()
    {
        var result = _validator.Validate(Command());
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_WithInvalidEmail_Fails()
    {
        var result = _validator.Validate(Command(email: "inte-en-mejl"));
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validate_WithEmptyToken_Fails()
    {
        var result = _validator.Validate(Command(token: ""));
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validate_WithWeakPassword_Fails()
    {
        var result = _validator.Validate(Command(newPassword: "kort", confirm: "kort"));
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validate_WhenPasswordsDoNotMatch_Fails()
    {
        var result = _validator.Validate(Command(confirm: "AnnatLösen123!"));
        Assert.That(result.IsValid, Is.False);
    }
}
