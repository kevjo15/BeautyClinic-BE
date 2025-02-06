using Application_Layer.Commands.CategoryCommands.UpdateCategory;
using Domain_Layer.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, UpdateCategoryResult>
{
    private readonly ICategoryRepository _categoryRepository;

    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<UpdateCategoryResult> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Check if the category exists
        var existingCategory = await _categoryRepository.GetByIdAsync(request.Id);
        if (existingCategory == null)
        {
            return new UpdateCategoryResult
            {
                Success = false,
                Message = "Category not found."
            };
        }

        // Proceed with the update
        existingCategory.Name = request.CategoryDto.Name; // Update the name or other properties as needed
        var success = await _categoryRepository.UpdateCategoryAsync(existingCategory);

        return new UpdateCategoryResult
        {
            Success = success,
            Message = success ? "Category updated successfully." : "Failed to update category."
        };
    }
} 