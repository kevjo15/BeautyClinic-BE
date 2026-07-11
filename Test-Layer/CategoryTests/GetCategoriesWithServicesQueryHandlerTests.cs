using Application_Layer.Common;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Queries.CategoryQueries.GetCategoriesWithServices;
using Application_Layer.Mapping;
using Domain_Layer.Models;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;

namespace Test_Layer.CategoryTests;

[TestFixture]
public class GetCategoriesWithServicesQueryHandlerTests
{
    private ICategoryRepository _categoryRepository = null!;
    private IFileService _fileService = null!;
    private IApplicationMapper _mapper = null!;
    private GetCategoriesWithServicesQueryHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _categoryRepository = A.Fake<ICategoryRepository>();
        _fileService = A.Fake<IFileService>();
        _mapper = A.Fake<IApplicationMapper>();

        var resolver = new ServiceImageUrlResolver(_fileService, NullLogger<ServiceImageUrlResolver>.Instance);
        _handler = new GetCategoriesWithServicesQueryHandler(_categoryRepository, resolver, _mapper);
    }

    [Test]
    public async Task Handle_ResolvesImageUrlsForNestedServices()
    {
        var category = new CategoryModel { Id = Guid.NewGuid(), Name = "Fillers" };
        var mappedDtos = new List<CategoryWithServicesDTO>
        {
            new()
            {
                Id = category.Id,
                Name = category.Name,
                Services =
                [
                    new ServiceDTO { Name = "Läppfillers", ImageUrl = "images/services/a.png" }
                ]
            }
        };

        A.CallTo(() => _categoryRepository.GetAllCategoriesAsync())
            .Returns([category]);
        A.CallTo(() => _mapper.ToCategoryWithServicesDtoList(A<IEnumerable<CategoryModel>>._))
            .Returns(mappedDtos);
        A.CallTo(() => _fileService.GenerateServiceImageReadUrlAsync("images/services/a.png", A<CancellationToken>._))
            .Returns("https://signed-url");

        var result = (await _handler.Handle(new GetCategoriesWithServicesQuery(), CancellationToken.None)).ToList();

        Assert.That(result[0].Services.First().ImageUrl, Is.EqualTo("https://signed-url"));
    }
}
