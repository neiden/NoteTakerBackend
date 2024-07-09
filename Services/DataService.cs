
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Token.Data;
using TokenTest.Models;
namespace Token.Services;

using Microsoft.AspNetCore.Mvc;
public class DataService
{
    private readonly TokenContext _context;
    private readonly IConfiguration _configuration;
    private readonly Aes _aes;

    public DataService(TokenContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _aes = Aes.Create();
        _aes.Key = Convert.FromBase64String(_configuration["AESKEY"]);
    }

    [HttpPost]
    public async Task<DataEntry> CreateData(DataEntry data)
    {
        if (data.Note != null)
        {
            data.Note = AesHelper.Encrypt(data.Note!, _aes);
        }
        _context.Data.Add(data);
        await _context.SaveChangesAsync();
        return data;
    }

    [HttpGet]
    public async Task<DataEntry> GetDataById(int id)
    {
        var data = await _context.Data.FirstOrDefaultAsync(m => m.Id == id);
        if (data != null && data.Note != null)
        {
            data.Note = AesHelper.Decrypt(data.Note!, _aes);
        }
        return data;
    }

    [HttpGet]
    public async Task<List<DataEntry>> GetStudentData(int studentId, int goalId)
    {
        var data = await _context.Data.Where(n => n.StudentId == studentId && n.GoalId == goalId).ToListAsync();
        foreach (var d in data)
        {
            if (d.Note != null)
            {
                d.Note = AesHelper.Decrypt(d.Note!, _aes);
            }
        }
        return data;
    }

    [HttpPut]
    public async Task EditDataById(DataEntry data)
    {
        if (data.Note != null)
        {
            data.Note = AesHelper.Encrypt(data.Note!, _aes);
        }
        _context.Entry(data).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    [HttpDelete]
    public async Task DeleteDataById(int id)
    {
        var data = await _context.Data.FirstOrDefaultAsync(m => m.Id == id);
        if (data != null)
        {
            _context.Data.Remove(data);
            await _context.SaveChangesAsync();
        }
    }

    [HttpGet]
    public async Task<List<DataEntry>> GetDataByRange(DateTime start, DateTime end, int goalId, int studentId)
    {
        var data = await _context.Data.Where(m => m.Date >= start && m.Date <= end && m.StudentId == studentId && m.GoalId == goalId).ToListAsync();
        foreach (var d in data)
        {
            if (d.Note != null)
            {
                d.Note = AesHelper.Decrypt(d.Note!, _aes);
            }
        }
        return data;
    }

}