using Application_Layer.Commands.UserCommands.Login;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Jwt;
using Domain_Layer.Models;
using FakeItEasy;

namespace Test_Layer.UserTests.UserUnitTests
{
    [TestFixture]
    public class LoginCommandHandlerTests
    {
        private LoginCommandHandler _handler;
        private IUserRepository _userRepository;
        private IJwtTokenGenerator _jwtTokenGenerator;
        private IRefreshTokenService _refreshTokenService;

        [SetUp]
        public void SetUp()
        {
            _userRepository = A.Fake<IUserRepository>();
            _jwtTokenGenerator = A.Fake<IJwtTokenGenerator>();
            _refreshTokenService = A.Fake<IRefreshTokenService>();

            _handler = new LoginCommandHandler(_userRepository, _jwtTokenGenerator, _refreshTokenService);
        }

        [Test]
        public async Task Handle_WhenCalled_ShouldReturnTokensAndCreateRefreshToken()
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
                Id = "user-123",
                Email = loginUserDTO.Email
            };

            var refreshTokenEntity = new UserRefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userModel.Id,
                TokenHash = "hashed_token",
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            A.CallTo(() => _userRepository.FindByEmailAsync(loginUserDTO.Email)).Returns(userModel);
            A.CallTo(() => _userRepository.CheckPasswordAsync(userModel, loginUserDTO.Password)).Returns(true);
            A.CallTo(() => _userRepository.GetRolesAsync(userModel)).Returns(new List<string> { "Customer" });
            A.CallTo(() => _jwtTokenGenerator.GenerateToken(userModel.Id, userModel.Email, A<IEnumerable<string>>._))
                .Returns("valid_access_token");
            A.CallTo(() => _refreshTokenService.GenerateRefreshTokenAsync(userModel.Id, A<string>._, A<string>._))
                .Returns(("raw_refresh_token", refreshTokenEntity));

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.IsTrue(result.Successful);
            Assert.That(result.Data?.AccessToken, Is.EqualTo("valid_access_token"));
            Assert.That(result.Data?.RefreshToken, Is.EqualTo("raw_refresh_token"));

            // Verify refresh token was created
            A.CallTo(() => _refreshTokenService.GenerateRefreshTokenAsync(userModel.Id, A<string>._, A<string>._))
                .MustHaveHappenedOnceExactly();
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

            A.CallTo(() => _userRepository.FindByEmailAsync(loginUserDTO.Email)).Returns((UserModel?)null);

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
