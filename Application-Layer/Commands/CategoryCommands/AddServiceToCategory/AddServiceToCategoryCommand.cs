using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.CategoryCommands.AddServiceToCategory
{
    public class AddServiceToCategoryCommand : IRequest<OperationResult>
    {
        public Guid CategoryId { get; }
        public Guid ServiceId { get; }

        public AddServiceToCategoryCommand(Guid categoryId, Guid serviceId)
        {
            CategoryId = categoryId;
            ServiceId = serviceId;
        }
    }
} 
