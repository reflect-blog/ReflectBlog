using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ReflectBlog.Entities;

namespace ReflectBlog.Data.Configurations
{
    public class ArticleConfiguration : IEntityTypeConfiguration<Article>
    {
        public void Configure(EntityTypeBuilder<Article> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.User)
                .OnDelete(DeleteBehavior.Cascade);

            builder.ToTable(name: "Articles");
        }
    }
}
