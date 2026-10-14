using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Market.Models;
using Market.Domain.Services;
using Market.Resources;
using Market.Extensions;
using Market.Mapping;


namespace Market.Controllers
{
    [Route("/api/[controller]")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;   
        }

        [HttpGet]
        public async Task<IEnumerable<CategoryResource>> GetAllAsync()
        {
            var categories = await _categoryService.ListAsync();
            return System.Linq.Enumerable.Select(categories, MarketResourceMapper.ToResource);
        }

        [Authorize(Roles = UserRoles.Admin), HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] SaveCategoryResource resource)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.GetErrorMessages());

            var category = MarketResourceMapper.ToModel(resource);
            var result = await _categoryService.SaveAsync(category);

            if (!result.Success)
                return Conflict(new { message = result.Message });

            var categoryResource = MarketResourceMapper.ToResource(result.Category);
            return Ok(categoryResource);
        }

        [Authorize(Roles = UserRoles.Admin), HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(int id, [FromBody] SaveCategoryResource resource)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.GetErrorMessages());

            var category = MarketResourceMapper.ToModel(resource);
            var result = await _categoryService.UpdateAsync(id, category);

            if (!result.Success)
                return result.Message == "Category not found." ? NotFound(new { message = result.Message }) : Conflict(new { message = result.Message });

            var categoryResource = MarketResourceMapper.ToResource(result.Category);
            return Ok(categoryResource);
        }

        [Authorize(Roles = UserRoles.Admin), HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await _categoryService.DeleteAsync(id);

            if (!result.Success)
                return result.Message == "Category not found." ? NotFound(new { message = result.Message }) : Conflict(new { message = result.Message });

            var categoryResource = MarketResourceMapper.ToResource(result.Category);
            return Ok(categoryResource);
        }
    }

}
