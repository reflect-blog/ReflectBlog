using ReflectBlog.Data;
using ReflectBlog.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Blog.UnitTests.MockData
{
    public static class Data
    {
        public static void Seed(BlogDbContext dbContext)
        {

            List<User> users = new List<User>()
            {
                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = "Administrator",
                    GivenName = Faker.Name.First(),
                    FamilyName = Faker.Name.Last()
                },

                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = "Administrator",
                    GivenName = Faker.Name.First(),
                    FamilyName = Faker.Name.Last()
                },
                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = "Administrator",
                    GivenName = Faker.Name.First(),
                    FamilyName = Faker.Name.Last()
                },                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = "Administrator",
                    GivenName = Faker.Name.First(),
                    FamilyName = Faker.Name.Last()
                },                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = "Administrator",
                    GivenName = Faker.Name.First(),
                    FamilyName = Faker.Name.Last()
                },                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = "Administrator",
                    GivenName = Faker.Name.First(),
                    FamilyName = Faker.Name.Last()
                },                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = "Administrator",
                    GivenName = Faker.Name.First(),
                    FamilyName = Faker.Name.Last()
                },                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = "Administrator",
                    GivenName = Faker.Name.First(),
                    FamilyName = Faker.Name.Last()
                },                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = "Administrator",
                    GivenName = Faker.Name.First(),
                    FamilyName = Faker.Name.Last()
                },                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = "Administrator",
                    GivenName = Faker.Name.First(),
                    FamilyName = Faker.Name.Last()
                },                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = "Administrator",
                    GivenName = Faker.Name.First(),
                    FamilyName = Faker.Name.Last()
                },                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = "Administrator",
                    GivenName = Faker.Name.First(),
                    FamilyName = Faker.Name.Last()
                },                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = "Administrator",
                    GivenName = Faker.Name.First(),
                    FamilyName = Faker.Name.Last()
                },                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = "Administrator",
                    GivenName = Faker.Name.First(),
                    FamilyName = Faker.Name.Last()
                },                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = "Administrator",
                    GivenName = Faker.Name.First(),
                    FamilyName = Faker.Name.Last()
                },                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = "Administrator",
                    GivenName = Faker.Name.First(),
                    FamilyName = Faker.Name.Last()
                },                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = "Administrator",
                    GivenName = Faker.Name.First(),
                    FamilyName = Faker.Name.Last()
                },                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = "Administrator",
                    GivenName = Faker.Name.First(),
                    FamilyName = Faker.Name.Last()
                },                new User
                {
                    Username = Faker.Name.First(),
                    Salt = Faker.Lorem.Words(1).First(),
                    Password = Faker.Lorem.Words(1).First(),
                    Email = Faker.Internet.Email(),
                    Role = "Administrator",
                    GivenName = Faker.Name.First(),
                    FamilyName = Faker.Name.Last()
                },
            };
            
            List<Article> articles = new List<Article>()
            {
                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1
                },

                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1
                },
                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1
                },new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1
                },                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1
                },                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1
                },                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1
                },                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1
                },                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1
                },                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1
                },                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1
                },                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1
                },                new Article
                {
                   Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1
                },                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1
                },                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1
                },                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1
                },                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1
                },                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1
                },                new Article
                {
                    Title = Faker.Name.First(),
                    Content = Faker.Lorem.Words(1).First(),
                    AuthorId = 1
                },
            };

            List<Category> categories = new List<Category>()
            {
                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                    
                },

                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                    
                },
                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                    
                },new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                    
                },                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                    
                },                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                    
                },                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                    
                },                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                    
                },                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                    
                },                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                    
                },                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                    
                },                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                    
                },                new Category
                {
                   Name = Faker.Name.First(),
                    IsDefault = true
                    
                },                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                    
                },                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                    
                },                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                    
                },                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                    
                },                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                    
                },                new Category
                {
                    Name = Faker.Name.First(),
                    IsDefault = true
                    
                },
            };

            if(!dbContext.Users.Any())
                dbContext.Users.AddRange(users);

            if (!dbContext.Articles.Any())
                dbContext.Articles.AddRange(articles);

            if (!dbContext.Categories.Any())
                dbContext.Categories.AddRange(categories);

            dbContext.SaveChanges();

        }
    }
}
