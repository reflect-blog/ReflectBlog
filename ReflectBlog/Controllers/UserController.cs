﻿using ReflectBlog.Data;
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
                var identity = HttpContext.User.Identity as ClaimsIdentity;
                var currentUser = HelperMethods.GetCurrentUser(identity);

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
            catch (Exception ex)
            {
                //return BadRequest(ex.Message);
                return BadRequest();
            }
        }

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser(User userModel)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var currentUser = HelperMethods.GetCurrentUser(identity);

            if (identity.Claims.FirstOrDefault(x => x.Type == "UserId").Value != userModel.Id.ToString() && identity.Claims.FirstOrDefault(x => x.Type == "Role").Value != "Administrator")
                return Unauthorized();

            var userToUpdate = _dbContext.Update(userModel);
            await _dbContext.SaveChangesAsync();

            return Ok(userToUpdate.Entity);
        }

        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUser([Required] int id)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var currentUser = HelperMethods.GetCurrentUser(identity);

            if (identity.Claims.FirstOrDefault(x => x.Type == "UserId").Value != id.ToString() && identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value != "Administrator")
                return Unauthorized();
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

        //private User GetCurrentUser()
        //{
        //    var identity = HttpContext.User.Identity as ClaimsIdentity;

        //    if (identity != null)
        //    {
        //        var userClaims = identity.Claims;
        //        int.TryParse(userClaims.FirstOrDefault(o => o.Type == "UserId")?.Value, out int userId);

        //        return new User
        //        {
        //            Id = userId,
        //            Username = userClaims.FirstOrDefault(o => o.Type == "UserName")?.Value,
        //            Email = userClaims.FirstOrDefault(o => o.Type == "Email")?.Value,
        //            GivenName = userClaims.FirstOrDefault(o => o.Type == "GivenName")?.Value,
        //            FamilyName = userClaims.FirstOrDefault(o => o.Type == "FamilyName")?.Value,
        //            Role = userClaims.FirstOrDefault(o => o.Type == "Role")?.Value
        //        };
        //    }
        //    return null;
        //}

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
