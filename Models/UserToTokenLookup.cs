
namespace TokenTest.Models;
public class UserToTokenLookup
{
    public int Id { get; set; }
    public required int UserId { get; set; }
    public string? Email { get; set; }
    public string? VerifyEmailToken { get; set; }
    public string? ResetPasswordToken { get; set; }

}