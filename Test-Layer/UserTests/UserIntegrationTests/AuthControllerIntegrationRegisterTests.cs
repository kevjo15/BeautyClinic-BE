using API_Layer.Controllers;
using Application_Layer.Commands.UserCommands.RegisterUser;
using Application_Layer.DTOs;
using Domain_Layer.Common;
using Domain_Layer.Models;
using FakeItEasy;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Test_Layer.UserTests.UserIntegrationTests
{
    [TestFixture]
    public class AuthControllerIntegrationRegisterTests
    {
        private IMediator _mediator;
        private IConfiguration _configuration;
        private AuthController _authController;

        [SetUp]
        public void SetUp()
        {
            // Skapa en fake för IMediator och IConfiguration
            _mediator = A.Fake<IMediator>();
            _configuration = A.Fake<IConfiguration>();
            _authController = new AuthController(_mediator, _configuration);
        }

        [Test]
        public async Task Register_ReturnsOk_WhenUserIsRegisteredSuccessfully()
        {
            // Arrange
            var registerUserDTO = new RegisterUserDTO
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = "0701234567",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var expectedUser = new UserModel
            {
                Email = registerUserDTO.Email,
                UserName = registerUserDTO.Email,
                FirstName = registerUserDTO.FirstName,
                LastName = registerUserDTO.LastName
            };

            var registerResult = OperationResult<UserModel>.Success(expectedUser);

            // Mocka mediators "Send" metod så att den returnerar en framgångsrik OperationResult
            A.CallTo(() => _mediator.Send(A<RegisterUserCommand>._, A<CancellationToken>._))
                .Returns(registerResult);

            // Act
            var actionResult = await _authController.Register(registerUserDTO) as OkObjectResult;

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(actionResult);

            var resultUser = actionResult?.Value as UserModel;
            Assert.NotNull(resultUser);
            Assert.That(resultUser.Email, Is.EqualTo(expectedUser.Email));
            Assert.That(resultUser.FirstName, Is.EqualTo(expectedUser.FirstName));
            Assert.That(resultUser.LastName, Is.EqualTo(expectedUser.LastName));
        }


        [Test]
        public async Task Register_ReturnsBadRequest_WhenRegistrationFails()
        {
            // Arrange
            var registerUserDTO = new RegisterUserDTO
            {
                Email = "invalid-email",
                FirstName = "Test",
                LastName = "User",
                PhoneNumber = "0701234567",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var registerResult = OperationResult<UserModel>.Failure("Invalid email format");

            // Simulera att registreringen misslyckas genom att returnera ett negativt resultat från mediatorn
            A.CallTo(() => _mediator.Send(A<RegisterUserCommand>._, A<CancellationToken>._))
                .Returns(registerResult);

            // Act
            var actionResult = await _authController.Register(registerUserDTO);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(actionResult);

            var badRequestResult = (actionResult as BadRequestObjectResult)?.Value as string;
            Assert.NotNull(badRequestResult);
            Assert.That(badRequestResult, Is.EqualTo("Invalid email format"));
        }

    }
}
