using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application_Layer.DTOs;
using AutoMapper;
using System.Linq;

namespace Application_Layer.Queries.CategoryQueries.GetCategoriesWithServices
{
    public class GetCategoriesWithServicesQueryHandler : IRequestHandler<GetCategoriesWithServicesQuery, IEnumerable<CategoryWithServicesDTO>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public GetCategoriesWithServicesQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryWithServicesDTO>> Handle(GetCategoriesWithServicesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            return _mapper.Map<IEnumerable<CategoryWithServicesDTO>>(categories);
        }
    }
}
