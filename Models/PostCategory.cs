namespace EliteTGTask.Models
{


    public class PostCategory
    {
        public int PostId { get; set; }
        public virtual Post Post { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
