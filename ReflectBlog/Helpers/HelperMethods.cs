using ReflectBlog.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Linq.Expressions;
using System.Linq;
using ReflectBlog.Entities;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ReflectBlog.Helpers
{
    public static class HelperMethods
    {
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
        {
            return condition
                ? query.Where(predicate)
                : query;
        }

        public static async Task<string> ImgurImageUpload(IFormFile image)
        {
            string base64Image;
            using (var ms = new MemoryStream())
            {
                image.CopyTo(ms);
                var fileBytes = ms.ToArray();
                base64Image = Convert.ToBase64String(fileBytes);
                // act on the Base64 data
            }

            var httpclient = new HttpClient();
            httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", "c88b8634c974be4");
            httpclient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "text/plain");
            var response = await httpclient.PostAsync("https://api.imgur.com/3/image", new StringContent(base64Image));
            var stringcontent = await response.Content.ReadAsStringAsync();
            var imgurResponseModel = JsonConvert.DeserializeObject<ImgurResponseModel>(stringcontent);
            return imgurResponseModel?.Data?.Link;
        }

        public static User GetCurrentUser(ClaimsIdentity identity)
        {
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

        public static string CreateMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes).ToLower();
            }
        }
    }
}
