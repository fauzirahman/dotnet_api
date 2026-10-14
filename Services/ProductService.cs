using System.Collections.Generic;
using System.Threading.Tasks;
using Market.Models;
using Market.Domain.Repositories;
using Market.Domain.Services;
using Market.Domain.Communication;
using Market.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;


namespace Market.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
    
        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Product>> ListAsync()
        {
            return await _productRepository.ListAsync();
        }

        public async Task<ProductResponse> SaveAsync(Product product)
        {
            product.Name = product.Name.Trim();
            if (product.Price <= 0 || product.StockQuantity < 0)
                return new ProductResponse("Price must be positive and stock cannot be negative.");
            var category = await _categoryRepository.FindByIdAsync(product.CategoryId);
            if (category == null)
                return new ProductResponse("Category not found.");
            product.Category = category;
            await _productRepository.AddAsync(product);
            await _unitOfWork.CompleteAsync();
            return new ProductResponse(product);
        }

        public async Task<ProductResponse> UpdateAsync(int id, Product product)
        {
            var existing = await _productRepository.FindByIdAsync(id);
            if (existing == null)
                return new ProductResponse("Product not found.");
            if (product.Price <= 0 || product.StockQuantity < 0)
                return new ProductResponse("Price must be positive and stock cannot be negative.");
            var category = await _categoryRepository.FindByIdAsync(product.CategoryId);
            if (category == null)
                return new ProductResponse("Category not found.");

            existing.Name = product.Name.Trim();
            existing.QuantityInPackage = product.QuantityInPackage;
            existing.UnitOfMeasurement = product.UnitOfMeasurement;
            existing.Price = product.Price;
            existing.StockQuantity = product.StockQuantity;
            existing.CategoryId = product.CategoryId;
            existing.Category = category;
            await _unitOfWork.CompleteAsync();
            return new ProductResponse(existing);
        }

        public async Task<ProductResponse> DeleteAsync(int id)
        {
            var existing = await _productRepository.FindByIdAsync(id);
            if (existing == null)
                return new ProductResponse("Product not found.");

            try
            {
                _productRepository.Remove(existing);
                await _unitOfWork.CompleteAsync();
                return new ProductResponse(existing);
            }
            catch (DbUpdateException)
            {
                return new ProductResponse("The product cannot be deleted because it is part of order history.");
            }
        }
    }
}