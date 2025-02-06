using MediatR;
using Application_Layer.DTOs;

namespace Application_Layer.Commands.CategoryCommands.UpdateCategory
{
    public class UpdateCategoryCommand : IRequest<UpdateCategoryResult>
    {
        public Guid Id { get; }
        public CategoryDTO CategoryDto { get; }

        public UpdateCategoryCommand(Guid id, CategoryDTO categoryDto)
        {
            Id = id;
            CategoryDto = categoryDto;
        }
    }
} 