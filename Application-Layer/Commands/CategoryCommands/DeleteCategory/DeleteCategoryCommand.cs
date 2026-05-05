using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.CategoryCommands.DeleteCategory
{
    public class DeleteCategoryCommand : IRequest<OperationResult>
    {
        public Guid CategoryId { get; }

        public DeleteCategoryCommand(Guid categoryId)
        {
            CategoryId = categoryId;
        }
    }
} 
