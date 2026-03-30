using Application_Layer.Commands.UserCommands.RegisterUser;
using Application_Layer.DTOs;

namespace Test_Layer.UserTests.UserUnitTests;

[TestFixture]
public class RegisterUserCommandValidatorTests
{
    private RegisterUserCommandValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new RegisterUserCommandValidator();
    }

    [Test]
    public void Validate_WithValidData_ShouldPass()
    {
        var command = new RegisterUserCommand(new RegisterUserDTO
        {
            FirstName = "Alice",
            LastName = "Andersson",
            Email = "alice@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            PhoneNumber = "0701234567"
        });

        var result = _validator.Validate(command);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_WithMismatchedPasswords_ShouldFail()
    {
        var command = new RegisterUserCommand(new RegisterUserDTO
        {
            FirstName = "Alice",
            LastName = "Andersson",
            Email = "alice@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password456!",
            PhoneNumber = "0701234567"
        });

        var result = _validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Select(e => e.ErrorMessage), Has.Some.EqualTo("Passwords do not match."));
    }

    [Test]
    public void Validate_WithMissingPhoneNumber_ShouldFail()
    {
        var command = new RegisterUserCommand(new RegisterUserDTO
        {
            FirstName = "Alice",
            LastName = "Andersson",
            Email = "alice@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            PhoneNumber = string.Empty
        });

        var result = _validator.Validate(command);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Select(e => e.ErrorMessage), Has.Some.EqualTo("Phone number is required."));
    }
}
