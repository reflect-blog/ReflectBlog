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
    [Route("[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly BlogDbContext _dbContext;

        public CategoryController(BlogDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="search"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetCategory")]
        public async Task<IActionResult> GetCategory([Required] int id)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
                return NotFound();

            return Ok(category);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryModel"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="categoryModel"></param>
        /// <returns></returns>

        [Authorize(Roles = "Administrator")]
        [HttpPut("UpdateCategory")]
        public async Task<IActionResult> UpdateCategory(Category categoryModel)
        {
            var categoryToUpdate = _dbContext.Update(categoryModel);
            await _dbContext.SaveChangesAsync();

            return Ok(categoryToUpdate.Entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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