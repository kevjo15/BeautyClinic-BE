using MediatR;
using Application_Layer.Commands.CategoryCommands.DeleteCategory;
using Domain_Layer.Common;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, OperationResult>
{
    private readonly ICategoryRepository _categoryRepository;

    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<OperationResult> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        return await _categoryRepository.DeleteCategoryAsync(request.CategoryId);
    }
}
