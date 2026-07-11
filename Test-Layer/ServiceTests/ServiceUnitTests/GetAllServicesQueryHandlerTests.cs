using Application_Layer.Common;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Queries.ServiceQueries.GetAllServices;
using Application_Layer.Mapping;
using Domain_Layer.Models;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Test_Layer.ServiceTests.ServiceUnitTests;

[TestFixture]
public class GetAllServicesQueryHandlerTests
{
    private IServiceRepository _serviceRepository = null!;
    private IFileService _fileService = null!;
    private IApplicationMapper _mapper = null!;
    private GetAllServicesQueryHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _serviceRepository = A.Fake<IServiceRepository>();
        _fileService = A.Fake<IFileService>();
        _mapper = A.Fake<IApplicationMapper>();

        var resolver = new ServiceImageUrlResolver(_fileService, NullLogger<ServiceImageUrlResolver>.Instance);
        _handler = new GetAllServicesQueryHandler(_serviceRepository, resolver, _mapper);
    }

    [Test]
    public async Task Handle_WhenServiceHasImageUrl_ShouldReplaceItWithSignedUrl()
    {
        var service = new ServiceModel
        {
            Id = Guid.NewGuid(),
            Name = "Botox",
            ImageUrl = "images/services/image.jpg"
        };

        var mappedDtos = new List<ServiceDTO>
        {
            new() { Id = service.Id, Name = service.Name, ImageUrl = service.ImageUrl }
        };

        A.CallTo(() => _serviceRepository.GetAllServicesAsync())
            .Returns([service]);
        A.CallTo(() => _mapper.ToServiceDtoList(A<IEnumerable<ServiceModel>>._))
            .Returns(mappedDtos);
        A.CallTo(() => _fileService.GenerateServiceImageReadUrlAsync("images/services/image.jpg", A<CancellationToken>._))
            .Returns("https://signed-url");

        var result = (await _handler.Handle(new GetAllServicesQuery(), CancellationToken.None)).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].ImageUrl, Is.EqualTo("https://signed-url"));
    }

    [Test]
    public async Task Handle_WhenServiceHasNoImageUrl_ShouldNotCallFileService()
    {
        var service = new ServiceModel
        {
            Id = Guid.NewGuid(),
            Name = "Consultation",
            ImageUrl = string.Empty
        };

        var mappedDtos = new List<ServiceDTO>
        {
            new() { Id = service.Id, Name = service.Name, ImageUrl = string.Empty }
        };

        A.CallTo(() => _serviceRepository.GetAllServicesAsync())
            .Returns([service]);
        A.CallTo(() => _mapper.ToServiceDtoList(A<IEnumerable<ServiceModel>>._))
            .Returns(mappedDtos);

        var result = (await _handler.Handle(new GetAllServicesQuery(), CancellationToken.None)).ToList();

        Assert.That(result[0].ImageUrl, Is.EqualTo(string.Empty));
        A.CallTo(() => _fileService.GenerateServiceImageReadUrlAsync(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
