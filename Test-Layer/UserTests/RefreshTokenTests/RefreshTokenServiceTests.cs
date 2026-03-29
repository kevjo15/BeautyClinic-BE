using Application_Layer.Interfaces;
using Domain_Layer.Models;
using FakeItEasy;
using Infrastructure_Layer.Identity;
using Microsoft.Extensions.Configuration;

namespace Test_Layer.UserTests.RefreshTokenTests
{
    [TestFixture]
    public class RefreshTokenServiceTests
    {
        private RefreshTokenService _service;
        private IRefreshTokenRepository _repository;
        private IConfiguration _configuration;

        [SetUp]
        public void SetUp()
        {
            _repository = A.Fake<IRefreshTokenRepository>();

            // Setup fake configuration
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "JwtSettings:RefreshTokenExpiryDays", "7" },
                { "JwtSettings:RefreshTokenPepper", "TestPepper123" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _service = new RefreshTokenService(_repository, _configuration);
        }

        [Test]
        public async Task GenerateRefreshTokenAsync_ShouldCreateTokenWithHashedValue()
        {
            // Arrange
            var userId = "user-123";
            UserRefreshToken? savedToken = null;

            A.CallTo(() => _repository.CreateAsync(A<UserRefreshToken>._))
                .Invokes((UserRefreshToken t) => savedToken = t)
                .ReturnsLazily((UserRefreshToken t) => Task.FromResult(t));

            // Act
            var (rawToken, tokenEntity) = await _service.GenerateRefreshTokenAsync(userId, "127.0.0.1", "Test-Agent");

            // Assert
            Assert.IsNotNull(rawToken);
            Assert.IsNotEmpty(rawToken);
            Assert.That(tokenEntity.UserId, Is.EqualTo(userId));
            Assert.That(tokenEntity.CreatedByIp, Is.EqualTo("127.0.0.1"));
            Assert.That(tokenEntity.UserAgent, Is.EqualTo("Test-Agent"));
            Assert.That(tokenEntity.ExpiresAt, Is.GreaterThan(DateTime.UtcNow));
            Assert.IsNull(tokenEntity.RevokedAt);

            // Verify token hash is not the same as raw token (it's hashed)
            Assert.That(tokenEntity.TokenHash, Is.Not.EqualTo(rawToken));

            // Verify the hash is consistent
            var computedHash = _service.HashToken(rawToken);
            Assert.That(tokenEntity.TokenHash, Is.EqualTo(computedHash));
        }

        [Test]
        public async Task ValidateRefreshTokenAsync_WithValidToken_ShouldReturnSuccess()
        {
            // Arrange
            var rawToken = "valid_raw_token";
            var tokenHash = _service.HashToken(rawToken);
            var userId = "user-123";

            var storedToken = new UserRefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                RevokedAt = null
            };

            A.CallTo(() => _repository.GetByTokenHashAsync(tokenHash)).Returns(storedToken);

            // Act
            var (isValid, token, error) = await _service.ValidateRefreshTokenAsync(rawToken);

            // Assert
            Assert.IsTrue(isValid);
            Assert.IsNotNull(token);
            Assert.IsNull(error);
            Assert.That(token!.UserId, Is.EqualTo(userId));
        }

        [Test]
        public async Task ValidateRefreshTokenAsync_WithRevokedToken_ShouldInvalidateChainAndReturnError()
        {
            // Arrange
            var rawToken = "revoked_token";
            var tokenHash = _service.HashToken(rawToken);
            var userId = "user-123";

            var revokedToken = new UserRefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                RevokedAt = DateTime.UtcNow.AddHours(-1) // Token was revoked
            };

            A.CallTo(() => _repository.GetByTokenHashAsync(tokenHash)).Returns(revokedToken);

            // Act
            var (isValid, token, error) = await _service.ValidateRefreshTokenAsync(rawToken);

            // Assert
            Assert.IsFalse(isValid);
            Assert.IsNull(token);
            Assert.That(error, Does.Contain("revoked"));

            // Verify that RevokeTokenChainAsync was called (token reuse detection)
            A.CallTo(() => _repository.RevokeTokenChainAsync(tokenHash, A<string>._, A<string>._))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task ValidateRefreshTokenAsync_WithExpiredToken_ShouldReturnError()
        {
            // Arrange
            var rawToken = "expired_token";
            var tokenHash = _service.HashToken(rawToken);

            var expiredToken = new UserRefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = "user-123",
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                RevokedAt = null
            };

