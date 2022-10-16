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
        /// Endpoint to get paginated data for users
        /// </summary>
        /// <param name="search">Search keyword</param>
        /// <param name="page">Page Number</param>
        /// <param name="pageSize">Page Size</param>
        /// <returns>Users Paginated</returns>
        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers(string search, int page = 1, int pageSize = 10)
        {
            Expression<Func<User, bool>> searchCondition = x => x.GivenName.Contains(search) || x.FamilyName.Contains(search) || x.Email.Contains(search);

            var users = await _dbContext.Users.WhereIf(!string.IsNullOrEmpty(search), searchCondition)
                                                   .OrderBy(x => x.Id)
                                                   .Skip((page - 1) * pageSize).Take(pageSize)
                                                   .ToListAsync();

            var usersPaged = new PagedInfo<User>
            {
                Data = users,
                TotalCount = await _dbContext.Users.CountAsync(),
                PageSize = pageSize,
                Page = page
            };

            return Ok(usersPaged);
        }

        /// <summary>
        /// Endpoint to get user by id
        /// </summary>
        /// <param name="id">Id of user to get</param>
        /// <returns>User if found</returns>
        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser([Required] int id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Endpoint to create a new user
        /// </summary>
        /// <param name="userModel">Model with parameters required to create an user</param>
        /// <returns>Newly created user</returns>
        [AllowAnonymous]
        [HttpPost("PostUser")]
        public async Task<IActionResult> PostUser(UserModel userModel)
        {
            try
            {
                var currentUser = GetCurrentUser();

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
            catch(Exception ex)
            {
                //return BadRequest(ex.Message);
                return BadRequest();
            }
        }

        /// <summary>
        /// Method to update an user 
        /// </summary>
        /// <param name="userModel">Model with parameters required to update an user</param>
        /// <returns>Updated User</returns>
        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser(User userModel)
        {
            try
            {
                var userToUpdate = _dbContext.Update(userModel);
                await _dbContext.SaveChangesAsync();

                return Ok(userToUpdate.Entity);
            }
            catch
            {
                return BadRequest();
            }

             
        }

        /// <summary>
        /// Method to delete a user based on id
        /// </summary>
        /// <param name="id">Id of user to be deleted</param>
        /// <returns>Deleted confirmation</returns>
        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser([Required] int id)
        {
            var userToDelete = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (userToDelete == null)
                return NotFound();

            _dbContext.Remove(userToDelete);
            await _dbContext.SaveChangesAsync();

            return Ok("Deleted User!");
        }


        /// <summary>
        /// Method to return current(authenticated) user data
        /// </summary>
        /// <returns>Current User Data</returns>
        private User GetCurrentUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            {
                var userClaims = identity.Claims;
                int.TryParse(userClaims.FirstOrDefault(o => o.Type == "UserId")?.Value, out int userId);

                return new User
                {
                    Id = userId,
                    Username = userClaims.FirstOrDefault(o => o.Type == "UserName")?.Value,
                    Email = userClaims.FirstOrDefault(o => o.Type == "Email")?.Value,
                    GivenName = userClaims.FirstOrDefault(o => o.Type == "GivenName")?.Value,
                    FamilyName = userClaims.FirstOrDefault(o => o.Type == "FamilyName")?.Value,
                    Role = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value
                };
            }
            return null;
        }

    }
}
