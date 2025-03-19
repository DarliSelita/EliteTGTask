namespace EliteTGTask.Models
{ 
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;


    public class ApplicationUser : IdentityUser
    {
        public String FullName { get; set; }

        // Perdorim keyword-in Virtual pasi na ndihmon ne eficencen e loading te te dhenave per nje role user te blogut, si dhe nuk perdorim .Include() pa arsye
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }

}