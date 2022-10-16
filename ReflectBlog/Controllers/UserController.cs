using ReflectBlog.Data;
using ReflectBlog.Entities;
using ReflectBlog.Helpers;
using ReflectBlog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ReflectBlog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly BlogDbContext _dbContext;

        public UserController(BlogDbContext dbContext)
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
        [HttpGet("GetUsers")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetUsers(string search, int page = 1, int pageSize = 10)
        {
            Expression<Func<User, bool>> searchCondition = x => x.GivenName.Contains(search) || x.FamilyName.Contains(search) || x.Email.Contains(search);

            var users = await _dbContext.Users.WhereIf(!string.IsNullOrEmpty(search), searchCondition)
                                                   .OrderBy(x => x.Id)
                                                   .Skip((page - 1) * pageSize).Take(pageSize)
                                                   .ToListAsync();

            var UsersPaged = new PagedInfo<User>
            {
                Data = users,
                TotalCount = await _dbContext.Users.CountAsync(),
                PageSize = pageSize,
                Page = page
            };

            return Ok(UsersPaged);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser([Required] int id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("PostUser")]
        public async Task<IActionResult> PostUser(UserModel userModel)
        {
            try
            {
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                var currentUser = HelperMethods.GetCurrentUser(identity);

                var user = new User
                {
                    Username = userModel.Username,
                    Email = userModel.Email,
                    Password = userModel.Password,
                    GivenName = userModel.GivenName,
                    FamilyName = userModel.FamilyName,
                    Role = "User"
                };

                user.Salt = Guid.NewGuid().ToString();
                user.Password = HelperMethods.CreateMD5(user.Salt + userModel.Password);
                var userToAdd = await _dbContext.AddAsync(user);
                await _dbContext.SaveChangesAsync();

                return Ok(userToAdd.Entity);
            }
            catch (Exception ex)
            {
                //return BadRequest(ex.Message);
                return BadRequest();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser(User userModel) 
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var currentUser = HelperMethods.GetCurrentUser(identity);

            if (currentUser.Id != userModel.Id && currentUser.Role != "Administrator")
                return Unauthorized();

            var userToUpdate = _dbContext.Update(userModel);
            await _dbContext.SaveChangesAsync();

            return Ok(userToUpdate.Entity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser([Required] int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var currentUser = HelperMethods.GetCurrentUser(identity);

            if (currentUser.Id != id && currentUser.Role != "Administrator")
                return Unauthorized();
            var userToDelete = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (userToDelete == null)
                return NotFound();

            _dbContext.Remove(userToDelete);
            await _dbContext.SaveChangesAsync();

            return Ok("Deleted User!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("Admins")]
        [Authorize(Roles = "Administrator")]
        public IActionResult AdminsEndpoint()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var currentUser = HelperMethods.GetCurrentUser(identity);

            return Ok($"Hi {currentUser.GivenName}, you are an {currentUser.Role}");
        }

        [HttpGet("Authors")]
        [Authorize(Roles = "Author")]
        public IActionResult AuthorsEndpoint()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var currentUser = HelperMethods.GetCurrentUser(identity);

            return Ok($"Hi {currentUser.GivenName}, you are a {currentUser.Role}");
        }

        [HttpGet("AdminsAndAuthors")]
        [Authorize(Roles = "Administrator,Author")]
        public IActionResult AdminsAndAuthorsEndpoint()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var currentUser = HelperMethods.GetCurrentUser(identity);

            return Ok($"Hi {currentUser.GivenName}, you are an {currentUser.Role}");
        }


    }
}
