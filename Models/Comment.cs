using System.ComponentModel.DataAnnotations;

namespace EliteTGTask.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Text { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; } 

        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public int postId { get; set; }
        public Post Post { get; set; }
        public Comment()
        {
            CreatedAt = DateTime.Now;
        }

    }
}
