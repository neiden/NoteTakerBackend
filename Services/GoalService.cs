
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Token.Data;
using TokenTest.Models;
namespace Token.Services;

public class GoalService
{
    private readonly TokenContext _context;
    private readonly IConfiguration _configuration;
    private readonly Aes _aes;

    public GoalService(TokenContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _aes = Aes.Create();
        _aes.Key = Convert.FromBase64String(_configuration["AESKEY"]);
    }

    [HttpPost]
    public async Task CreateGoal(Goal goal)
    {
        _context.Goal.Add(goal);
        await _context.SaveChangesAsync();
    }

    [HttpGet]
    public async Task<Goal> GetGoalById(int id)
    {
        var goal = await _context.Goal.FirstOrDefaultAsync(m => m.Id == id);
        return goal;
    }

    [HttpGet]
    public async Task<List<Goal>> GetStudentGoals(int studentId)
    {
        var goals = await _context.Goal.Where(n => n.StudentId == studentId).ToListAsync();
        return goals;
    }

    [HttpDelete]
    public async Task DeleteGoalById(int id)
    {
        var goal = await _context.Goal.FirstOrDefaultAsync(m => m.Id == id);
        if (goal != null)
        {
            _context.Goal.Remove(goal);
            await _context.SaveChangesAsync();
        }
    }
    [HttpPut]
    public async Task UpdateRecentData(int goalId, int dataId)
    {
        var goal = await _context.Goal.FirstOrDefaultAsync(m => m.Id == goalId);
        goal!.RecentData = dataId;
        _context.Entry(goal).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    [HttpPut]
    public async Task EditGoalById(Goal goal)
    {

        _context.Entry(goal).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }


}