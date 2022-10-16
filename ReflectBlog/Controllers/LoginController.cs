using ReflectBlog.Data;
using ReflectBlog.Entities;
using ReflectBlog.Helpers;
using ReflectBlog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace ReflectBlog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LoginController : ControllerBase
    {
        private IConfiguration _config;
        private readonly BlogDbContext _dbContext;

        public LoginController(IConfiguration config, BlogDbContext dbContext)
        {
            _config = config;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Login User Endpoint
        /// </summary>
        /// <param name="userLogin">Login model containing username and password parameters</param>
        /// <returns>User Token</returns>
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] UserLogin userLogin)
        {
            var user = Authenticate(userLogin);

            if (user != null)
            {
                var token = Generate(user);
                return Ok(token);
            }

            return NotFound("User not found");
        }

        /// <summary>
        /// Endpoint to generate jwt token for given user
        /// </summary>
        /// <param name="user">User date based on which the token will be created</param>
        /// <returns>Generated Token</returns>
        private string Generate(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim("UserName", user.Username),
                new Claim("Email", user.Email),
                new Claim("GivenName", user.GivenName),
                new Claim("FamilyName", user.FamilyName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Audience"],
              claims,
              expires: DateTime.Now.AddHours(2),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Authenticate user method
        /// </summary>
        /// <param name="userLogin">Login model containing username and password parameters</param>
        /// <returns>Current user if exists and provided correct credentials</returns>
        private User Authenticate(UserLogin userLogin)
        {

            var currentUser = _dbContext.Users.FirstOrDefault(o => o.Username.ToLower() == userLogin.Username);

            if (currentUser != null)
            {
                var correctPassword = (HelperMethods.CreateMD5(currentUser.Salt + userLogin.Password) == currentUser.Password);

                if (correctPassword)
                    return currentUser;
            }

            return null;
        }
    }
}
