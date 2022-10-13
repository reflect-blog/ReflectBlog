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
    public class UserControllerTests
    {
        private readonly BlogDbContext dbContext;
        private readonly DbContextOptions<BlogDbContext> dbContextOptions;
        public UserControllerTests()
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
        public async Task GetUsers_WithSearch_ShouldReturn_OkResponse(string search)
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

            var userController = new UserController(dbContext);
            userController.ControllerContext = new ControllerContext();
            userController.ControllerContext.HttpContext = new DefaultHttpContext();
            userController.ControllerContext.HttpContext.User = claimsPrincipal;

            dbContext.Users.AddRange(new List<User>
            {
                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = Faker.Name.First(),
                    GivenName = Faker.Lorem.Words(1).First() + search + Faker.Lorem.Words(1).First(),
                    FamilyName = Faker.Name.Last()
                },
                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = Faker.Name.First(),
                    GivenName = search + " " + Faker.Lorem.Words(1).First(),
                    FamilyName = Faker.Name.Last()
                },
                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = Faker.Name.First(),
                    GivenName =  Faker.Lorem.Words(2).First() + " " + search,
                    FamilyName = Faker.Name.Last()
                }
            }
            );

            await dbContext.SaveChangesAsync();

            // Act
            // Here we act on the unit of code that we want to test

            var response = await userController.GetUsers(search);
            var okResult = response as OkObjectResult;

            // Assert
            // Here we assert the results

            int totalCountInDb = await dbContext.Users.CountAsync();
            int expectedDataCount = totalCountInDb >= 10 ? 10 : totalCountInDb;

            if (!string.IsNullOrEmpty(search))
            {
                expectedDataCount = await dbContext.Users.Where(x => x.GivenName.Contains(search) || x.FamilyName.Contains(search) || x.Email.Contains(search)).CountAsync();
            }

            Assert.NotNull(okResult);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);

            var data = okResult.Value as PagedInfo<User>;

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
        public async Task GetUsers_WithPageParameters_ShouldReturn_PagedResponse(int page, int pageSize)
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

            var userController = new UserController(dbContext);
            userController.ControllerContext = new ControllerContext();
            userController.ControllerContext.HttpContext = new DefaultHttpContext();
            userController.ControllerContext.HttpContext.User = claimsPrincipal;

            // Act

            var response = await userController.GetUsers(null, page, pageSize);
            var okResult = response as OkObjectResult;

            // Assert

            var totalCountInDb = await dbContext.Users.CountAsync();
            var totalItemsPerPage = await dbContext.Users.Skip((page - 1) * pageSize).Take(pageSize).CountAsync();

            Assert.NotNull(okResult);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);

            var data = okResult.Value as PagedInfo<User>;

            Assert.NotNull(data.Data);
            Assert.Equal(totalItemsPerPage, data.Data.Count);
            Assert.Equal(totalCountInDb, data.TotalCount);
            Assert.Equal(data.Page, page);
            Assert.Equal(data.PageSize, pageSize);
        }

        [Fact]
        public async Task GetUser_WithExistingId_ShouldReturn_Success()
        {
            // Arrange

            var expectedUser = await dbContext.Users.FirstOrDefaultAsync();
            var userController = new UserController(dbContext);

            // Act

            var response = await userController.GetUser(expectedUser.Id);
            var okResult = response as OkObjectResult;

            // Assert

            var actualUser = okResult.Value as User;

            Assert.NotNull(actualUser);
            Assert.Equal(expectedUser.Id, actualUser.Id);
            Assert.Equal(expectedUser.Email, actualUser.Email);
        }

        [Fact]

        public async Task GetUser_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange

            var id = await dbContext.Users.MaxAsync(x => x.Id);
            var userController = new UserController(dbContext);


            // Act

            var response = await userController.GetUser(id + 1);
            var result = response as NotFoundResult;

            // Assert

            Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task PostUser_ValidUserModel_ShouldReturnCreated()
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

            var userController = new UserController(dbContext);
            userController.ControllerContext = new ControllerContext();
            userController.ControllerContext.HttpContext = new DefaultHttpContext();
            userController.ControllerContext.HttpContext.User = claimsPrincipal;

            var userToCreate = new UserModel
            {
                Username = Faker.Name.First(),
                //Salt = Faker.Lorem.Words(1).First(),
                Password = Faker.Lorem.Words(1).First(),
                Email = Faker.Internet.Email(),
                //Role = Faker.Name.First(),
                GivenName = Faker.Lorem.Words(1).First(),
                FamilyName = Faker.Name.Last()
            };

            // Act

            var response = await userController.PostUser(userToCreate);
            var okResult = response as ObjectResult;

            // Assert

            Assert.NotNull(okResult);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
            //Assert.Equal((int)HttpStatusCode.Created, okResult.StatusCode);

            var actualUser = okResult.Value as User;
            Assert.NotNull(actualUser);

            Assert.Equal(userToCreate.Username, actualUser.Username);
            //Assert.Equal(userToCreate.Salt, actualUser.Salt);
            //Assert.Equal(userToCreate.Password, actualUser.Password); // Salt and Assert
            Assert.Equal(userToCreate.Email, actualUser.Email);
            //Assert.Equal(userToCreate.Role, actualUser.Role);
            Assert.Equal(userToCreate.GivenName, actualUser.GivenName);
            Assert.Equal(userToCreate.FamilyName, actualUser.FamilyName);
        }

        [Fact]
        public async Task PostUser_MissingRequiredFields_ShouldReturn_BadRequest()
        {
            // Arrange

            var userController = new UserController(dbContext);
            userController.ControllerContext = new ControllerContext();
            userController.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act
            var result = await userController.PostUser(new UserModel
            {
                Username = Faker.Name.First(),
                Password = Faker.Lorem.Words(1).First()
            });

            var objectRes = result as BadRequestResult;

            // Assert

            Assert.Equal((int)HttpStatusCode.BadRequest, objectRes.StatusCode);
            //Assert.Null(objectRes.Value);
        }

        [Fact]
        public async Task PostUser_NullParameter_ShouldReturnBadRequest()
        {
            // Arrange

            var userController = new UserController(dbContext);
            userController.ControllerContext = new ControllerContext();
            userController.ControllerContext.HttpContext = new DefaultHttpContext();

            // Act

            var result = await userController.PostUser(null);
            var objectRes = result as BadRequestResult;

            // Assert

            Assert.Equal((int)HttpStatusCode.BadRequest, objectRes.StatusCode);
            //Assert.Null(objectRes.Value);
        }

        [Fact]
        public async Task DeleteUser_ExistingUse_ShouldDeleteSuccessfully()
        {
            // Arrange

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

            var userController = new UserController(dbContext);
            userController.ControllerContext = new ControllerContext();
            userController.ControllerContext.HttpContext = new DefaultHttpContext();
            userController.ControllerContext.HttpContext.User = claimsPrincipal;

            // Act

            var result = await userController.DeleteUser(user.Id);
            var okResult = result as OkObjectResult;

            // Assert

            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
            Assert.Equal("Deleted User!", (string)okResult.Value);


        }

        [Fact]
        public async Task DeleteUser_NonExistingId_ShouldReturnNotFound()
        {
            // Arrange

            var id = await dbContext.Users.MaxAsync(x => x.Id);

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

            var userController = new UserController(dbContext);
            userController.ControllerContext = new ControllerContext();
            userController.ControllerContext.HttpContext = new DefaultHttpContext();
            userController.ControllerContext.HttpContext.User = claimsPrincipal; ;

            // Act

            var result = await userController.DeleteUser(id + 1);
            var objectResult = result as NotFoundResult;

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, objectResult.StatusCode);
        }


        #region Private Methods

        //private void Seed()
        //{

        //    List<User> users = new List<User>()
        //    {
        //        new User
        //        {
        //            Username = Faker.Name.First(),
        //            Salt = Faker.Lorem.Words(1).First(),
        //            Password = Faker.Lorem.Words(1).First(),
        //            Email = Faker.Internet.Email(),
        //            Role = Faker.Name.First(),
        //            GivenName = Faker.Name.First(),
        //            FamilyName = Faker.Name.Last()
        //        },

        //        new User
        //        {
        //            Username = Faker.Name.First(),
        //            Salt = Faker.Lorem.Words(1).First(),
        //            Password = Faker.Lorem.Words(1).First(),
        //            Email = Faker.Internet.Email(),
        //            Role = Faker.Name.First(),
        //            GivenName = Faker.Name.First(),
        //            FamilyName = Faker.Name.Last()
        //        },
        //        new User
        //        {
        //            Username = Faker.Name.First(),
        //            Salt = Faker.Lorem.Words(1).First(),
        //            Password = Faker.Lorem.Words(1).First(),
        //            Email = Faker.Internet.Email(),
        //            Role = Faker.Name.First(),
        //            GivenName = Faker.Name.First(),
        //            FamilyName = Faker.Name.Last()
        //        },                new User
        //        {
        //            Username = Faker.Name.First(),
        //            Salt = Faker.Lorem.Words(1).First(),
        //            Password = Faker.Lorem.Words(1).First(),
        //            Email = Faker.Internet.Email(),
        //            Role = Faker.Name.First(),
        //            GivenName = Faker.Name.First(),
        //            FamilyName = Faker.Name.Last()
        //        },                new User
        //        {
        //            Username = Faker.Name.First(),
        //            Salt = Faker.Lorem.Words(1).First(),
        //            Password = Faker.Lorem.Words(1).First(),
        //            Email = Faker.Internet.Email(),
        //            Role = Faker.Name.First(),
        //            GivenName = Faker.Name.First(),
        //            FamilyName = Faker.Name.Last()
        //        },                new User
        //        {
        //            Username = Faker.Name.First(),
        //            Salt = Faker.Lorem.Words(1).First(),
        //            Password = Faker.Lorem.Words(1).First(),
        //            Email = Faker.Internet.Email(),
        //            Role = Faker.Name.First(),
        //            GivenName = Faker.Name.First(),
        //            FamilyName = Faker.Name.Last()
        //        },                new User
        //        {
        //            Username = Faker.Name.First(),
        //            Salt = Faker.Lorem.Words(1).First(),
        //            Password = Faker.Lorem.Words(1).First(),
        //            Email = Faker.Internet.Email(),
        //            Role = Faker.Name.First(),
        //            GivenName = Faker.Name.First(),
        //            FamilyName = Faker.Name.Last()
        //        },                new User
        //        {
        //            Username = Faker.Name.First(),
        //            Salt = Faker.Lorem.Words(1).First(),
        //            Password = Faker.Lorem.Words(1).First(),
        //            Email = Faker.Internet.Email(),
        //            Role = Faker.Name.First(),
        //            GivenName = Faker.Name.First(),
        //            FamilyName = Faker.Name.Last()
        //        },                new User
        //        {
        //            Username = Faker.Name.First(),
        //            Salt = Faker.Lorem.Words(1).First(),
        //            Password = Faker.Lorem.Words(1).First(),
        //            Email = Faker.Internet.Email(),
        //            Role = Faker.Name.First(),
        //            GivenName = Faker.Name.First(),
        //            FamilyName = Faker.Name.Last()
        //        },                new User
        //        {
        //            Username = Faker.Name.First(),
        //            Salt = Faker.Lorem.Words(1).First(),
        //            Password = Faker.Lorem.Words(1).First(),
        //            Email = Faker.Internet.Email(),
        //            Role = Faker.Name.First(),
        //            GivenName = Faker.Name.First(),
        //            FamilyName = Faker.Name.Last()
        //        },                new User
        //        {
        //            Username = Faker.Name.First(),
        //            Salt = Faker.Lorem.Words(1).First(),
        //            Password = Faker.Lorem.Words(1).First(),
        //            Email = Faker.Internet.Email(),
        //            Role = Faker.Name.First(),
        //            GivenName = Faker.Name.First(),
        //            FamilyName = Faker.Name.Last()
        //        },                new User
        //        {
        //            Username = Faker.Name.First(),
        //            Salt = Faker.Lorem.Words(1).First(),
        //            Password = Faker.Lorem.Words(1).First(),
        //            Email = Faker.Internet.Email(),
        //            Role = Faker.Name.First(),
        //            GivenName = Faker.Name.First(),
        //            FamilyName = Faker.Name.Last()
        //        },                new User
        //        {
        //            Username = Faker.Name.First(),
        //            Salt = Faker.Lorem.Words(1).First(),
        //            Password = Faker.Lorem.Words(1).First(),
        //            Email = Faker.Internet.Email(),
        //            Role = Faker.Name.First(),
        //            GivenName = Faker.Name.First(),
        //            FamilyName = Faker.Name.Last()
        //        },                new User
        //        {
        //            Username = Faker.Name.First(),
        //            Salt = Faker.Lorem.Words(1).First(),
        //            Password = Faker.Lorem.Words(1).First(),
        //            Email = Faker.Internet.Email(),
        //            Role = Faker.Name.First(),
        //            GivenName = Faker.Name.First(),
        //            FamilyName = Faker.Name.Last()
        //        },                new User
        //        {
        //            Username = Faker.Name.First(),
        //            Salt = Faker.Lorem.Words(1).First(),
        //            Password = Faker.Lorem.Words(1).First(),
        //            Email = Faker.Internet.Email(),
        //            Role = Faker.Name.First(),
        //            GivenName = Faker.Name.First(),
        //            FamilyName = Faker.Name.Last()
        //        },                new User
        //        {
        //            Username = Faker.Name.First(),
        //            Salt = Faker.Lorem.Words(1).First(),
        //            Password = Faker.Lorem.Words(1).First(),
        //            Email = Faker.Internet.Email(),
        //            Role = Faker.Name.First(),
        //            GivenName = Faker.Name.First(),
        //            FamilyName = Faker.Name.Last()
        //        },                new User
        //        {
        //            Username = Faker.Name.First(),
        //            Salt = Faker.Lorem.Words(1).First(),
        //            Password = Faker.Lorem.Words(1).First(),
        //            Email = Faker.Internet.Email(),
        //            Role = Faker.Name.First(),
        //            GivenName = Faker.Name.First(),
        //            FamilyName = Faker.Name.Last()
        //        },                new User
        //        {
        //            Username = Faker.Name.First(),
        //            Salt = Faker.Lorem.Words(1).First(),
        //            Password = Faker.Lorem.Words(1).First(),
        //            Email = Faker.Internet.Email(),
        //            Role = Faker.Name.First(),
        //            GivenName = Faker.Name.First(),
        //            FamilyName = Faker.Name.Last()
        //        },                new User
        //        {
        //            Username = Faker.Name.First(),
        //            Salt = Faker.Lorem.Words(1).First(),
        //            Password = Faker.Lorem.Words(1).First(),
        //            Email = Faker.Internet.Email(),
        //            Role = Faker.Name.First(),
        //            GivenName = Faker.Name.First(),
        //            FamilyName = Faker.Name.Last()
        //        },
        //    };

        //    dbContext.Users.AddRange(users);
        //    dbContext.SaveChanges();

        //}



        #endregion

    }
}
