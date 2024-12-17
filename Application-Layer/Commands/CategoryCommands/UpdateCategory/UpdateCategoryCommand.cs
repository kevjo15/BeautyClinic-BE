using MediatR;
using Domain_Layer.Models;

public class UpdateCategoryCommand : IRequest<bool>
{
    public CategoryModel Category { get; }

    public UpdateCategoryCommand(CategoryModel category)
    {
        Category = category;
    }
} 