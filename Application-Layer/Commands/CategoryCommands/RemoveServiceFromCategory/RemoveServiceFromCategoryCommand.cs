using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.CategoryCommands.RemoveServiceFromCategory
{
    public class RemoveServiceFromCategoryCommand : IRequest<OperationResult>
    {
        public Guid CategoryId { get; }
        public Guid ServiceId { get; }

        public RemoveServiceFromCategoryCommand(Guid categoryId, Guid serviceId)
        {
            CategoryId = categoryId;
            ServiceId = serviceId;
        }
    }
} 
