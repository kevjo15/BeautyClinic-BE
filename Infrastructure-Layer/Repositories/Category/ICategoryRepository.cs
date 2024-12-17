using Domain_Layer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICategoryRepository
{
    Task<IEnumerable<CategoryModel>> GetAllCategoriesAsync();
    Task AddCategoryAsync(CategoryModel category);
    Task<bool> UpdateCategoryAsync(CategoryModel category);
    Task<bool> DeleteCategoryAsync(Guid id);
    Task<bool> AddServiceToCategoryAsync(Guid categoryId, ServiceModel service);
    Task<bool> RemoveServiceFromCategoryAsync(Guid categoryId, Guid serviceId);
} 