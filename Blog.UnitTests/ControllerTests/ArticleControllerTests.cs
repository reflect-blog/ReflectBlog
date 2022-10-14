using ReflectBlog.Controllers;
using ReflectBlog.Data;
using ReflectBlog.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Web;
using System.Net;
using ReflectBlog.Models;
using Xunit.Sdk;
using System.Diagnostics.Contracts;
using System.Threading;

namespace Blog.UnitTests.ControllerTests
{
    public class ArticleControllerTests
    {
        private readonly BlogDbContext dbContext;
        private readonly DbContextOptions<BlogDbContext> dbContextOptions;
        public ArticleControllerTests()
        {
            // Build DbContextOptions

            dbContextOptions = new DbContextOptionsBuilder<BlogDbContext>()
                       .UseInMemoryDatabase(databaseName: "BlogDb")
                       .Options;

            // Initialize new Context
            dbContext = new BlogDbContext(dbContextOptions);

            // Seed with test data
            MockData.Data.Seed(dbContext);
        }

        [Theory]
        [InlineData("A")]
        [InlineData("B")]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetArticles_WithSearch_ShouldReturn_OkResponse(string search)
        {
            // Arrange
            // We prepare the data that we will need to act on the test
            var user = await dbContext.Users.FirstOrDefaultAsync();

            var claims = new List<Claim>()
                {
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("ArticleName", user.Username.ToString()),
                    new Claim("Email", user.Email.ToString()),
                    new Claim("GivenName", user.GivenName.ToString()),
                    new Claim("FamilyName", user.FamilyName.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var articleController = new ArticleController(dbContext);
            articleController.ControllerContext = new ControllerContext();
            articleController.ControllerContext.HttpContext = new DefaultHttpContext();
            articleController.ControllerContext.HttpContext.User = claimsPrincipal;

            dbContext.Articles.AddRange(new List<Article>
            {
                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1,
                    CategoryId = 1
                },
                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1,
                    CategoryId = 1
                },
                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1,
                    CategoryId = 1
                }
            }
            );

            await dbContext.SaveChangesAsync();

            // Act
            // Here we act on the unit of code that we want to test

            var response = await articleController.GetArticles(search);
            var okResult = response as OkObjectResult;

            // Assert
            // Here we assert the results

            int totalCountInDb = await dbContext.Articles.CountAsync();
            int expectedDataCount = totalCountInDb >= 10 ? 10 : totalCountInDb;

            if (!string.IsNullOrEmpty(search))
            {
                expectedDataCount = await dbContext.Articles.Where(x => x.Title.Contains(search) || x.Content.Contains(search)).CountAsync();
            }

            Assert.NotNull(okResult);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);

            var data = okResult.Value as PagedInfo<Article>;

