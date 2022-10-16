using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReflectBlog.Entities
{
    public class Category
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public bool IsDefault { get; set; }

        public ICollection<Article> Articles { get; set; }
    }
}
