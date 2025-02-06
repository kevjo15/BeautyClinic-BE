using MediatR;

namespace Application_Layer.Commands.CategoryCommands.DeleteCategory
{
    public class DeleteCategoryCommand : IRequest<DeleteCategoryResult>
    {
        public Guid CategoryId { get; }

        public DeleteCategoryCommand(Guid categoryId)
        {
            CategoryId = categoryId;
        }
    }
} 