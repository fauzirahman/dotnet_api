using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Market.Models;
using Market.Domain.Services;
using Market.Resources;
using Market.Mapping;

namespace Market.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IEnumerable<ProductResource>> ListAsync()
        {
            var products = await _productService.ListAsync();
            return System.Linq.Enumerable.Select(products, MarketResourceMapper.ToResource);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            var products = await _productService.ListAsync();
            var product = System.Linq.Enumerable.FirstOrDefault(products, item => item.Id == id);
            return product == null ? NotFound() : Ok(MarketResourceMapper.ToResource(product));
        }

        [Authorize(Roles = UserRoles.Admin), HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] SaveProductResource resource)
        {
            var result = await _productService.SaveAsync(MarketResourceMapper.ToModel(resource));
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            return Created($"/api/products/{result.Product.Id}", MarketResourceMapper.ToResource(result.Product));
        }

        [Authorize(Roles = UserRoles.Admin), HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] SaveProductResource resource)
        {
            var result = await _productService.UpdateAsync(id, MarketResourceMapper.ToModel(resource));
            if (!result.Success)
                return result.Message == "Product not found." ? NotFound(new { message = result.Message }) : BadRequest(new { message = result.Message });
            return Ok(MarketResourceMapper.ToResource(result.Product));
        }

        [Authorize(Roles = UserRoles.Admin), HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await _productService.DeleteAsync(id);
            if (!result.Success)
                return result.Message == "Product not found." ? NotFound(new { message = result.Message }) : Conflict(new { message = result.Message });
            return NoContent();
        }
    }
}