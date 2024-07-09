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
public class DataController : ControllerBase
{
    private readonly DataService _dataService;
    public DataController(DataService dataService)
    {
        _dataService = dataService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateData([FromBody] DataEntry data)
    {
        Log.Information("Controller called: Creating data ", data.Note);
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var createdData = await _dataService.CreateData(data);
            return Ok(createdData);
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal server error while creating the data: " + e.Message);
        }
    }

    [HttpGet("{dataId}")]
    public async Task<IActionResult> GetDataById(int dataId)
    {
        var data = await _dataService.GetDataById(dataId);
        if (data == null)
        {
            return NotFound();
        }
        return Ok(data);
    }

    [HttpGet("get/student/{studentId}/goal/{goalId}")]
    public async Task<IActionResult> GetStudentData(int studentId, int goalId)
    {
        try
        {
            var data = await _dataService.GetStudentData(studentId, goalId);
            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal server error while getting the data: " + e.Message);
        }
    }

    [HttpPut("update")]
    public async Task<IActionResult> EditDataById([FromBody] DataEntry data)
    {
        try
        {
            await _dataService.EditDataById(data);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal server error while editing the data: " + e.Message);
        }
    }

    [HttpGet("range")]
    public async Task<IActionResult> GetDataByRange([FromQuery] DateTime start, [FromQuery] DateTime end, [FromQuery] int studentId, [FromQuery] int goalId)
    {
        var data = await _dataService.GetDataByRange(start, end, studentId, goalId);
        if (data == null)
        {
            return NotFound();
        }
        return Ok(data);
    }

    [HttpDelete("delete/{dataId}")]
    public async Task<IActionResult> DeleteDataById(int dataId)
    {
        try
        {
            await _dataService.DeleteDataById(dataId);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal server error while deleting the data: " + e.Message);
        }
    }


}