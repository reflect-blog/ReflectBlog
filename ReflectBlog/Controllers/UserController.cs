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
using System.Security.Cryptography;
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

        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers(string search, int page = 1, int pageSize = 10)
        {

            var currentUser = GetCurrentUser();

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

        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser([Required] int id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }

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
                    FamilyName = userModel.FamilyName
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

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser(User userModel)
        {
            var userToUpdate = _dbContext.Update(userModel);
            await _dbContext.SaveChangesAsync();

            return Ok(userToUpdate.Entity);
        }

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

        [HttpGet("Admins")]
        [Authorize(Roles = "Administrator")]
        public IActionResult AdminsEndpoint()
        {
            var currentUser = GetCurrentUser();

            return Ok($"Hi {currentUser.GivenName}, you are an {currentUser.Role}");
        }


        [HttpGet("Editors")]
        [Authorize(Roles = "Editor")]
        public IActionResult EditorsEndpoint()
        {
            var currentUser = GetCurrentUser();

            return Ok($"Hi {currentUser.GivenName}, you are a {currentUser.Role}");
        }

        [HttpGet("AdminsAndEditors")]
        [Authorize(Roles = "Administrator,Editor")]
        public IActionResult AdminsAndEditorsEndpoint()
        {
            var currentUser = GetCurrentUser();

            return Ok($"Hi {currentUser.GivenName}, you are an {currentUser.Role}");
        }

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

        //[NonAction]
        //public string CreateMD5(string input)
        //{
        //    using (MD5 md5 = MD5.Create())
        //    {
        //        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        //        byte[] hashBytes = md5.ComputeHash(inputBytes);

        //        return Convert.ToHexString(hashBytes).ToLower();
        //    }
        //}
    }
}
