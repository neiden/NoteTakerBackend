
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
        //TODO Add key as application setting in Azure Web App
    }

    private string Encrypt(string text)
    {
        _aes.GenerateIV();
        var symmetricEncryptor = _aes.CreateEncryptor(_aes.Key, _aes.IV);

        using (var memoryStream = new MemoryStream())
        {
            memoryStream.Write(_aes.IV, 0, _aes.IV.Length);

            using (var cryptoStream = new CryptoStream(memoryStream, symmetricEncryptor, CryptoStreamMode.Write))
            {
                using (var streamWriter = new StreamWriter(cryptoStream))
                {
                    streamWriter.Write(text);
                }
            }
            return Convert.ToBase64String(memoryStream.ToArray());
        }
    }

    private string Decrypt(string encryptedText)
    {
        var encryptedBytes = Convert.FromBase64String(encryptedText);

        var iv = new byte[_aes.BlockSize / 8];
        Array.Copy(encryptedBytes, iv, iv.Length);

        var symmetricDecryptor = _aes.CreateDecryptor(_aes.Key, iv);

        using (var memoryStream = new MemoryStream(encryptedBytes, iv.Length, encryptedBytes.Length - iv.Length))
        {
            using (var cryptoStream = new CryptoStream(memoryStream, symmetricDecryptor, CryptoStreamMode.Read))
            {
                using (var streamReader = new StreamReader(cryptoStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }

    [HttpPost]
    public async Task CreateStudent(Student student)
    {
        student.FName = Encrypt(student.FName);
        student.LName = Encrypt(student.LName);
        student.School = Encrypt(student.School);
        _context.Student.Add(student);
        await _context.SaveChangesAsync();
    }

    [HttpGet]
    public async Task<Student> GetStudentById(int id)
    {
        var student = await _context.Student.FirstOrDefaultAsync(m => m.Id == id);
        if (student != null)
        {
            student.FName = Decrypt(student.FName);
            student.LName = Decrypt(student.LName);
            student.School = Decrypt(student.School);
        }
        return student;
    }

    [HttpGet]
    public async Task<List<Student>> GetAllStudents()
    {
        var students = await _context.Student.ToListAsync();
        foreach (var student in students)
        {
            student.FName = Decrypt(student.FName);
            student.LName = Decrypt(student.LName);
            student.School = Decrypt(student.School);
        }
        return students;
    }

    [HttpGet]
    public async Task<List<Student>> GetAllStudentsByTeacherId(int id)
    {
        var students = await _context.Student.Where(m => m.TeacherId == id).ToListAsync();
        foreach (var student in students)
        {
            student.FName = Decrypt(student.FName);
            student.LName = Decrypt(student.LName);
            student.School = Decrypt(student.School);
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
        student.FName = Encrypt(student.FName);
        student.LName = Encrypt(student.LName);
        student.School = Encrypt(student.School);
        _context.Entry(student).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

}