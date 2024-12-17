using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application_Layer.DTOs;

namespace Application_Layer.Queries.CategoryQueries
{
    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryNameDTO>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryNameDTO>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            return categories.Select(c => new CategoryNameDTO
            {
                Name = c.Name
            }).ToList();
        }
    }
} 