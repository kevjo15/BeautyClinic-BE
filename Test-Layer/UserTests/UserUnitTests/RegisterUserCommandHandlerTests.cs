using Application_Layer.Commands.UserCommands.RegisterUser;
using Application_Layer.DTO_s;
using Application_Layer.Interfaces;
using AutoMapper;
using Domain_Layer.Models;
using FakeItEasy;
using Microsoft.AspNetCore.Identity;

namespace Test_Layer.UserTests.UserUnitTests
{
    [TestFixture]
    public class RegisterUserCommandHandlerTests
    {
        private RegisterUserCommandHandler _handler;
        private IUserRepository _userRepository;
        private IMapper _mapper;

        [SetUp]
        public void Setup()
        {
            _userRepository = A.Fake<IUserRepository>();
            _mapper = A.Fake<IMapper>();

            _handler = new RegisterUserCommandHandler(_userRepository, _mapper);
        }

        [Test]
        public async Task Handle_WhenCalled_ShouldRegisterUser()
        {
            // Arrange
            var registerUserDTO = new RegisterUserDTO
            {
                Email = "test@example.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User"
            };

            var command = new RegisterUserCommand(registerUserDTO);

            // Mappa till UserModel med FakeItEasy
            var userModel = new UserModel
            {
                Email = registerUserDTO.Email,
                FirstName = registerUserDTO.FirstName,
                LastName = registerUserDTO.LastName
            };
            A.CallTo(() => _mapper.Map<UserModel>(registerUserDTO)).Returns(userModel);

            // Returnera ett lyckat IdentityResult
            var identityResult = IdentityResult.Success;
            A.CallTo(() => _userRepository.RegisterUserAsync(userModel, registerUserDTO.Password))
                .Returns(Task.FromResult(identityResult));

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.That(result.CreatedUser.Email, Is.EqualTo(userModel.Email));
        }

        [Test]
        public async Task Handle_WhenUserAlreadyExists_ShouldReturnError()
        {
            // Arrange
            var registerUserDTO = new RegisterUserDTO
            {
                Email = "existing@example.com",
                Password = "Password123!",
                FirstName = "Existing",
                LastName = "User"
            };

            var command = new RegisterUserCommand(registerUserDTO);

            var userModel = new UserModel
            {
                Email = registerUserDTO.Email
            };

            A.CallTo(() => _mapper.Map<UserModel>(registerUserDTO)).Returns(userModel);

            // Simulera att användarregistreringen misslyckas och returnera IdentityResult med fel
            var identityResult = IdentityResult.Failed(new IdentityError { Description = "User already exists" });
            A.CallTo(() => _userRepository.RegisterUserAsync(userModel, registerUserDTO.Password))
                .Returns(Task.FromResult(identityResult));

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.That(result.Errors.Count, Is.EqualTo(1));  // Kontrollera att exakt ett fel returneras
            Assert.That(result.Errors[0], Is.EqualTo("User already exists"));  // Kontrollera att felmeddelandet är korrekt

            // Kontrollera att rätt metoder anropades exakt en gång
            A.CallTo(() => _userRepository.RegisterUserAsync(userModel, registerUserDTO.Password))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _mapper.Map<UserModel>(registerUserDTO))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task Handle_WhenUnexpectedExceptionOccurs_ShouldReturnError()
        {
            // Arrange
            var registerUserDTO = new RegisterUserDTO
            {
                Email = "test@example.com",
                Password = "Password123!",
                FirstName = "Test",
                LastName = "User"
            };

            var command = new RegisterUserCommand(registerUserDTO);

            var userModel = new UserModel
            {
                Email = registerUserDTO.Email,
                FirstName = registerUserDTO.FirstName,
                LastName = registerUserDTO.LastName
            };

            A.CallTo(() => _mapper.Map<UserModel>(registerUserDTO)).Returns(userModel);

            // Simulera ett oväntat undantag
            A.CallTo(() => _userRepository.RegisterUserAsync(userModel, registerUserDTO.Password))
                .Throws(new Exception("Unexpected error"));

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Errors.Contains("An unexpected error occurred: Unexpected error"));
        }

    }
}
