using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Mapping;
using System.Linq;

namespace Application_Layer.Queries.CategoryQueries.GetCategoriesWithServices
{
    public class GetCategoriesWithServicesQueryHandler : IRequestHandler<GetCategoriesWithServicesQuery, IEnumerable<CategoryWithServicesDTO>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IServiceImageUrlResolver _imageUrlResolver;
        private readonly IApplicationMapper _mapper;

        public GetCategoriesWithServicesQueryHandler(
            ICategoryRepository categoryRepository,
            IServiceImageUrlResolver imageUrlResolver,
            IApplicationMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _imageUrlResolver = imageUrlResolver;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CategoryWithServicesDTO>> Handle(GetCategoriesWithServicesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            var dtos = _mapper.ToCategoryWithServicesDtoList(categories);
            await _imageUrlResolver.ApplyAsync(dtos.SelectMany(c => c.Services), cancellationToken);
            return dtos;
        }
    }
}
