using Domain_Layer.Models;
using Infrastructure_Layer.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    public async Task AddCategoryAsync(CategoryModel category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateCategoryAsync(CategoryModel category)
    {
        var existingCategory = await _context.Categories.FindAsync(category.Id);
        if (existingCategory == null) return false;

        existingCategory.Name = category.Name;
        _context.Categories.Update(existingCategory);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(Guid id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddServiceToCategoryAsync(Guid categoryId, ServiceModel service)
    {
        var category = await _context.Categories.Include(c => c.Services).FirstOrDefaultAsync(c => c.Id == categoryId);
        if (category == null) return false;

        category.Services.Add(service);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveServiceFromCategoryAsync(Guid categoryId, Guid serviceId)
    {
        var category = await _context.Categories.Include(c => c.Services).FirstOrDefaultAsync(c => c.Id == categoryId);
        if (category == null) return false;

        var service = category.Services.FirstOrDefault(s => s.Id == serviceId);
        if (service == null) return false;

        category.Services.Remove(service);
        await _context.SaveChangesAsync();
        return true;
    }
} 