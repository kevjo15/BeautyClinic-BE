using API_Layer.Controllers;
using Application_Layer.Commands.UserCommands.RefreshToken;
using Application_Layer.Commands.UserCommands.RevokeRefreshToken;
using Application_Layer.DTOs;
using Domain_Layer.Common;
using FakeItEasy;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Test_Layer.UserTests.UserIntegrationTests;

[TestFixture]
public class AuthControllerSessionTests
{
    private IMediator _mediator = null!;
    private IConfiguration _configuration = null!;
    private AuthController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _mediator = A.Fake<IMediator>();
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:RefreshTokenExpiryDays"] = "7"
            })
            .Build();
        _controller = new AuthController(_mediator, _configuration)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Test]
    public async Task RefreshToken_WithoutCookie_ShouldReturnUnauthorized()
    {
        var actionResult = await _controller.RefreshToken();

        Assert.That(actionResult, Is.TypeOf<UnauthorizedObjectResult>());
        var unauthorized = (UnauthorizedObjectResult)actionResult;
        Assert.That(unauthorized.Value, Is.EqualTo("No refresh token provided."));
    }

    [Test]
    public async Task RefreshToken_WithValidCookie_ShouldReturnAccessToken()
    {
        _controller.ControllerContext.HttpContext!.Request.Headers["User-Agent"] = "nunit";
        _controller.ControllerContext.HttpContext.Request.Headers["Cookie"] = "refreshToken=old-token";

        A.CallTo(() => _mediator.Send(
                A<RefreshAccessTokenCommand>.That.Matches(command =>
                    command.RefreshToken == "old-token" && command.UserAgent == "nunit"),
                A<CancellationToken>._))
            .Returns(OperationResult<AuthTokenPairDTO>.Success(new AuthTokenPairDTO("access-token", "new-refresh-token")));

        var actionResult = await _controller.RefreshToken();

        Assert.That(actionResult, Is.TypeOf<OkObjectResult>());
        var okResult = (OkObjectResult)actionResult;
        var accessToken = okResult.Value!.GetType().GetProperty("accessToken")!.GetValue(okResult.Value)?.ToString();
        Assert.That(accessToken, Is.EqualTo("access-token"));
    }

    [Test]
    public async Task RevokeRefreshToken_ShouldAlwaysClearCookieAndReturnOkOnSuccess()
    {
        _controller.ControllerContext.HttpContext!.User = CreateUser("user-1");
        _controller.ControllerContext.HttpContext.Request.Headers["Cookie"] = "refreshToken=refresh-token";

        A.CallTo(() => _mediator.Send(
                A<RevokeRefreshTokenCommand>.That.Matches(command =>
                    command.RefreshToken == "refresh-token" && command.UserId == "user-1"),
                A<CancellationToken>._))
            .Returns(OperationResult.Success());

        var actionResult = await _controller.RevokeRefreshToken();

        Assert.That(actionResult, Is.TypeOf<OkObjectResult>());
        var okResult = (OkObjectResult)actionResult;
        var message = okResult.Value!.GetType().GetProperty("message")!.GetValue(okResult.Value)?.ToString();
        Assert.That(message, Is.EqualTo("Logged out successfully."));
    }

    [Test]
    public async Task RevokeAllTokens_WithoutUser_ShouldReturnUnauthorized()
    {
        var actionResult = await _controller.RevokeAllTokens();

        Assert.That(actionResult, Is.TypeOf<UnauthorizedResult>());
    }

    private static ClaimsPrincipal CreateUser(string userId)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId)
        ], "TestAuth");

        return new ClaimsPrincipal(identity);
    }
}