            Assert.NotNull(data.Data);
            Assert.Equal(expectedDataCount, data.Data.Count);
            Assert.Equal(totalCountInDb, data.TotalCount);
            Assert.Equal(10, data.PageSize);
        }

        [Theory]
        [InlineData(4, 10)]
        [InlineData(3, 10)]
        [InlineData(2, 10)]
        [InlineData(1, 10)]
        public async Task GetArticles_WithPageParameters_ShouldReturn_PagedResponse(int page, int pageSize)
        {
            // Arrange
            // We prepare the data that we will need to act on the test
            var user = await dbContext.Users.FirstOrDefaultAsync();

            var claims = new List<Claim>()
                {
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("UserName", user.Username.ToString()),
                    new Claim("Email", user.Email.ToString()),
                    new Claim("GivenName", user.GivenName.ToString()),
                    new Claim("FamilyName", user.FamilyName.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var articleController = new ArticleController(dbContext);
            articleController.ControllerContext = new ControllerContext();
            articleController.ControllerContext.HttpContext = new DefaultHttpContext();
            articleController.ControllerContext.HttpContext.User = claimsPrincipal;

            // Act

            var response = await articleController.GetArticles(null, page, pageSize);
            var okResult = response as OkObjectResult;

            // Assert

            var totalCountInDb = await dbContext.Articles.CountAsync();
            var totalItemsPerPage = await dbContext.Articles.Skip((page - 1) * pageSize).Take(pageSize).CountAsync();

            Assert.NotNull(okResult);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);

            var data = okResult.Value as PagedInfo<Article>;

            Assert.NotNull(data.Data);
            Assert.Equal(totalItemsPerPage, data.Data.Count);
            Assert.Equal(totalCountInDb, data.TotalCount);
            Assert.Equal(data.Page, page);
            Assert.Equal(data.PageSize, pageSize);
        }

        [Fact]
        public async Task GetArticle_WithExistingId_ShouldReturn_Success()
        {
            // Arrange

            var expectedArticle = await dbContext.Articles.FirstOrDefaultAsync();
            var articleController = new ArticleController(dbContext);

            // Act

            var response = await articleController.GetArticle(expectedArticle.Id);
            var okResult = response as OkObjectResult;

            // Assert

            var actualArticle = okResult.Value as Article;

            Assert.NotNull(actualArticle);
            Assert.Equal(expectedArticle.Id, actualArticle.Id);
            Assert.Equal(expectedArticle.Title, actualArticle.Title);
            Assert.Equal(expectedArticle.Content, actualArticle.Content);
        }

        [Fact]

        public async Task GetArticle_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange

            var id = await dbContext.Articles.MaxAsync(x => x.Id);
            var articleController = new ArticleController(dbContext);


            // Act

            var response = await articleController.GetArticle(id + 1);
            var result = response as NotFoundResult;

            // Assert

            Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task PostArticle_ValidArticleModel_ShouldReturnCreated()
        {
            // Arrange
            // We prepare the data that we will need to act on the test
            var user = await dbContext.Users.FirstOrDefaultAsync();

            var claims = new List<Claim>()
                {
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("UserName", user.Username.ToString()),
                    new Claim("Email", user.Email.ToString()),
                    new Claim("GivenName", user.GivenName.ToString()),
                    new Claim("FamilyName", user.FamilyName.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var articleController = new ArticleController(dbContext);
            articleController.ControllerContext = new ControllerContext();
            articleController.ControllerContext.HttpContext = new DefaultHttpContext();
            articleController.ControllerContext.HttpContext.User = claimsPrincipal;

            var articleToCreate = new ArticleModel
            {
                Title = Faker.Name.First(),
                Content = Faker.Lorem.Words(1).First(),
                AuthorId = 1,
                CategoryId = 1
            };

            // Act

            var response = await articleController.PostArticle(articleToCreate);
            var okResult = response as ObjectResult;

            // Assert

            Assert.NotNull(okResult);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
            //Assert.Equal((int)HttpStatusCode.Created, okResult.StatusCode);

            var actualArticle = okResult.Value as Article;
            Assert.NotNull(actualArticle);

            Assert.Equal(articleToCreate.Title, actualArticle.Title);
            Assert.Equal(articleToCreate.Content, actualArticle.Content);
        }

        [Fact]
        public async Task PostArticle_MissingRequiredFields_ShouldReturn_BadRequest()
        {
            // Arrange

            var articleController = new ArticleController(dbContext);
            articleController.ControllerContext = new ControllerContext();
            articleController.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act
            var result = await articleController.PostArticle(new ArticleModel
            {
                Content = Faker.Lorem.Words(1).First(),
                AuthorId = 1,
                CategoryId = 1
            });

            var objectRes = result as BadRequestResult;

            // Assert

            Assert.Equal((int)HttpStatusCode.BadRequest, objectRes.StatusCode);
            //Assert.Null(objectRes.Value);
        }

        [Fact]
        public async Task PostArticle_NullParameter_ShouldReturnBadRequest()
        {
            // Arrange

            var articleController = new ArticleController(dbContext);
            articleController.ControllerContext = new ControllerContext();
            articleController.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act

            var result = await articleController.PostArticle(null);
            var objectRes = result as BadRequestResult;

            // Assert

            Assert.Equal((int)HttpStatusCode.BadRequest, objectRes.StatusCode);
            //Assert.Null(objectRes.Value);
        }

        [Fact]
        public async Task DeleteArticle_ExistingUse_ShouldDeleteSuccessfully()
        {
            // Arrange

            var article = dbContext.Articles.FirstOrDefaultAsync();

            var articleController = new ArticleController(dbContext);
            articleController.ControllerContext = new ControllerContext();
            articleController.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act

            var result = await articleController.DeleteArticle(article.Id);
            var okResult = result as OkObjectResult;

            // Assert

            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
            Assert.Equal("Deleted Article!", (string)okResult.Value);


        }

        [Fact]
        public async Task DeleteArticle_NonExistingId_ShouldReturnNotFound()
        {
            // Arrange

            var id = await dbContext.Articles.MaxAsync(x => x.Id);

            var articleController = new ArticleController(dbContext);
            articleController.ControllerContext = new ControllerContext();
            articleController.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act

            var result = await articleController.DeleteArticle(id + 1);
            var objectResult = result as NotFoundResult;

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
        }
    }
}
