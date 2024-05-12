
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Token.Data;
using TokenTest.Models;

namespace Token.Services;
public class StudentService
{
    private readonly TokenContext _context;
    private readonly IConfiguration _configuration;
    private readonly Aes _aes;

    public StudentService(TokenContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        _aes = Aes.Create();
        _aes.Key = Convert.FromBase64String(_configuration["AESKEY"]);
    }

    [HttpPost]
    public async Task CreateStudent(Student student)
    {
        student.FName = AesHelper.Encrypt(student.FName, _aes);
        student.LName = AesHelper.Encrypt(student.LName, _aes);
        student.School = AesHelper.Encrypt(student.School, _aes);
        _context.Student.Add(student);
        await _context.SaveChangesAsync();
    }

    [HttpGet]
    public async Task<Student> GetStudentById(int id)
    {
        var student = await _context.Student.FirstOrDefaultAsync(m => m.Id == id);
        if (student != null)
        {
            student.FName = AesHelper.Decrypt(student.FName, _aes);
            student.LName = AesHelper.Decrypt(student.LName, _aes);
            student.School = AesHelper.Decrypt(student.School, _aes);
        }
        return student;
    }

    [HttpGet]
    public async Task<List<Student>> GetAllStudents()
    {
        var students = await _context.Student.ToListAsync();
        foreach (var student in students)
        {
            student.FName = AesHelper.Decrypt(student.FName, _aes);
            student.LName = AesHelper.Decrypt(student.LName, _aes);
            student.School = AesHelper.Decrypt(student.School, _aes);
        }
        return students;
    }

    [HttpGet]
    public async Task<List<Student>> GetAllStudentsByTeacherId(int id)
    {
        var students = await _context.Student.Where(m => m.TeacherId == id).ToListAsync();
        foreach (var student in students)
        {
            student.FName = AesHelper.Decrypt(student.FName, _aes);
            student.LName = AesHelper.Decrypt(student.LName, _aes);
            student.School = AesHelper.Decrypt(student.School, _aes);
        }
        return students;
    }


    [HttpDelete]
    public async Task<Student> DeleteStudent(int id)
    {
        var student = await _context.Student.FirstOrDefaultAsync(m => m.Id == id);
        if (student == null)
        {
            return null;
        }

        _context.Student.Remove(student);
        await _context.SaveChangesAsync();
        return student;
    }

    [HttpPut]
    public async Task UpdateStudent(Student student)
    {
        student.FName = AesHelper.Encrypt(student.FName, _aes);
        student.LName = AesHelper.Encrypt(student.LName, _aes);
        student.School = AesHelper.Encrypt(student.School, _aes);
        _context.Entry(student).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

}