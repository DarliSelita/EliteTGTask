namespace EliteTGTask.Models
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public class ApplicationDBContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options, IPasswordHasher<ApplicationUser> passwordHasher)
            : base(options)
        {
            _passwordHasher = passwordHasher;
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<PostCategory> PostCategories { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Many-to-many relationship for PostCategory **cascade ben delete te gjithe rekording nga te gjitha tabelat
            builder.Entity<PostCategory>()
                .HasKey(pc => new { pc.PostId, pc.CategoryId });

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

            // Seed roles
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "2", Name = "Editor", NormalizedName = "EDITOR" },
                new IdentityRole { Id = "3", Name = "Member", NormalizedName = "MEMBER" }
            );
            // Hashi per "admin1234" eshte marre nga 1 hash converter online, ndoshta ka ndonje menyre me te mire, por migrationi ka problem me te dhena qe nuk jane statike, ne kete rast passwordi qe une do krijoja "admin1234"
            var hardcodedHashedPassword = "AQAAAAIAAYagAAAAEL/yTcc+DgnF5qmPg0KIOET1yIr5Uj0CEHzHCDlHK3d+2o/1KA78LcVNcpS3FWBllw==";

            var admin = new ApplicationUser
            {
                Id = "admin-id",
                UserName = "admin",
                NormalizedUserName = "ADMIN",
                Email = "admin@blog.com",
                NormalizedEmail = "ADMIN@BLOG.COM",
                EmailConfirmed = true,
                PasswordHash = hardcodedHashedPassword,
                SecurityStamp = "static-security-stamp",
                ConcurrencyStamp = "static-concurrency-stamp",
                FullName = "Admin",
            };

            builder.Entity<ApplicationUser>().HasData(admin);

            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { UserId = "admin-id", RoleId = "1" }
            );

            
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Nature" },
                new Category { Id = 2, Name = "Sport" },
                new Category { Id = 3, Name = "Politics" },
                new Category { Id = 4, Name = "Economy" },
                new Category { Id = 5, Name = "Art" },
                new Category { Id = 6, Name = "Science" },
                new Category { Id = 7, Name = "Entertainment" },
                new Category { Id = 8, Name = "News" },
                new Category { Id = 9, Name = "War" },
                new Category { Id = 10, Name = "Showbiz" }
            );
        }
    }
}