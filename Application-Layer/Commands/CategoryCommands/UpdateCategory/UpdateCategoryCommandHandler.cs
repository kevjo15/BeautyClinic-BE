using Application_Layer.Commands.CategoryCommands.UpdateCategory;
using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, OperationResult<CategoryModel>>
{
    private readonly ICategoryRepository _categoryRepository;

    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<OperationResult<CategoryModel>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(request.Id);
        if (existingCategory == null)
        {
            return OperationResult<CategoryModel>.Failure("Category not found.");
        }

        existingCategory.Name = request.CategoryDto.Name;
        return await _categoryRepository.UpdateCategoryAsync(existingCategory);
    }
}
