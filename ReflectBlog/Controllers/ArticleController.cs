using ReflectBlog.Data;
using ReflectBlog.Entities;
using ReflectBlog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;
using ReflectBlog.Helpers;
using static System.Net.Mime.MediaTypeNames;
using System.Linq.Expressions;
using System.Security.Claims;

namespace ReflectBlog.Controllers
{
    [ApiController]
    [Produces("application/json", "application/problem+json")]
    [Authorize]
    [Route("[controller]")]
    public class ArticleController : ControllerBase
    {
        private readonly BlogDbContext _dbContext;

        public ArticleController(BlogDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("GetArticles")]
        public async Task<IActionResult> GetArticles(string search, int page = 1, int pageSize = 10)
        {
            Expression<Func<Article, bool>> searchCondition = x =>
            x.Title.Contains(search) ||
            x.Content.Contains(search) ||
            x.User.Email.Contains(search) ||
            x.User.GivenName.Contains(search) ||
            x.User.FamilyName.Contains(search);

            var articles = await _dbContext.Articles.Include(x => x.Category).Include(x => x.User)
                                                    .WhereIf(!string.IsNullOrEmpty(search), searchCondition)
                                                    .Skip((page - 1) * pageSize).Take(pageSize)
                                                   .ToListAsync();

            var articlesPaged = new PagedInfo<Article>
            {
                Data = articles,
                TotalCount = await _dbContext.Articles.CountAsync(),
                PageSize = pageSize,
                Page = page
            };

            return Ok(articlesPaged);
        }

        [HttpGet("GetArticle")]
        public async Task<IActionResult> GetArticle([Required] int id)
        {
            var article = await _dbContext.Articles.FirstOrDefaultAsync(x => x.Id == id);

            if (article == null)
                return NotFound();

            return Ok(article);
        }

        [Authorize(Roles = "Administrator,Author")]
        [HttpPost("PostArticleV1")]
        public async Task<IActionResult> PostArticleV1([FromForm] ArticleModel articleModel)
        {
            var article = new Article
            {
                Title = articleModel.Title,
                Content = articleModel.Content,
                Date = articleModel.Date,
                AuthorId = articleModel.AuthorId,
                CategoryId = articleModel.CategoryId
            };

            var extension = Path.GetExtension(articleModel.Image.FileName);

            if (extension != ".png")
            {
                return BadRequest("Only png files are accepted.");
            }

            var imgurResponseLink = await HelperMethods.ImgurImageUpload(articleModel.Image);

            article.ImageUrl = imgurResponseLink;

            var articleToAdd = await _dbContext.AddAsync(articleModel);
            await _dbContext.SaveChangesAsync();

            return Ok(articleToAdd.Entity);
        }

        [Authorize(Roles = "Administrator,Author")]
        [HttpPost("PostArticle")]
        public async Task<IActionResult> PostArticle(ArticleModel articleModel)
        {
            try
            {
                var article = new Article
                {
                    Title = articleModel.Title,
                    Content = articleModel.Content,
                    Date = articleModel.Date,
                    AuthorId = articleModel.AuthorId,
                    CategoryId = articleModel.CategoryId
                };

                var articleToAdd = await _dbContext.AddAsync(article);
                await _dbContext.SaveChangesAsync();

                return Ok(articleToAdd.Entity);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [Authorize(Roles = "Administrator,Author")]
        [HttpPut("UpdateArticle")]
        public async Task<IActionResult> UpdateArticle(Article articleModel)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var currentUser = HelperMethods.GetCurrentUser(identity);

            if (identity.Claims.FirstOrDefault(x => x.Type == "UserId").Value != articleModel.AuthorId.ToString())
                return BadRequest("You can update only articles created by you!");

            var articleToUpdate = _dbContext.Update(articleModel);
            await _dbContext.SaveChangesAsync();

            return Ok(articleToUpdate.Entity);
        }

        [Authorize(Roles = "Administrator,Author")]
        [HttpDelete("DeleteArticle")]
        public async Task<IActionResult> DeleteArticle([Required] int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var currentUser = HelperMethods.GetCurrentUser(identity);

            var articleToDelete = await _dbContext.Articles.FirstOrDefaultAsync(x => x.Id == id);

            if (identity.Claims.FirstOrDefault(x => x.Type == "UserId").Value != articleToDelete.AuthorId.ToString())
                return BadRequest("You can delete only articles created by you!");


            if (articleToDelete == null)
                return NotFound();

            _dbContext.Remove(articleToDelete);
            await _dbContext.SaveChangesAsync();

            return Ok("Deleted Article!");
        }

        [Authorize(Roles = "Administrator,Author")]
        [HttpPost("UploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            var extension = Path.GetExtension(image.FileName);

            if (extension != ".png")
            {
                return BadRequest("Only png files are accepted.");
            }

            var imgurResponseLink = await HelperMethods.ImgurImageUpload(image);

            //return ImgurResponseModel.Data.Link;
            return Ok(imgurResponseLink);
        }
    }
}