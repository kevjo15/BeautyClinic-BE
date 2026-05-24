using Application_Layer.PipelineBehaviour;
using Domain_Layer.Common;
using FakeItEasy;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Test_Layer.Behaviors;

[TestFixture]
public class LoggingBehaviourTests
{
    private ILogger<LoggingBehaviour<TestRequest, OperationResult<string>>> _logger = null!;
    private LoggingBehaviour<TestRequest, OperationResult<string>> _behaviour = null!;

    [SetUp]
    public void SetUp()
    {
        _logger = A.Fake<ILogger<LoggingBehaviour<TestRequest, OperationResult<string>>>>();
        _behaviour = new LoggingBehaviour<TestRequest, OperationResult<string>>(_logger);
    }

    [Test]
    public async Task Handle_WhenSuccessful_ShouldReturnResponseAndLogInformation()
    {
        var expected = OperationResult<string>.Success("ok");
        RequestHandlerDelegate<OperationResult<string>> next = () => Task.FromResult(expected);

        var result = await _behaviour.Handle(new TestRequest(), next, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        A.CallTo(_logger)
            .Where(call => call.Method.Name == "Log" &&
                           call.GetArgument<LogLevel>(0) == LogLevel.Information)
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task Handle_WhenOperationResultFails_ShouldLogWarning()
    {
        var expected = OperationResult<string>.Failure("something went wrong");
        RequestHandlerDelegate<OperationResult<string>> next = () => Task.FromResult(expected);

        var result = await _behaviour.Handle(new TestRequest(), next, CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
        A.CallTo(_logger)
            .Where(call => call.Method.Name == "Log" &&
                           call.GetArgument<LogLevel>(0) == LogLevel.Warning)
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public void Handle_WhenExceptionThrown_ShouldLogErrorAndRethrow()
    {
        RequestHandlerDelegate<OperationResult<string>> next = () => throw new InvalidOperationException("boom");

        var act = async () => await _behaviour.Handle(new TestRequest(), next, CancellationToken.None);

        Assert.That(act, Throws.TypeOf<InvalidOperationException>());
        A.CallTo(_logger)
            .Where(call => call.Method.Name == "Log" &&
                           call.GetArgument<LogLevel>(0) == LogLevel.Error)
            .MustHaveHappenedOnceExactly();
    }
}

public class TestRequest : IRequest<OperationResult<string>>;
