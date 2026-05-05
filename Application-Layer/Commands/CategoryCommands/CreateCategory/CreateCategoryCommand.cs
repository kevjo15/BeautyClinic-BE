using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Commands.CategoryCommands.CreateCategory
{
    public class CreateCategoryCommand : IRequest<OperationResult<CategoryModel>>
    {
        public string CategoryName { get; }

        public CreateCategoryCommand(string categoryName)
        {
            CategoryName = categoryName;
        }
    }
}
