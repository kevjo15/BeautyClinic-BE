using Domain_Layer.Common;
using Domain_Layer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICategoryRepository
{
    Task<IEnumerable<CategoryModel>> GetAllCategoriesAsync();
    Task<OperationResult<CategoryModel>> AddCategoryAsync(CategoryModel category);
    Task<OperationResult<CategoryModel>> UpdateCategoryAsync(CategoryModel category);
    Task<OperationResult> DeleteCategoryAsync(Guid id);
    Task<CategoryModel?> GetByIdAsync(Guid id);
}
