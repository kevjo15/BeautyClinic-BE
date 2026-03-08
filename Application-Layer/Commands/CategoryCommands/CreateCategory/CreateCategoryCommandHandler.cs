using MediatR;
using Domain_Layer.Models;

namespace Application_Layer.Commands.CategoryCommands.CreateCategory
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResult>
    {
        private readonly ICategoryRepository _categoryRepository;

        public CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
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