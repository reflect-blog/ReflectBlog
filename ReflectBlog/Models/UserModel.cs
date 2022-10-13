using System.ComponentModel.DataAnnotations;

namespace ReflectBlog.Models
{
    public class UserModel
    {
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        [Required]
        public string GivenName { get; set; }
        [Required]
        public string FamilyName { get; set; }
    }
}
