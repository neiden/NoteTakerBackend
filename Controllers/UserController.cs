using TokenTest.Services;

using TokenTest.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace TokenTest.Controllers;
[Authorize]
[ApiController]
[RequireHttps]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly IConfiguration _configuration;

    public UserController(UserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login login)
    {
        // Retrieve the user from the database
        Log.Information("Controller called: Logging in user with login {0}", login.Username);
        var user = await _userService.GetUserByLogin(login.Username);
        if (user == null)
        {
            return Unauthorized("Invalid login");
        }

        // Hash the provided password with the stored salt
        byte[] salt = Convert.FromBase64String(user.PasswordSalt);
        using var hmac = new HMACSHA512(salt);

        byte[] hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(login.Password));
        if (Convert.ToBase64String(hash) != user.Password)
        {
            return Unauthorized("Invalid password");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, user.Id.ToString())
            ]),
            Expires = DateTime.UtcNow.AddHours(2),
            Issuer = _configuration["Jwt:Issuer"],
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

    [HttpGet]
    [AllowAnonymous]
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
            return BadRequest("Username already taken");
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

        user.PasswordSalt = Convert.ToBase64String(salt);
        user.Password = Convert.ToBase64String(hash);


        await _userService.CreateUser(user);
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }
}