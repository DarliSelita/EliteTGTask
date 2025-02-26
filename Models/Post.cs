namespace EliteTGTask.Models
{
    using Microsoft.Identity.Client;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    public class Post
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Required]
        public string ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<PostCategory> PostCategories { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public Post ()
        {
            CreatedAt = DateTime.Now;
        }

    }
}
