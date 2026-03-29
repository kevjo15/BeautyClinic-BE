using Application_Layer.Commands.UserCommands.RefreshToken;
using Application_Layer.Interfaces;
using Application_Layer.Jwt;
using Domain_Layer.Models;
using FakeItEasy;

namespace Test_Layer.UserTests.RefreshTokenTests
{
    [TestFixture]
    public class RefreshAccessTokenCommandHandlerTests
    {
        private RefreshAccessTokenCommandHandler _handler;
        private IUserRepository _userRepository;
        private IJwtTokenGenerator _jwtTokenGenerator;
        private IRefreshTokenService _refreshTokenService;

        [SetUp]
        public void SetUp()
        {
            _userRepository = A.Fake<IUserRepository>();
            _jwtTokenGenerator = A.Fake<IJwtTokenGenerator>();
            _refreshTokenService = A.Fake<IRefreshTokenService>();

            _handler = new RefreshAccessTokenCommandHandler(
                _userRepository,
                _jwtTokenGenerator,
                _refreshTokenService);
        }

        [Test]
        public async Task Handle_WithValidRefreshToken_ShouldRotateTokenAndReturnNewTokens()
        {
            // Arrange
            var oldRefreshToken = "old_refresh_token";
            var newRefreshToken = "new_refresh_token";
            var userId = "user-123";

            var command = new RefreshAccessTokenCommand(oldRefreshToken, "127.0.0.1", "Test-Agent");

            var user = new UserModel
            {
                Id = userId,
                Email = "test@example.com"
            };

            var newTokenEntity = new UserRefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = "new_hash",
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            A.CallTo(() => _refreshTokenService.RotateRefreshTokenAsync(oldRefreshToken, "127.0.0.1", "Test-Agent"))
                .Returns((newRefreshToken, newTokenEntity));
            A.CallTo(() => _userRepository.FindByIdAsync(userId)).Returns(user);
            A.CallTo(() => _userRepository.GetRolesAsync(user)).Returns(new List<string> { "Customer" });
            A.CallTo(() => _jwtTokenGenerator.GenerateToken(user.Id, user.Email, A<IEnumerable<string>>._))
                .Returns("new_access_token");

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.IsTrue(result.Successful);
            Assert.That(result.AccessToken, Is.EqualTo("new_access_token"));
            Assert.That(result.RefreshToken, Is.EqualTo("new_refresh_token"));
        }

        [Test]
        public async Task Handle_WithInvalidRefreshToken_ShouldReturnError()
        {
            // Arrange
            var invalidToken = "invalid_token";
            var command = new RefreshAccessTokenCommand(invalidToken);

            A.CallTo(() => _refreshTokenService.RotateRefreshTokenAsync(invalidToken, A<string>._, A<string>._))
                .Returns(((string, UserRefreshToken)?)null);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.IsFalse(result.Successful);
            Assert.That(result.Error, Does.Contain("Invalid or expired refresh token"));
        }

        [Test]
        public async Task Handle_WithEmptyRefreshToken_ShouldReturnError()
        {
            // Arrange
            var command = new RefreshAccessTokenCommand("");

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.IsFalse(result.Successful);
            Assert.That(result.Error, Does.Contain("Refresh token is required"));
        }

        [Test]
        public async Task Handle_WhenUserNotFound_ShouldReturnError()
        {
            // Arrange
            var refreshToken = "valid_token";
            var userId = "nonexistent-user";
            var command = new RefreshAccessTokenCommand(refreshToken);

            var newTokenEntity = new UserRefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = "hash",
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            A.CallTo(() => _refreshTokenService.RotateRefreshTokenAsync(refreshToken, A<string>._, A<string>._))
                .Returns(("new_token", newTokenEntity));
            A.CallTo(() => _userRepository.FindByIdAsync(userId)).Returns((UserModel?)null);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.IsFalse(result.Successful);
            Assert.That(result.Error, Does.Contain("User not found"));
        }
    }
}
