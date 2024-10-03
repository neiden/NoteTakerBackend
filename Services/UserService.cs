using System.Collections.Generic;
using System.Security.Cryptography;
using TokenTest.Models;
using Token.Data;
using Token.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SendGrid.Helpers.Mail;

namespace TokenTest.Services
{
    public class UserService
    {
        private List<User> users;
        private readonly Aes _aes;
        private readonly IConfiguration _configuration;
        private readonly Token.Data.TokenContext _context;
        private readonly EmailService _emailService;
        public UserService(EmailService emailService, Token.Data.TokenContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _aes = Aes.Create();
            _aes.Key = Convert.FromBase64String(_configuration["AESKEY"]);
        }


        public async Task<bool> SendVerificationEmail(int userId, string token, string email)
        {
            Log.Information("service called: Send Authentication Email with token {0}", token, " and username {1}", email);

            var to = new EmailAddress(email, "User");
            string subject = "Verify your email";
            string content = "Please click the link below to verify your email address";
            string htmlContent = $"<strong>Please click the link below to verify your email address</strong> <br> <a href='https://fluentflow.net/verify-account?token={token}&id={userId}'>Verify Email</a>";
            var status = await _emailService.SendEmail(to, content, htmlContent, subject);
            return status;
        }

        public async Task<bool> SendResetPasswordEmail(int userId, string token, string email)
        {
            Log.Information("Service called: Send Reset Password Email with token {0}", token, " and email {1}", email);
            var to = new EmailAddress(email, "User");
            string subject = "Reset your password";
            string content = "Please click the link below to reset your password";
            string htmlContent = $"<strong>Please click the link below to reset your password</strong> <br> <a href='https://fluentflow.net/reset-password/reset?token={token}&id={userId}'>Reset Password</a>";
            var status = await _emailService.SendEmail(to, content, htmlContent, subject);
            return status;
        }

        public async Task<bool> ResetPassword(int userId, string token, string newPassword, string passwordSalt)
        {
            Log.Information("Service called: Reset Password with userID {0}", userId);
            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == userId);
            if (user == null)
            {
                return false;
            }
            var result = await _context.UserToTokenLookup.FirstOrDefaultAsync(m => m.UserId == user.Id && m.ResetPasswordToken! == token);
            if (result != null)
            {
                user.Password = newPassword;
                user.PasswordSalt = passwordSalt;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            Log.Information("Service called: Getting user by email {0}", email);
            var user = await _context.Users.FirstOrDefaultAsync(m => m.Email == email);
            return user;
        }

        public async Task CreateUserToEmailResetToken(int userId, string token)
        {
            Log.Information("Service called: Creating user to email reset token with userID {0}", userId);
            var userToTokenLookup = new UserToTokenLookup
            {
                UserId = userId,
                ResetPasswordToken = token
            };
            _context.UserToTokenLookup.Add(userToTokenLookup);
            await _context.SaveChangesAsync();
        }
        public async Task CreateUserToEmailToken(int userId, string token)
        {
            Log.Information("Service called: Creating user to email token with userID {0}", userId);
            var userToTokenLookup = new UserToTokenLookup
            {
                UserId = userId,
                VerifyEmailToken = token
            };
            _context.UserToTokenLookup.Add(userToTokenLookup);
            await _context.SaveChangesAsync();
        }

        [HttpPost]
        public async Task<bool> VerifyEmail(string token, int userId)
        {
            Log.Information("Service called: Verifying email with userId {0}", userId, " and token {1}", token);
            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == userId);
            if (user == null)
            {
                return false;
            }
            var result = await _context.UserToTokenLookup.FirstOrDefaultAsync(m => m.UserId == user.Id && m.VerifyEmailToken! == token);
            if (result != null)
            {
                user.Authenticated = true;

                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        [HttpPost]
        public async Task CreateUser(User user)
        {
            Log.Information("Service called: Creating user with login {0}", user.Login);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        [HttpGet]
        public async Task<User> GetUserById(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            return user;
        }

        [HttpGet]
        public async Task<List<User>> GetAllUsers()
        {
            Log.Information("Service called: Getting all users");
            var users = await _context.Users.ToListAsync();
            return users;
        }

        [HttpGet]
        public async Task<User> GetUserByLogin(string login)
        {
            Log.Information("Service called: Getting user with login {0}", login);
            var user = await _context.Users.FirstOrDefaultAsync(m => m.Login == login);
            return user;
        }

        // [HttpPut]
        // public async Task<bool> UpdateEmail(int id, string email){
        //     var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
        //     if (user == null){
        //         return false;
        //     }
        //     user.Email = email;
        //     _context.Entry(user).State = EntityState.Modified;
        //     var result = await _context.SaveChangesAsync(); 
        // }

    }
}
