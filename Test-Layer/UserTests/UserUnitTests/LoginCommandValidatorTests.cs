using Application_Layer.Commands.UserCommands.Login;
using Application_Layer.DTOs;

namespace Test_Layer.UserTests.UserUnitTests;

[TestFixture]
public class LoginCommandValidatorTests
{
    private LoginCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new LoginCommandValidator();
    }

    [Test]
    public void Validate_WithValidData_ShouldPass()
    {
        var command = new LoginCommand(new LoginUserDTO
        {
            Email = "test@example.com",
            Password = "Password123!"
        });

        var result = _validator.Validate(command);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_WithInvalidEmail_ShouldFail()
    {
        var command = new LoginCommand(new LoginUserDTO
        {
            Email = "invalid-email",
            Password = "Password123!"
        });

        var result = _validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Select(e => e.ErrorMessage), Has.Some.EqualTo("A valid email address is required."));
    }

    [Test]
    public void Validate_WithMissingPassword_ShouldFail()
    {
        var command = new LoginCommand(new LoginUserDTO
        {
            Email = "test@example.com",
            Password = string.Empty
        });

        var result = _validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Select(e => e.ErrorMessage), Has.Some.EqualTo("Password is required."));
    }
}
