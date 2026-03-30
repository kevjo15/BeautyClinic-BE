using Domain_Layer.Common;
using Domain_Layer.Models;
using Infrastructure_Layer.Database;
using Microsoft.EntityFrameworkCore;

public class CategoryRepository : ICategoryRepository
{
    private readonly ElsaBeautyDbContext _context;

    public CategoryRepository(ElsaBeautyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryModel>> GetAllCategoriesAsync()
    {
        return await _context.Categories.Include(c => c.Services).ToListAsync();
    }

    public async Task<OperationResult<CategoryModel>> AddCategoryAsync(CategoryModel category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
        return OperationResult<CategoryModel>.Success(category);
    }

    public async Task<OperationResult<CategoryModel>> UpdateCategoryAsync(CategoryModel category)
    {
        var existingCategory = await _context.Categories.FindAsync(category.Id);
        if (existingCategory == null)
        {
            return OperationResult<CategoryModel>.Failure("Category not found.");
        }

        existingCategory.Name = category.Name;
        _context.Categories.Update(existingCategory);
        await _context.SaveChangesAsync();
        return OperationResult<CategoryModel>.Success(existingCategory);
    }

    public async Task<OperationResult> DeleteCategoryAsync(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return OperationResult.Failure("Category not found.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return OperationResult.Success();
    }

    public async Task<CategoryModel?> GetByIdAsync(Guid id)
    {
        return await _context.Categories.FindAsync(id);
    }
}
