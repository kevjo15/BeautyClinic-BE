using API_Layer.Controllers;
using Application_Layer.Commands.UserCommands.Login;
using Application_Layer.Commands.UserCommands.RegisterUser;
using Application_Layer.DTO_s;
using Application_Layer.Jwt;
using Domain_Layer.Models;
using FakeItEasy;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Layer.UserTests.UserIntergrationTests
{
    [TestFixture]
    public class UserControllerIntegrationLoginTests
    {
        private IMediator _mediator;
        private UserController _userController;
        private IJwtTokenGenerator _jwtTokenGenerator;


        [SetUp]
        public void SetUp()
        {
            // Skapa en fake för IMediator och IJwtTokenGenerator
            _mediator = A.Fake<IMediator>();
            _jwtTokenGenerator = A.Fake<IJwtTokenGenerator>();

            // Mocka token generation
            A.CallTo(() => _jwtTokenGenerator.GenerateToken(A<UserModel>._))
                .Returns("fake_token");

            // Skapa en instans av UserController med fake mediator
            _userController = new UserController(_mediator);

            // Registrera en användare som används i inloggnings- och andra tester
            var registerUserDTO = new RegisterUserDTO
            {
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var expectedUser = new UserModel
            {
                Email = registerUserDTO.Email,
                UserName = registerUserDTO.UserName,
                FirstName = registerUserDTO.FirstName,
                LastName = registerUserDTO.LastName
            };

            var registerResult = new RegisterResult(true, expectedUser);

            // Mocka mediators "Send" metod för registrering så att den returnerar en framgångsrik RegisterResult
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

            var loginResult = new LoginResult
            {
                Successful = true,
                Token = expectedToken
            };

            // Mocka mediators "Send" metod så att den returnerar en framgångsrik LoginResult
            A.CallTo(() => _mediator.Send(A<LoginCommand>._, A<CancellationToken>._))
                .Returns(loginResult);

            // Act
            var actionResult = await _userController.Login(loginUserDTO) as OkObjectResult;

            // Assert
            Assert.IsNotNull(actionResult, "ActionResult is null, expected OkObjectResult.");
            Assert.NotNull(actionResult?.Value, "ActionResult.Value is null.");
            Assert.IsInstanceOf<OkObjectResult>(actionResult);

            // Istället för att hantera resultatet som en IDictionary, konvertera det direkt till en anonym typ
            var resultObject = actionResult?.Value.GetType().GetProperty("token")?.GetValue(actionResult.Value, null);

            // Kontrollera att token finns i resultatet
            Assert.NotNull(resultObject, "Token is null.");

            // Hämta token från resultatet och kontrollera att den matchar det förväntade värdet
            Assert.That(resultObject.ToString(), Is.EqualTo(expectedToken));

            Assert.That(actionResult.StatusCode, Is.EqualTo(200), "Expected status code 200 for successful login.");
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

            var loginResult = new LoginResult
            {
                Successful = false,
                Error = "Invalid credentials"
            };

            // Mocka mediators "Send" metod så att den returnerar ett negativt LoginResult
            A.CallTo(() => _mediator.Send(A<LoginCommand>._, A<CancellationToken>._))
                .Returns(loginResult);

            // Act
            var actionResult = await _userController.Login(loginUserDTO) as BadRequestObjectResult;

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(actionResult);

            var errorMessage = actionResult?.Value as string;
            Assert.NotNull(errorMessage);
            Assert.That(errorMessage, Is.EqualTo("Invalid credentials"));
            Assert.That(actionResult.StatusCode, Is.EqualTo(400), "Expected status code 400 for failed login.");
            Assert.That(loginResult.Successful, Is.False, "Expected login to fail.");
        }

    }
}
