using TokenTest.Services;

using TokenTest.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Token.Services;
using SendGrid.Helpers.Mail;
using SendGrid;

namespace TokenTest.Controllers;
[Authorize]
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly EmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly Aes _aes;

    public UserController(EmailService emailService, UserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
        _emailService = emailService;
        _aes = Aes.Create();
        _aes.Key = Convert.FromBase64String(_configuration["AESKEY"]);
    }

    [AllowAnonymous]
    [HttpPost("reset-password-email")]
    public async Task<IActionResult> SendResetPasswordEmail([FromBody] Login login, string token)
    {
        Log.Information("Controller called: Send Reset Password Email");
        var user = await _userService.GetUserByLogin(login.Username);
        var to = new EmailAddress(user.Email, user.Login);
        string subject = "Reset your password";
        string content = "Please click the link below to reset your password";
        string htmlContent = $"<strong>Please click the link below to reset your password</strong> <br> <a href='http://localhost:4200/reset-password?token={token}'>Reset Password</a>";
        var status = await _emailService.SendEmail(to, content, htmlContent, subject);
        if (!status)
        {
            return BadRequest("Email failed to send");
        }
        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("authentication-email")]
    public async Task<IActionResult> SendAuthenticationEmail([FromBody] Login login, string validationToken)
    {
        Log.Information("Controller called: Send Authentication Email");
        var user = await _userService.GetUserByLogin(login.Username);
        var to = new EmailAddress(user.Email, user.Login);
        string subject = "Verify your email";
        string content = "Please click the link below to verify your email address";
        string htmlContent = $"<strong>Please click the link below to verify your email address</strong> <br> <a href='https://localhost:4200/verify-email?token={validationToken}'>Verify Email</a>";
        var status = await _emailService.SendEmail(to, content, htmlContent, subject);
        if (!status)
        {
            return BadRequest("Email failed to send");
        }
        return Ok();
    }


    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login login)
    {
        Log.Information("Controller called: Logging in user with login {0}", login.Username);
        // Retrieve the user from the database
        var user = await _userService.GetUserByLogin(login.Username);
        if (user == null)
        {
            Log.Information("Controller: User {0} not found", login.Username);
            return Unauthorized(new { message = "Invalid login" });
        }
        if (!user.Authenticated)
        {
            Log.Information("Controller: User {0} not verified", login.Username);
            return Unauthorized(new { message = "Email not verified" });
        }

        // Hash the provided password with the stored salt
        byte[] salt = Convert.FromBase64String(user.PasswordSalt);
        using var hmac = new HMACSHA512(salt);

        byte[] hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(login.Password));
        if (Convert.ToBase64String(hash) != user.Password)
        {
            Log.Information("Controller: User {0} entered invalid password", login.Username);
            return Unauthorized(new { message = "Invalid password" });
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JWT"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, user.Id.ToString())
            ]),
            Expires = DateTime.UtcNow.AddHours(2),
            Issuer = _configuration["JWT_ISSUER"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        Log.Information("Controller: User {0} logged in", login.Username);

        return Ok(new { Token = tokenString, UserId = user.Id });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userService.GetUserById(id);
        {
            if (user == null)
                return NotFound();
        }
        return Ok(user);
    }

    [AllowAnonymous]
    [HttpPost("verify-email/{token}/{userId}")]
    public async Task<IActionResult> VerifyEmail(string token, int userId)
    {
        Log.Information("Controller called: Verifying email with token {0}", token, " and email {1}", userId);

        var result = await _userService.VerifyEmail(token, userId);
        if (!result)
        {
            return BadRequest("Invalid token");
        }
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        Log.Information("Controller called: Getting all users");
        var users = await _userService.GetAllUsers();
        return Ok(users);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        Log.Information("Controller called: Creating user with login {0}", user.Login);
        if (await _userService.GetUserByLogin(user.Login) != null)
        {
            return BadRequest(new { message = "Username already taken" });
        }

        if (!ModelState.IsValid)
        {
            Log.Information("Controller: Model state is invalid");
            return BadRequest(ModelState);
        }


        byte[] salt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        using var hmac = new HMACSHA512(salt);
        byte[] hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(user.Password));

        user.Password = Convert.ToBase64String(hash);


        user.PasswordSalt = Convert.ToBase64String(salt);
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JWT"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, user.Id.ToString())
            ]),
            Expires = DateTime.UtcNow.AddMinutes(30),
            Issuer = _configuration["JWT_ISSUER"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);


        await _userService.CreateUser(user);
        await _userService.CreateUserToEmailToken(user.Id, tokenString);
        var emailResult = await _userService.SendVerificationEmail(user.Id!, tokenString, user.Email!);
        if (!emailResult)
        {
            return BadRequest("Email failed to send");
        }
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    [HttpPost("reset-password/{userId}/{token}/{newPassword}")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(int userId, string token, string newPassword)
    {
        Log.Information("Controller called: Resetting password for user with ID {0}", userId);
        var user = await _userService.GetUserById(userId);
        if (user == null)
        {
            return BadRequest(new { message = "User not found" });
        }

        byte[] salt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        using var hmac = new HMACSHA512(salt);
        byte[] hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(newPassword));

        var passwordSalt = Convert.ToBase64String(salt);
        newPassword = Convert.ToBase64String(hash);

        var result = await _userService.ResetPassword(userId, token, newPassword, passwordSalt);
        if (!result)
        {
            return BadRequest("Invalid token");
        }
        return Ok();
    }

    [HttpPost("reset-password/{email}")]
    [AllowAnonymous]
    public async Task<IActionResult> SendResetPasswordEmail(string email)
    {
        Log.Information("Controller called: Send Reset Password Email");
        var user = await _userService.GetUserByEmail(email);
        if (user == null)
        {
            return BadRequest(new { message = "No account associated with that email found" });
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["JWT"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, user.Id.ToString())
            ]),
            Expires = DateTime.UtcNow.AddMinutes(30),
            Issuer = _configuration["JWT_ISSUER"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        await _userService.CreateUserToEmailResetToken(user.Id, tokenString);
        await _userService.SendResetPasswordEmail(user.Id, tokenString, email);
        return Ok();
    }
}