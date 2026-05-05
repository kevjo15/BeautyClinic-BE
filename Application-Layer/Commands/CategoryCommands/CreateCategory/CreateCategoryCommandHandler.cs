using Domain_Layer.Common;
using MediatR;
using Domain_Layer.Models;

namespace Application_Layer.Commands.CategoryCommands.CreateCategory
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, OperationResult<CategoryModel>>
    {
        private readonly ICategoryRepository _categoryRepository;

        public CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<OperationResult<CategoryModel>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = new CategoryModel { Name = request.CategoryName };
            return await _categoryRepository.AddCategoryAsync(category);
        }
    }
}
