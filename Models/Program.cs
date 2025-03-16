using EliteTGTask.Models;
using Microsoft.AspNetCore.Identity;

public class Programs
{
    public static void Main(string[] args)
    {
        var passwordHasher = new PasswordHasher<ApplicationUser>();
        var hashedPassword = passwordHasher.HashPassword(null, "admin1234");
        Console.WriteLine("Hashed Password: " + hashedPassword);
    }
}