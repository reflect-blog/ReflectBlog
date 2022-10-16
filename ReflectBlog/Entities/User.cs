using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReflectBlog.Entities
{
    public class User
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        public string Salt { get; set; }
        [Required]
        public string Password { get; set; }
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        public string Role { get; set; }
        [Required]
        public string GivenName { get; set; }
        [Required]
        public string FamilyName { get; set; }

        public ICollection<Article> Articles { get; set; }
    }
}
