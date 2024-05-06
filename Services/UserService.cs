using System.Collections.Generic;
using TokenTest.Models;
using Token.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace TokenTest.Services
{
    public class UserService
    {
        private List<User> users;
        private readonly Token.Data.TokenContext _context;
        public UserService(Token.Data.TokenContext context)
        {
            _context = context;
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

    }



}
