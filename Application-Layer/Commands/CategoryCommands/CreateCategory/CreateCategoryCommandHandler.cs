using MediatR;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Domain_Layer.Models;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDTO>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<CategoryDTO> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = _mapper.Map<CategoryModel>(request.CategoryCreateDto);
        await _categoryRepository.AddCategoryAsync(category);
        return _mapper.Map<CategoryDTO>(category);
    }
} 