namespace TokenTest.Models;
public class User
{
    public int Id { get; set; }
    public required string Login { get; set; }
    public required string Password { get; set; }
    public required string PasswordSalt { get; set; }
    public string? Email { get; set; }
}