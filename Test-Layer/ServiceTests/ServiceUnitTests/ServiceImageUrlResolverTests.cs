using Application_Layer.Common;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Test_Layer.ServiceTests.ServiceUnitTests;

[TestFixture]
public class ServiceImageUrlResolverTests
{
    private IFileService _fileService = null!;
    private ServiceImageUrlResolver _resolver = null!;

    [SetUp]
    public void SetUp()
    {
        _fileService = A.Fake<IFileService>();
        _resolver = new ServiceImageUrlResolver(_fileService, NullLogger<ServiceImageUrlResolver>.Instance);
    }

    [Test]
    public async Task ResolveAsync_WhenValueIsEmpty_ReturnsEmptyWithoutCallingFileService()
    {
        var result = await _resolver.ResolveAsync("", CancellationToken.None);

        Assert.That(result, Is.EqualTo(string.Empty));
        A.CallTo(() => _fileService.GenerateServiceImageReadUrlAsync(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task ResolveAsync_WhenValueIsLegacyFullUrl_ReturnsItUnchanged()
    {
        var legacyUrl = "http://127.0.0.1:10000/devstoreaccount1/services/old.png";

        var result = await _resolver.ResolveAsync(legacyUrl, CancellationToken.None);

        Assert.That(result, Is.EqualTo(legacyUrl));
        A.CallTo(() => _fileService.GenerateServiceImageReadUrlAsync(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task ResolveAsync_WhenValueIsBlobPath_ReturnsSignedUrl()
    {
        A.CallTo(() => _fileService.GenerateServiceImageReadUrlAsync("images/services/a.png", A<CancellationToken>._))
            .Returns("https://signed-url");

        var result = await _resolver.ResolveAsync("images/services/a.png", CancellationToken.None);

        Assert.That(result, Is.EqualTo("https://signed-url"));
    }

    [Test]
    public async Task ResolveAsync_WhenFileServiceThrows_ReturnsEmptyInsteadOfFailing()
    {
        A.CallTo(() => _fileService.GenerateServiceImageReadUrlAsync(A<string>._, A<CancellationToken>._))
            .Throws(new InvalidOperationException("bad blob path"));

        var result = await _resolver.ResolveAsync("not-a-valid-path", CancellationToken.None);

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task ApplyAsync_ResolvesEveryServiceInPlace()
    {
        var dtos = new List<ServiceDTO>
        {
            new() { Name = "Botox", ImageUrl = "images/services/a.png" },
            new() { Name = "Konsultation", ImageUrl = "" }
        };
        A.CallTo(() => _fileService.GenerateServiceImageReadUrlAsync("images/services/a.png", A<CancellationToken>._))
            .Returns("https://signed-url");

        await _resolver.ApplyAsync(dtos, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(dtos[0].ImageUrl, Is.EqualTo("https://signed-url"));
            Assert.That(dtos[1].ImageUrl, Is.EqualTo(string.Empty));
        });
    }
}
