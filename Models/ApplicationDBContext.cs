namespace EliteTGTask.Models
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using System.Reflection.Emit;

    public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
    {

        // Remember Liskov Rule in SOLID, always use bigger instances of deriving classes, not of the class thats deriving
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options) { }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<PostCategory> PostCategories { get; set; }
        public DbSet<Comment> Comments { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Call the function base in order to configure Identity tables, REMEMBER SI TEK KONSTRUKTORI

            // Krijon nje many to many relationship per klasen e Post Category ( 1 postim mund te jete pjese e shume kategorive, si dhe cdo kategori mund te kete shume postime)
            builder.Entity<PostCategory>()
                .HasKey(pc => new { pc.PostId, pc.CategoryId }); // Celes kryesor per te identifikuar ne menyr unike 1 post dhe 1 kategori 

            builder.Entity<PostCategory>()
                .HasOne(pc => pc.Post)
                .WithMany(p => p.PostCategories)
                .HasForeignKey(pc => pc.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PostCategory>()
                .HasOne(pc => pc.Category)
                .WithMany()
                .HasForeignKey(pc => pc.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.postId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed rolet e kerkuara ne task: Admin, Editor dhe Member
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "2", Name = "Editor", NormalizedName = "EDITOR" },
                new IdentityRole { Id = "3", Name = "Member", NormalizedName = "MEMBER" }
            );

            builder.Entity<ApplicationUser>().HasData(
                 new ApplicationUser
                 {
                     Id = "admin-id",
                     UserName = "admin",
                     NormalizedUserName = "ADMIN",
                     Email = "admin@blog.com",
                     NormalizedEmail = "ADMIN@BLOG.COM",
                     EmailConfirmed = true,
                     PasswordHash = "AQAAAAIAAYagAAAAEDn9Alfb2+L1ugcsgxY...",
                     SecurityStamp = "12345",
                     ConcurrencyStamp = "static-concurrency-stamp",
                     FullName = "Admin",
                 }

                 );
         
            // I bejme assign seeded account rolin e adminit *REMEMBER DARL*
                     builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { UserId = "admin-id", RoleId = "1" }

                );

            // Gjithashtu bejme seed kategorite e kerkuara ne task
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Nature" },
                new Category { Id = 2, Name = "Sport" },
                new Category { Id = 3, Name = "Politics" },
                new Category { Id = 4, Name = "Economy" },
                new Category { Id = 5, Name = "Art" },
                new Category { Id = 6, Name = "Science" },
                new Category { Id = 7, Name = "Entertainment" },
                new Category { Id = 8, Name = "News" },
                new Category { Id = 9, Name = "WAr" },
                new Category { Id = 10, Name = "Showbiz" }

               );

        }
    }
}
