using ReflectBlog.Data;
using ReflectBlog.Entities;
using ReflectBlog.Helpers;
using ReflectBlog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ReflectBlog.Controllers
{
    [ApiController]
    [Produces("application/json", "application/problem+json")]
    [Authorize]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly BlogDbContext _dbContext;

        public CategoryController(BlogDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Method to get existing categories paginated
        /// </summary>
        /// <param name="search">Search keyword</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paginated date for categories</returns>
        [HttpGet("GetCategories")]
        public async Task<IActionResult> GetCategories(string search, int page = 1, int pageSize = 10)
        {
            Expression<Func<Category, bool>> searchCondition = x => x.Name.Contains(search);

            var categories = await _dbContext.Categories.WhereIf(!string.IsNullOrEmpty(search), searchCondition)
                                                    .OrderBy(x => x.Id)
                                                    .Skip((page - 1) * pageSize).Take(pageSize)
                                                   .ToListAsync();

            var categoriesPaged = new PagedInfo<Category>
            {
                Data = categories,
                TotalCount = await _dbContext.Categories.CountAsync(),
                PageSize = pageSize,
                Page = page
            };

            return Ok(categoriesPaged);
        }

        /// <summary>
        /// Get Categories Endpoint - List
        /// </summary>
        /// <returns>List of Categories</returns>
        [HttpGet("GetcategoriesList")]

        public async Task<IActionResult> Get()
        {
            var category = await _dbContext.Categories.ToListAsync();
            return Ok(category);
        }

        /// <summary>
        /// Method to get a category based on id
        /// </summary>
        /// <param name="id">Category Id to get</param>
        /// <returns>Category</returns>
        [HttpGet("GetCategory")]
        public async Task<IActionResult> GetCategory([Required] int id)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
                return NotFound();

            return Ok(category);
        }

        /// <summary>
        /// Create new category endpoint
        /// </summary>
        /// <param name="categoryModel">Model with required parameters for creating a new category</param>
        /// <returns>Newly created category</returns>
        [Authorize(Roles = "Administrator")]
        [HttpPost("PostCategory")]
        public async Task<IActionResult> PostCategory(CategoryModel categoryModel)
        {
            try
            {
                var category = new Category
                {
                    Name = categoryModel.Name,
                    IsDefault = categoryModel.IsDefault
                };
                var categoryToAdd = await _dbContext.AddAsync(category);
                await _dbContext.SaveChangesAsync();

                return Ok(categoryToAdd.Entity);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Endpoint to update an existing category
        /// </summary>
        /// <param name="categoryModel">Model with required parameters for updatng a category</param>
        /// <returns>Newly updated category</returns>
        [Authorize(Roles = "Administrator")]
        [HttpPut("UpdateCategory")]
        public async Task<IActionResult> UpdateCategory(Category categoryModel)
        {
            var categoryToUpdate = _dbContext.Update(categoryModel);
            await _dbContext.SaveChangesAsync();

            return Ok(categoryToUpdate.Entity);
        }

        /// <summary>
        /// Endpoint to delete a category by id
        /// </summary>
        /// <param name="id">Id of category to be deleted</param>
        /// <returns>Delete confirmation</returns>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("DeleteCategory")]
        public async Task<IActionResult> DeleteCategory([Required] int id)
        {
            var categoryToDelete = await _dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id);

            if (categoryToDelete == null)
                return NotFound();

            _dbContext.Remove(categoryToDelete);
            await _dbContext.SaveChangesAsync();

            return Ok("Deleted Category!");
        }
    }
}