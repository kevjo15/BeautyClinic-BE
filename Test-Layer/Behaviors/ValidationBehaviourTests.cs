using Application_Layer.Commands.UserCommands.Login;
using Application_Layer.DTOs;
using Application_Layer.PipelineBehaviour;
using Domain_Layer.Common;
using FluentValidation;
using MediatR;

namespace Test_Layer.Behaviors;

[TestFixture]
public class ValidationBehaviourTests
{
    [Test]
    public async Task Handle_WithValidRequest_ShouldProceedToNextHandler()
    {
        var behavior = new ValidationBehaviour<LoginCommand, OperationResult<AuthTokenPairDTO>>(
            [new LoginCommandValidator()]);

        var request = new LoginCommand(new LoginUserDTO
        {
            Email = "test@example.com",
            Password = "Password123!"
        });

        var expected = OperationResult<AuthTokenPairDTO>.Success(new AuthTokenPairDTO("access-token", "refresh-token"));
        RequestHandlerDelegate<OperationResult<AuthTokenPairDTO>> next = () => Task.FromResult(expected);

        var result = await behavior.Handle(request, next, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
    }

    [Test]
    public void Handle_WithInvalidRequest_ShouldThrowValidationException()
    {
        var behavior = new ValidationBehaviour<LoginCommand, OperationResult<AuthTokenPairDTO>>(
            [new LoginCommandValidator()]);

        var request = new LoginCommand(new LoginUserDTO
        {
            Email = "invalid-email",
            Password = string.Empty
        });

        RequestHandlerDelegate<OperationResult<AuthTokenPairDTO>> next =
            () => Task.FromResult(OperationResult<AuthTokenPairDTO>.Success(new AuthTokenPairDTO("a", "b")));

        var act = async () => await behavior.Handle(request, next, CancellationToken.None);

        Assert.That(act, Throws.TypeOf<ValidationException>());
    }
}
