using MediatR;

public class CreateCategoryCommand : IRequest<CategoryDTO>
{
    public CategoryCreateDTO CategoryCreateDto { get; }

    public CreateCategoryCommand(CategoryCreateDTO categoryCreateDto)
    {
        CategoryCreateDto = categoryCreateDto;
    }
} 