using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Token.Services;
using TokenTest.Models;
namespace TokenTest.Controllers;


using Microsoft.AspNetCore.Mvc;
[Authorize]
[ApiController]
[Route("[controller]")]
public class GoalController : ControllerBase
{

    private readonly GoalService _goalService;
    public GoalController(GoalService goalService)
    {
        _goalService = goalService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateGoal([FromBody] Goal goal)
    {
        Log.Information("Controller called: Creating goal for student {0}", goal.StudentId);
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            await _goalService.CreateGoal(goal);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal server error while creating the goal: " + e.Message);
        }
    }

    [HttpGet("{goalId}")]
    public async Task<IActionResult> GetGoalById(int goalId)
    {
        var goal = await _goalService.GetGoalById(goalId);
        if (goal == null)
        {
            return NotFound();
        }
        return Ok(goal);
    }

    [HttpGet("get/student/{studentId}")]
    public async Task<IActionResult> GetStudentGoals(int studentId)
    {
        try
        {
            var goals = await _goalService.GetStudentGoals(studentId);
            if (goals == null)
            {
                return NotFound();
            }
            return Ok(goals);
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal server error while getting the goals: " + e.Message);
        }
    }


    [HttpDelete("delete/{goalId}")]
    public async Task<IActionResult> DeleteGoalById(int goalId)
    {
        try
        {
            await _goalService.DeleteGoalById(goalId);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal server error while deleting the goal: " + e.Message);
        }
    }

    [HttpPut("recentdata/{goalId}/{dataId}")]
    public async Task<IActionResult> UpdateRecentData(int goalId, int dataId)
    {
        try
        {
            await _goalService.UpdateRecentData(goalId, dataId);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal server error while updating the recent data: " + e.Message);
        }
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateGoal([FromBody] Goal goal)
    {
        try
        {
            await _goalService.EditGoalById(goal);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal server error while updating the goal: " + e.Message);
        }
    }
}