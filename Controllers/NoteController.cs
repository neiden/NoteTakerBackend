using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Token.Services;
using TokenTest.Models;
namespace TokenTest.Controllers;



[Authorize]
[ApiController]
[Route("[controller]")]
public class NoteController : ControllerBase
{
    private readonly NoteService _noteService;

    public NoteController(NoteService noteService)
    {
        _noteService = noteService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateNote([FromBody] Note note)
    {
        Log.Information("Controller called: Creating note for student {0}", note.StudentId);
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            await _noteService.CreateNote(note);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal server error while creating the note: " + e.Message);
        }
    }

    [HttpGet("{noteId}")]
    public async Task<IActionResult> GetNoteById(int noteId)
    {
        var note = await _noteService.GetNoteById(noteId);
        if (note == null)
        {
            return NotFound();
        }
        return Ok(note);
    }

    [HttpGet("get/student/{studentId}")]
    public async Task<IActionResult> GetStudentNotes(int studentId)
    {
        try
        {
            var notes = await _noteService.GetStudentNotes(studentId);
            if (notes == null)
            {
                return NotFound();
            }
            return Ok(notes);
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal server error while getting the notes: " + e.Message);
        }
    }

    [HttpDelete("delete/{noteId}")]
    public async Task<IActionResult> DeleteNoteById(int noteId)
    {
        try
        {
            await _noteService.DeleteNoteById(noteId);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal server error while deleting the note: " + e.Message);
        }
    }

    [HttpPut("update/{noteId}")]
    public async Task<IActionResult> UpdateNoteById([FromBody] Note note, int noteId)
    {
        try
        {
            await _noteService.EditNoteById(noteId, note);
            return Ok();
        }
        catch (Exception e)
        {
            return StatusCode(500, "Internal server error while updating the note: " + e.Message);
        }
    }

}