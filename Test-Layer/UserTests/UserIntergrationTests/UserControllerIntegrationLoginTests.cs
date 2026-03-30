using API_Layer.Controllers;
using Application_Layer.Commands.UserCommands.Login;
using Application_Layer.Commands.UserCommands.RegisterUser;
using Application_Layer.DTOs;
using Domain_Layer.Common;
using Domain_Layer.Models;
using FakeItEasy;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Test_Layer.UserTests.UserIntergrationTests
{
    [TestFixture]
    public class UserControllerIntegrationLoginTests
    {
        private IMediator _mediator;
        private IConfiguration _configuration;
        private UserController _userController;


        [SetUp]
        public void SetUp()
        {
            // Skapa en fake för IMediator och IConfiguration
            _mediator = A.Fake<IMediator>();
            _configuration = A.Fake<IConfiguration>();

            // Skapa en instans av UserController med fake mediator och configuration
            _userController = new UserController(_mediator, _configuration);

            // Registrera en användare som används i inloggnings- och andra tester
            var registerUserDTO = new RegisterUserDTO
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                PhoneNumber = "0701234567"
            };

            var expectedUser = new UserModel
            {
                Email = registerUserDTO.Email,
                UserName = registerUserDTO.Email,
                FirstName = registerUserDTO.FirstName,
                LastName = registerUserDTO.LastName
            };

            var registerResult = OperationResult<UserModel>.Success(expectedUser);

            // Mocka mediators "Send" metod för registrering så att den returnerar en framgångsrik OperationResult
            A.CallTo(() => _mediator.Send(A<RegisterUserCommand>._, A<CancellationToken>._))
                .Returns(registerResult);

            // Registrera användaren
            _userController.Register(registerUserDTO);
        }

        [Test]
        public async Task Login_ReturnsOk_WhenLoginIsSuccessful()
        {
            // Arrange
            var loginUserDTO = new LoginUserDTO
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var expectedToken = "fake_token";

            var loginResult = OperationResult<AuthTokenPairDTO>.Success(
                new AuthTokenPairDTO(expectedToken, null));

            // Mocka mediators "Send" metod så att den returnerar en framgångsrik OperationResult
            A.CallTo(() => _mediator.Send(A<LoginCommand>._, A<CancellationToken>._))
                .Returns(loginResult);

            // Act
            var actionResult = await _userController.Login(loginUserDTO) as OkObjectResult;

            // Assert
            Assert.IsNotNull(actionResult, "ActionResult is null, expected OkObjectResult.");
            Assert.NotNull(actionResult?.Value, "ActionResult.Value is null.");
            Assert.IsInstanceOf<OkObjectResult>(actionResult);

            var resultProperty = actionResult?.Value?.GetType().GetProperty("accessToken");
            Assert.NotNull(resultProperty, "accessToken property is missing.");
            var resultObject = resultProperty!.GetValue(actionResult!.Value, null);

            // Kontrollera att token finns i resultatet
            Assert.NotNull(resultObject, "Token is null.");

            // Hämta token från resultatet och kontrollera att den matchar det förväntade värdet
            Assert.That(resultObject.ToString(), Is.EqualTo(expectedToken));

            Assert.That(actionResult!.StatusCode, Is.EqualTo(200), "Expected status code 200 for successful login.");
            Assert.That(loginResult.Successful, Is.True, "Expected login to be successful.");
            Assert.That(loginResult.Error, Is.Null, "Expected no error message for successful login.");
        }






        [Test]
        public async Task Login_ReturnsBadRequest_WhenLoginFails()
        {
            // Arrange
            var loginUserDTO = new LoginUserDTO
            {
                Email = "testuser@example.com",
                Password = "WrongPassword!"
            };

            var loginResult = OperationResult<AuthTokenPairDTO>.Failure("Invalid credentials");

            // Mocka mediators "Send" metod så att den returnerar ett negativt OperationResult
            A.CallTo(() => _mediator.Send(A<LoginCommand>._, A<CancellationToken>._))
                .Returns(loginResult);

            // Act
            var actionResult = await _userController.Login(loginUserDTO) as BadRequestObjectResult;

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(actionResult);
            Assert.NotNull(actionResult, "Expected BadRequestObjectResult.");

            var errorMessage = actionResult?.Value as string;
            Assert.NotNull(errorMessage);
            Assert.That(errorMessage, Is.EqualTo("Invalid credentials"));
            Assert.That(actionResult!.StatusCode, Is.EqualTo(400), "Expected status code 400 for failed login.");
            Assert.That(loginResult.Successful, Is.False, "Expected login to fail.");
        }

    }
}
