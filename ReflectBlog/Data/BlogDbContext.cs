using ReflectBlog.Entities;
using Microsoft.EntityFrameworkCore;

namespace ReflectBlog.Data
{
    public class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options)
            : base(options) { }

        public DbSet<Article> Articles => Set<Article>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.ApplyConfiguration(new UserConfiguration());
            //modelBuilder.ApplyConfiguration(new ArticleConfiguration());

            modelBuilder.Entity<User>()
                        .HasMany(c => c.Articles)
                        .WithOne(e => e.User)
                        .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

            modelBuilder.Entity<Article>()
                        .HasOne(e => e.User)
                        .WithMany(c => c.Articles);

            modelBuilder.Entity<Category>()
                        .HasMany(c => c.Articles)
                        .WithOne(e => e.Category)
                        .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Category>().HasIndex(c => c.Name).IsUnique();
        }
    }
}
