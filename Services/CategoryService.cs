using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Market.Models;
using Market.Domain.Repositories;
using Market.Domain.Services;
using Market.Domain.Communication;
using Market.Extensions;

namespace Market.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Category>> ListAsync()
        { 
            return await _categoryRepository.ListAsync();
        }

        public async Task<CategoryResponse> SaveAsync(Category category)
        {
            category.Name = category.Name.Trim();
            try
            {
                await _categoryRepository.AddAsync(category);
                await _unitOfWork.CompleteAsync();
                
                return new CategoryResponse(category);
            }
            catch (Exception)
            {
                return new CategoryResponse("The category could not be saved.");
            }
        }

        public async Task<CategoryResponse> UpdateAsync(int id, Category category)
        {
            var existingCategory = await _categoryRepository.FindByIdAsync(id);

            if (existingCategory == null)
                return new CategoryResponse("Category not found.");

            existingCategory.Name = category.Name.Trim();

            try
            {
                _categoryRepository.Update(existingCategory);
                await _unitOfWork.CompleteAsync();

                return new CategoryResponse(existingCategory);
            }
            catch (Exception)
            {
                return new CategoryResponse("The category could not be updated.");
            }
        }

        public async Task<CategoryResponse> DeleteAsync(int id)
        {
            var existingCategory = await _categoryRepository.FindByIdAsync(id);

            if (existingCategory == null)
                return new CategoryResponse("Category not found.");

            try
            {
                _categoryRepository.Remove(existingCategory);
                await _unitOfWork.CompleteAsync();

                return new CategoryResponse(existingCategory);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                return new CategoryResponse("The category cannot be deleted while products use it.");
            }
            catch (Exception)
            {
                return new CategoryResponse("The category could not be deleted.");
            }
        }
    }
}