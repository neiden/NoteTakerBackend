
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Token.Data;
using TokenTest.Models;
namespace Token.Services;

public class NoteService
{
    private readonly TokenContext _context;
    private readonly IConfiguration _configuration;
    private readonly Aes _aes;

    public NoteService(TokenContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _aes = Aes.Create();
        _aes.Key = Convert.FromBase64String(_configuration["AESKEY"]);
    }

    [HttpPost]
    public async Task CreateNote(Note note)
    {
        note.Content = AesHelper.Encrypt(note.Content, _aes);
        _context.Note.Add(note);
        await _context.SaveChangesAsync();
    }

    [HttpGet]
    public async Task<Note> GetNoteById(int id)
    {
        var note = await _context.Note.FirstOrDefaultAsync(m => m.Id == id);
        if (note != null)
        {
            note.Content = AesHelper.Decrypt(note.Content, _aes);
        }
        return note;
    }

    [HttpGet]
    public async Task<List<Note>> GetStudentNotes(int studentId)
    {
        var notes = await _context.Note.Where(n => n.StudentId == studentId).ToListAsync();
        foreach (var note in notes)
        {
            note.Content = AesHelper.Decrypt(note.Content, _aes);
        }
        return notes;
    }

    [HttpDelete]
    public async Task DeleteNoteById(int id)
    {
        var note = await _context.Note.FirstOrDefaultAsync(m => m.Id == id);
        if (note != null)
        {
            _context.Note.Remove(note);
            await _context.SaveChangesAsync();
        }
    }

    [HttpPut]
    public async Task EditNoteById(int id, Note note)
    {
        var noteToEdit = await _context.Note.FirstOrDefaultAsync(m => m.Id == id);
        if (noteToEdit != null)
        {
            noteToEdit.StudentId = note.StudentId;
            noteToEdit.Date = note.Date;
            noteToEdit.Content = AesHelper.Encrypt(note.Content, _aes);
            await _context.SaveChangesAsync();
        }
    }

}