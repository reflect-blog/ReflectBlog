using System.Collections.Generic;

namespace ReflectBlog.Models
{
    public class PagedInfo<T> where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        public List<T> Data { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}
