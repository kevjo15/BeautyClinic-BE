using Application_Layer.Commands.UserCommands.Login;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Jwt;
using Domain_Layer.Models;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;

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

            _handler = new LoginCommandHandler(
                _userRepository, _jwtTokenGenerator, _refreshTokenService,
                NullLogger<LoginCommandHandler>.Instance);
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
                Email = loginUserDTO.Email,
                EmailConfirmed = true
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
            Assert.That(result.Error, Is.EqualTo("Felaktigt email eller lösenord."));
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
            Assert.That(result.Error, Is.EqualTo("Felaktigt email eller lösenord."));
        }

        [Test]
        public async Task Handle_WhenEmailIsNotConfirmed_ShouldReturnErrorAfterValidPassword()
        {
            // Arrange
            var loginUserDTO = new LoginUserDTO
            {
                Email = "unconfirmed@example.com",
                Password = "Password123!"
            };

            var command = new LoginCommand(loginUserDTO);

            var userModel = new UserModel
            {
                Id = "user-456",
                Email = loginUserDTO.Email,
                EmailConfirmed = false
            };

            A.CallTo(() => _userRepository.FindByEmailAsync(loginUserDTO.Email)).Returns(userModel);
            A.CallTo(() => _userRepository.CheckPasswordAsync(userModel, loginUserDTO.Password)).Returns(true);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.IsFalse(result.Successful);
            Assert.That(result.Error, Does.Contain("inte bekräftad"));
            A.CallTo(() => _refreshTokenService.GenerateRefreshTokenAsync(A<string>._, A<string>._, A<string>._))
                .MustNotHaveHappened();
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

        [Test]
        public async Task Handle_WhenUserIsDeleted_IsRejectedDespiteCorrectPassword()
        {
            var loginUserDTO = new LoginUserDTO { Email = "gone@example.com", Password = "Password123!" };
            var deletedUser = new UserModel
            {
                Id = "user-deleted",
                Email = loginUserDTO.Email,
                EmailConfirmed = true,
                IsDeleted = true
            };

            A.CallTo(() => _userRepository.FindByEmailAsync(loginUserDTO.Email)).Returns(deletedUser);
            A.CallTo(() => _userRepository.CheckPasswordAsync(deletedUser, loginUserDTO.Password)).Returns(true);

            var result = await _handler.Handle(new LoginCommand(loginUserDTO), default);

            Assert.Multiple(() =>
            {
                Assert.That(result.Successful, Is.False);
                Assert.That(result.Error, Does.Contain("borttaget"));
            });
            // Ingen token får utfärdas för ett raderat konto.
            A.CallTo(() => _refreshTokenService.GenerateRefreshTokenAsync(A<string>._, A<string>._, A<string>._))
                .MustNotHaveHappened();
        }
    }
}
