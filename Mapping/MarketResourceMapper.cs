using Market.Extensions;
using Market.Models;
using Market.Resources;

namespace Market.Mapping
{
    public static class MarketResourceMapper
    {
        public static CategoryResource ToResource(Category category)
        {
            return new CategoryResource { Id = category.Id, Name = category.Name };
        }

        public static Category ToModel(SaveCategoryResource resource)
        {
            return new Category { Name = resource.Name };
        }

        public static ProductResource ToResource(Product product)
        {
            return new ProductResource
            {
                Id = product.Id,
                Name = product.Name,
                QuantityInPackage = product.QuantityInPackage,
                UnitOfMeasurement = product.UnitOfMeasurement.ToDescriptionString(),
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Category = product.Category == null ? null : ToResource(product.Category)
            };
        }

        public static Product ToModel(SaveProductResource resource)
        {
            return new Product
            {
                Name = resource.Name,
                QuantityInPackage = resource.QuantityInPackage,
                UnitOfMeasurement = resource.UnitOfMeasurement,
                Price = resource.Price,
                StockQuantity = resource.StockQuantity,
                CategoryId = resource.CategoryId
            };
        }
    }
}