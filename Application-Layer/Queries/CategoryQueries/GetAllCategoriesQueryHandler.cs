using Application_Layer.Mapping;
using MediatR;
using Application_Layer.DTOs;

namespace Application_Layer.Queries.CategoryQueries
{
    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryNameDTO>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IApplicationMapper _mapper;

        public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository, IApplicationMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryNameDTO>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            return _mapper.ToCategoryNameDtos(categories);
        }
    }
}