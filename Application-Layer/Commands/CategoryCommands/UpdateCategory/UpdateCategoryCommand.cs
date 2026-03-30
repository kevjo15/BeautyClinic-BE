using MediatR;
using Application_Layer.DTOs;
using Domain_Layer.Common;
using Domain_Layer.Models;

namespace Application_Layer.Commands.CategoryCommands.UpdateCategory
{
    public class UpdateCategoryCommand : IRequest<OperationResult<CategoryModel>>
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
