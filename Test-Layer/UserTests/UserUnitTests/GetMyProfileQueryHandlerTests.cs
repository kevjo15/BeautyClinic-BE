using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Queries.UserQueries.GetMyProfile;
using Application_Layer.Mapping;
using Domain_Layer.Models;
using FakeItEasy;

namespace Test_Layer.UserTests.UserUnitTests;

[TestFixture]
public class GetMyProfileQueryHandlerTests
{
    private IUserRepository _userRepository = null!;
    private IServiceImageUrlResolver _imageUrlResolver = null!;
    private IApplicationMapper _mapper = null!;
    private GetMyProfileQueryHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _userRepository = A.Fake<IUserRepository>();
        _imageUrlResolver = A.Fake<IServiceImageUrlResolver>();
        _mapper = A.Fake<IApplicationMapper>();
        _handler = new GetMyProfileQueryHandler(_userRepository, _imageUrlResolver, _mapper);
    }

    [Test]
    public async Task Handle_WhenUserHasAvatar_ReturnsSignedAvatarUrl()
    {
        var user = new UserModel { Id = "user-1", AvatarUrl = "images/avatars/user-1" };
        A.CallTo(() => _userRepository.FindByIdAsync("user-1")).Returns(user);
        A.CallTo(() => _mapper.ToUserProfileDto(user))
            .Returns(new UserProfileDTO { UserId = "user-1" });
        A.CallTo(() => _imageUrlResolver.ResolveAsync("images/avatars/user-1", A<CancellationToken>._))
            .Returns("https://signed-avatar-url");

        var result = await _handler.Handle(new GetMyProfileQuery("user-1"), CancellationToken.None);

        Assert.That(result!.AvatarUrl, Is.EqualTo("https://signed-avatar-url"));
    }

    [Test]
    public async Task Handle_WhenUserHasNoAvatar_LeavesAvatarUrlNull()
    {
        var user = new UserModel { Id = "user-1", AvatarUrl = null };
        A.CallTo(() => _userRepository.FindByIdAsync("user-1")).Returns(user);
        A.CallTo(() => _mapper.ToUserProfileDto(user))
            .Returns(new UserProfileDTO { UserId = "user-1" });

        var result = await _handler.Handle(new GetMyProfileQuery("user-1"), CancellationToken.None);

        Assert.That(result!.AvatarUrl, Is.Null);
        A.CallTo(() => _imageUrlResolver.ResolveAsync(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenUserExists_ReturnsMappedProfile()
    {
        var user = new UserModel
        {
            Id = "user-1",
            Email = "karin@example.com",
            FirstName = "Karin",
            LastName = "Karlsson",
            PhoneNumber = "0705555555"
        };
        var profile = new UserProfileDTO
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber
        };

        A.CallTo(() => _userRepository.FindByIdAsync("user-1")).Returns(user);
        A.CallTo(() => _mapper.ToUserProfileDto(user)).Returns(profile);

        var result = await _handler.Handle(new GetMyProfileQuery("user-1"), CancellationToken.None);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.UserId, Is.EqualTo("user-1"));
            Assert.That(result.PhoneNumber, Is.EqualTo("0705555555"));
        });
    }

    [Test]
    public async Task Handle_WhenUserIsMissing_ReturnsNull()
    {
        A.CallTo(() => _userRepository.FindByIdAsync("missing")).Returns((UserModel?)null);

        var result = await _handler.Handle(new GetMyProfileQuery("missing"), CancellationToken.None);

        Assert.That(result, Is.Null);
    }
}
