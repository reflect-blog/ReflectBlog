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
    public class CategoryControllerTests
    {
        private readonly BlogDbContext dbContext;
        private readonly DbContextOptions<BlogDbContext> dbContextOptions;
        public CategoryControllerTests()
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
        public async Task GetCategories_WithSearch_ShouldReturn_OkResponse(string search)
        {
            // Arrange
            // We prepare the data that we will need to act on the test
            var user = await dbContext.Users.FirstOrDefaultAsync();

            var claims = new List<Claim>()
                {
                    new Claim("UserId", user.Id.ToString()),
                    new Claim("CategoryName", user.Username.ToString()),
                    new Claim("Email", user.Email.ToString()),
                    new Claim("GivenName", user.GivenName.ToString()),
                    new Claim("FamilyName", user.FamilyName.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString()),
                };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var categoryController = new CategoryController(dbContext);
            categoryController.ControllerContext = new ControllerContext();
            categoryController.ControllerContext.HttpContext = new DefaultHttpContext();
            categoryController.ControllerContext.HttpContext.User = claimsPrincipal;

            dbContext.Categories.AddRange(new List<Category>
            {
                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                },
                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                },
                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                }
            }
            );

            await dbContext.SaveChangesAsync();

            // Act
            // Here we act on the unit of code that we want to test

            var response = await categoryController.GetCategories(search);
            var okResult = response as OkObjectResult;

            // Assert
            // Here we assert the results

            int totalCountInDb = await dbContext.Categories.CountAsync();
            int expectedDataCount = totalCountInDb >= 10 ? 10 : totalCountInDb;

            if (!string.IsNullOrEmpty(search))
            {
                expectedDataCount = await dbContext.Categories.Where(x => x.Name.Contains(search)).CountAsync();
            }

            Assert.NotNull(okResult);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);

            var data = okResult.Value as PagedInfo<Category>;

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
        public async Task GetCategories_WithPageParameters_ShouldReturn_PagedResponse(int page, int pageSize)
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

            var categoryController = new CategoryController(dbContext);
            categoryController.ControllerContext = new ControllerContext();
            categoryController.ControllerContext.HttpContext = new DefaultHttpContext();
            categoryController.ControllerContext.HttpContext.User = claimsPrincipal;

            // Act

            var response = await categoryController.GetCategories(null, page, pageSize);
            var okResult = response as OkObjectResult;

            // Assert

            var totalCountInDb = await dbContext.Categories.CountAsync();
            var totalItemsPerPage = await dbContext.Categories.Skip((page - 1) * pageSize).Take(pageSize).CountAsync();

            Assert.NotNull(okResult);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);

            var data = okResult.Value as PagedInfo<Category>;

            Assert.NotNull(data.Data);
            Assert.Equal(totalItemsPerPage, data.Data.Count);
            Assert.Equal(totalCountInDb, data.TotalCount);
            Assert.Equal(data.Page, page);
            Assert.Equal(data.PageSize, pageSize);
        }

        [Fact]
        public async Task GetCategory_WithExistingId_ShouldReturn_Success()
        {
            // Arrange

            var expectedCategory = await dbContext.Categories.FirstOrDefaultAsync();
            var categoryController = new CategoryController(dbContext);

            // Act

            var response = await categoryController.GetCategory(expectedCategory.Id);
            var okResult = response as OkObjectResult;

            // Assert

            var actualCategory = okResult.Value as Category;

            Assert.NotNull(actualCategory);
            Assert.Equal(expectedCategory.Id, actualCategory.Id);
            Assert.Equal(expectedCategory.Name, actualCategory.Name);
            Assert.Equal(expectedCategory.IsDefault, actualCategory.IsDefault);
        }

        [Fact]

        public async Task GetCategory_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange

            var id = await dbContext.Categories.MaxAsync(x => x.Id);
            var categoryController = new CategoryController(dbContext);


            // Act

            var response = await categoryController.GetCategory(id + 1);
            var result = response as NotFoundResult;

            // Assert

            Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task PostCategory_ValidCategoryModel_ShouldReturnCreated()
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

            var categoryController = new CategoryController(dbContext);
            categoryController.ControllerContext = new ControllerContext();
            categoryController.ControllerContext.HttpContext = new DefaultHttpContext();
            categoryController.ControllerContext.HttpContext.User = claimsPrincipal;

            var categoryToCreate = new CategoryModel
            {
                Name = Faker.Name.First(),
                IsDefault = true
            };

            // Act

            var response = await categoryController.PostCategory(categoryToCreate);
            var okResult = response as ObjectResult;

            // Assert

            Assert.NotNull(okResult);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
            //Assert.Equal((int)HttpStatusCode.Created, okResult.StatusCode);

            var actualCategory = okResult.Value as Category;
            Assert.NotNull(actualCategory);

            Assert.Equal(categoryToCreate.Name, actualCategory.Name);
            Assert.Equal(categoryToCreate.IsDefault, actualCategory.IsDefault);
        }

        [Fact]
        public async Task PostCategory_MissingRequiredFields_ShouldReturn_BadRequest()
        {
            // Arrange

            var categoryController = new CategoryController(dbContext);
            categoryController.ControllerContext = new ControllerContext();
            categoryController.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act
            var result = await categoryController.PostCategory(new CategoryModel
            {
                IsDefault = true
            });

            var objectRes = result as BadRequestResult;

            // Assert

            Assert.Equal((int)HttpStatusCode.BadRequest, objectRes.StatusCode);
            //Assert.Null(objectRes.Value);
        }

        [Fact]
        public async Task PostCategory_NullParameter_ShouldReturnBadRequest()
        {
            // Arrange

            var categoryController = new CategoryController(dbContext);
            categoryController.ControllerContext = new ControllerContext();
            categoryController.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act

            var result = await categoryController.PostCategory(null);
            var objectRes = result as BadRequestResult;

            // Assert

            Assert.Equal((int)HttpStatusCode.BadRequest, objectRes.StatusCode);
            //Assert.Null(objectRes.Value);
        }

        [Fact]
        public async Task DeleteCategory_ExistingUse_ShouldDeleteSuccessfully()
        {
            // Arrange

            var category = dbContext.Categories.FirstOrDefaultAsync();

            var categoryController = new CategoryController(dbContext);
            categoryController.ControllerContext = new ControllerContext();
            categoryController.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act

            var result = await categoryController.DeleteCategory(category.Id);
            var okResult = result as OkObjectResult;

            // Assert

            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
            Assert.Equal("Deleted Category!", (string)okResult.Value);


        }

        [Fact]
        public async Task DeleteCategory_NonExistingId_ShouldReturnNotFound()
        {
            // Arrange

            var id = await dbContext.Categories.MaxAsync(x => x.Id);

            var categoryController = new CategoryController(dbContext);
            categoryController.ControllerContext = new ControllerContext();
            categoryController.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act

            var result = await categoryController.DeleteCategory(id + 1);
            var objectResult = result as NotFoundResult;

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
        }
    }
}
