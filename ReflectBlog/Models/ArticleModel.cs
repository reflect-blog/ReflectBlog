using ReflectBlog.Entities;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ReflectBlog.Models
{
    public class ArticleModel
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public int? AuthorId { get; set; }
        public int? CategoryId { get; set; }
        public IFormFile Image { get; set; }
    }
}