            A.CallTo(() => _repository.GetByTokenHashAsync(tokenHash)).Returns(expiredToken);

            // Act
            var (isValid, token, error) = await _service.ValidateRefreshTokenAsync(rawToken);

            // Assert
            Assert.IsFalse(isValid);
            Assert.IsNull(token);
            Assert.That(error, Does.Contain("expired"));
        }

        [Test]
        public async Task ValidateRefreshTokenAsync_WithNonExistentToken_ShouldReturnError()
        {
            // Arrange
            var rawToken = "nonexistent_token";
            var tokenHash = _service.HashToken(rawToken);

            A.CallTo(() => _repository.GetByTokenHashAsync(tokenHash)).Returns((UserRefreshToken?)null);

            // Act
            var (isValid, token, error) = await _service.ValidateRefreshTokenAsync(rawToken);

            // Assert
            Assert.IsFalse(isValid);
            Assert.IsNull(token);
            Assert.That(error, Does.Contain("Invalid"));
        }

        [Test]
        public async Task RotateRefreshTokenAsync_ShouldRevokeOldAndCreateNewToken()
        {
            // Arrange
            var oldRawToken = "old_token";
            var oldTokenHash = _service.HashToken(oldRawToken);
            var userId = "user-123";

            var oldToken = new UserRefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = oldTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                RevokedAt = null
            };

            A.CallTo(() => _repository.GetByTokenHashAsync(oldTokenHash)).Returns(oldToken);
            A.CallTo(() => _repository.CreateAsync(A<UserRefreshToken>._))
                .ReturnsLazily((UserRefreshToken t) => Task.FromResult(t));

            // Act
            var result = await _service.RotateRefreshTokenAsync(oldRawToken, "127.0.0.1", "Test-Agent");

            // Assert
            Assert.IsNotNull(result);
            var (newRawToken, newTokenEntity) = result.Value;

            Assert.IsNotNull(newRawToken);
            Assert.IsNotEmpty(newRawToken);
            Assert.That(newTokenEntity.UserId, Is.EqualTo(userId));

            // Verify old token was updated (revoked and replacement set)
            A.CallTo(() => _repository.UpdateAsync(A<UserRefreshToken>.That.Matches(t =>
                t.Id == oldToken.Id &&
                t.RevokedAt != null &&
                t.ReplacedByTokenHash == newTokenEntity.TokenHash)))
                .MustHaveHappenedOnceExactly();

            // Verify new token was created
            A.CallTo(() => _repository.CreateAsync(A<UserRefreshToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task RevokeRefreshTokenAsync_ShouldMarkTokenAsRevoked()
        {
            // Arrange
            var rawToken = "token_to_revoke";
            var tokenHash = _service.HashToken(rawToken);
            var userId = "user-123";

            var token = new UserRefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                RevokedAt = null
            };

            A.CallTo(() => _repository.GetByTokenHashAsync(tokenHash)).Returns(token);

            // Act
            var result = await _service.RevokeRefreshTokenAsync(rawToken, "127.0.0.1", "User logout");

            // Assert
            Assert.IsTrue(result);
            A.CallTo(() => _repository.UpdateAsync(A<UserRefreshToken>.That.Matches(t =>
                t.Id == token.Id &&
                t.RevokedAt != null &&
                t.RevokedByIp == "127.0.0.1" &&
                t.RevokedReason == "User logout")))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public void HashToken_ShouldProduceDeterministicHash()
        {
            // Arrange
            var rawToken = "test_token_12345";

            // Act
            var hash1 = _service.HashToken(rawToken);
            var hash2 = _service.HashToken(rawToken);

            // Assert
            Assert.That(hash1, Is.EqualTo(hash2));
            Assert.That(hash1, Is.Not.EqualTo(rawToken));
        }

        [Test]
        public void HashToken_DifferentTokens_ShouldProduceDifferentHashes()
        {
            // Arrange
            var token1 = "token_one";
            var token2 = "token_two";

            // Act
            var hash1 = _service.HashToken(token1);
            var hash2 = _service.HashToken(token2);

            // Assert
            Assert.That(hash1, Is.Not.EqualTo(hash2));
        }
    }
}
