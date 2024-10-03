using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Token.Services;
using TokenTest.Models;

namespace TokenTest.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class StudentController : ControllerBase
{

    private readonly StudentService _studentService;
    private readonly IConfiguration _configuration;

    public StudentController(StudentService studentService, IConfiguration configuration)
    {
        _studentService = studentService;
        _configuration = configuration;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateStudent([FromBody] Student student)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            var jwtToken = authHeader.Replace("Bearer ", "");
            int id = int.Parse(JWTParser.GetClaim(jwtToken, "unique_name")!);
            student.TeacherId = id;
            await _studentService.CreateStudent(student);
            return Ok();
        }
        catch (DbUpdateException e) when (e.InnerException is SqlException sqlException && (sqlException.Number == 2627 || sqlException.Number == 2601))
        {
            return Conflict("Student already exists");
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal server error while creating the student: " + e.Message);
        }
    }

    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetStudentById(int id)
    {
        var student = await _studentService.GetStudentById(id);
        if (student == null)
        {
            return NotFound();
        }
        return Ok(student);
    }

    [HttpGet("getStudentsByTeacherId")]
    public async Task<IActionResult> GetAllStudentsByTeacherId()
    {
        try
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            // Extract the JWT token from the Authorization header
            var jwtToken = authHeader.Replace("Bearer ", "");
            int id = int.Parse(JWTParser.GetClaim(jwtToken, "unique_name")!);
            var students = await _studentService.GetAllStudentsByTeacherId(id);
            if (students == null)
            {
                return NotFound();
            }
            return Ok(students);
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal server error while getting all students: " + e.Message);
        }
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        try
        {
            var student = await _studentService.GetStudentById(id);
            if (student == null)
            {
                return NotFound();
            }
            await _studentService.DeleteStudent(student!.Id);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal server error while deleting the student: " + e.Message);
        }
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateStudent(Student student)
    {
        try
        {
            await _studentService.UpdateStudent(student);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal server error while updating the student: " + e.Message);
        }
    }

    [HttpPost("upload")]
    public async Task<IActionResult> ProcessFile(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty");
            }
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            var jwtToken = authHeader.Replace("Bearer ", "");
            int id = int.Parse(JWTParser.GetClaim(jwtToken, "unique_name")!);
            await _studentService.ProcessFile(file, id);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal server error while processing the file: " + e.Message);
        }
    }


}
