using Application_Layer.Commands.UserCommands.Login;
using Application_Layer.DTO_s;
using Application_Layer.Jwt;
using AutoMapper;
using Domain_Layer.Models;
using FakeItEasy;
using Infrastructure_Layer.Repositories.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Layer.UserTests
{
    [TestFixture]
    public class LoginCommandHandlerTests
    {
        private LoginCommandHandler _handler;
        private IUserRepository _userRepository;
        private IJwtTokenGenerator _jwtTokenGenerator;
        private IMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            _userRepository = A.Fake<IUserRepository>();
            _jwtTokenGenerator = A.Fake<IJwtTokenGenerator>();
            _mapper = A.Fake<IMapper>();

            _handler = new LoginCommandHandler(_userRepository, _jwtTokenGenerator, _mapper);
        }

        [Test]
        public async Task Handle_WhenCalled_ShouldReturnToken()
        {
            // Arrange
            var loginUserDTO = new LoginUserDTO
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var command = new LoginCommand(loginUserDTO);

            var userModel = new UserModel
            {
                Email = loginUserDTO.Email
            };

            A.CallTo(() => _mapper.Map<UserModel>(loginUserDTO)).Returns(userModel);
            A.CallTo(() => _userRepository.FindByEmailAsync(loginUserDTO.Email)).Returns(userModel);
            A.CallTo(() => _userRepository.CheckPasswordAsync(userModel, loginUserDTO.Password)).Returns(true);
            A.CallTo(() => _jwtTokenGenerator.GenerateToken(userModel)).Returns("valid_token");

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.IsTrue(result.Successful);
            Assert.That(result.Token, Is.EqualTo("valid_token"));
        }

        [Test]
        public async Task Handle_WhenUserDoesNotExist_ShouldReturnError()
        {
            // Arrange
            var loginUserDTO = new LoginUserDTO
            {
                Email = "nonexistent@example.com",
                Password = "Password123!"
            };

            var command = new LoginCommand(loginUserDTO);

            A.CallTo(() => _userRepository.FindByEmailAsync(loginUserDTO.Email)).Returns((UserModel)null);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.IsFalse(result.Successful);
            Assert.That(result.Error, Is.EqualTo("Användaren existerar inte."));
        }

        [Test]
        public async Task Handle_WhenPasswordIsIncorrect_ShouldReturnError()
        {
            // Arrange
            var loginUserDTO = new LoginUserDTO
            {
                Email = "test@example.com",
                Password = "WrongPassword123!"
            };

            var command = new LoginCommand(loginUserDTO);

            var userModel = new UserModel
            {
                Email = loginUserDTO.Email
            };

            A.CallTo(() => _userRepository.FindByEmailAsync(loginUserDTO.Email)).Returns(userModel);
            A.CallTo(() => _userRepository.CheckPasswordAsync(userModel, loginUserDTO.Password)).Returns(false);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.IsFalse(result.Successful);
            Assert.That(result.Error, Is.EqualTo("Felaktigt lösenord."));
        }

        [Test]
        public async Task Handle_WhenUnexpectedExceptionOccurs_ShouldReturnError()
        {
            // Arrange
            var loginUserDTO = new LoginUserDTO
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            var command = new LoginCommand(loginUserDTO);

            A.CallTo(() => _userRepository.FindByEmailAsync(loginUserDTO.Email)).Throws(new Exception("Unexpected error"));

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.IsFalse(result.Successful);
            Assert.That(result.Error, Does.Contain("An unexpected error occurred"));
        }
    }
}
