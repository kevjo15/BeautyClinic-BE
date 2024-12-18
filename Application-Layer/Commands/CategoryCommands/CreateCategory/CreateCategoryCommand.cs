using MediatR;
using Application_Layer.Commands.CategoryCommands.CreateCategory;

namespace Application_Layer.Commands.CategoryCommands.CreateCategory
{
    public class CreateCategoryCommand : IRequest<CreateCategoryResult>
    {
        public string CategoryName { get; }

        public CreateCategoryCommand(string categoryName)
        {
            CategoryName = categoryName;
        }
    }
} 