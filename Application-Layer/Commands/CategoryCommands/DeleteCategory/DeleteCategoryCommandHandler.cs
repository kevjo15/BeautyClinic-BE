using MediatR;
using Application_Layer.Commands.CategoryCommands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, DeleteCategoryResult>
{
    private readonly ICategoryRepository _categoryRepository;

    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<DeleteCategoryResult> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var success = await _categoryRepository.DeleteCategoryAsync(request.CategoryId);

        return new DeleteCategoryResult
        {
            Success = success,
            Message = success ? "Category deleted successfully." : "Failed to delete category."
        };
    }
}