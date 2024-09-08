using API_Layer.Controllers;
using Application_Layer.Commands.UserCommands;
using Application_Layer.DTO_s;
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
    public class UserControllerIntegrationRegisterTests
    {
        private IMediator _mediator;
        private UserController _userController;

        [SetUp]
        public void SetUp()
        {
            // Skapa en fake för IMediator
            _mediator = A.Fake<IMediator>();
            _userController = new UserController(_mediator);
        }

        [TearDown]
        public void TearDown()
        {
            _userController?.Dispose();
        }

        [Test]
        public async Task Register_ReturnsOk_WhenUserIsRegisteredSuccessfully()
        {
            // Arrange
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

            // Mocka mediators "Send" metod så att den returnerar en framgångsrik RegisterResult
            A.CallTo(() => _mediator.Send(A<RegisterUserCommand>._, A<CancellationToken>._))
                .Returns(registerResult);

            // Act
            var actionResult = await _userController.Register(registerUserDTO) as OkObjectResult;

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
                UserName = "testuser",
                Email = "invalid-email",
                FirstName = "Test",
                LastName = "User",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            var registerResult = new RegisterResult(false, null, new List<string> { "Invalid email format" });

            // Simulera att registreringen misslyckas genom att returnera ett negativt resultat från mediatorn
            A.CallTo(() => _mediator.Send(A<RegisterUserCommand>._, A<CancellationToken>._))
                .Returns(registerResult);

            // Act
            var actionResult = await _userController.Register(registerUserDTO);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(actionResult);

            var badRequestResult = (actionResult as BadRequestObjectResult)?.Value as List<string>;
            Assert.NotNull(badRequestResult);
            Assert.That(badRequestResult.Contains("Invalid email format"));
        }

    }
}