using MediatR;
using AutoMapper;
using Domain_Layer.Models;

namespace Application_Layer.Commands.CategoryCommands.CreateCategory
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResult>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<CreateCategoryResult> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = new CategoryModel { Name = request.CategoryName };
            await _categoryRepository.AddCategoryAsync(category);

            return new CreateCategoryResult
            {
                Id = category.Id,
                Message = "Category created successfully.",
                Success = true
            };
        }
    }
}