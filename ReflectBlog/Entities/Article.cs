using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReflectBlog.Entities
{
    public class Article
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public int? AuthorId { get; set; }
        public int? CategoryId { get; set; }

        [ForeignKey("AuthorId")]
        public User? User { get; set; }

        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }
    }
}
