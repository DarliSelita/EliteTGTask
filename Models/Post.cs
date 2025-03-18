namespace EliteTGTask.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text.Json.Serialization;

    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; } = string.Empty;

        [JsonIgnore] // Prevents serialization issues
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<PostCategory> PostCategories { get; set; } = new List<PostCategory>();

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public Post()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
