
using System.Security.Cryptography;
using System.Text;
using ExcelDataReader;
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

    public async Task ProcessFile(IFormFile file, int teacherId)
    {
        Console.WriteLine("Processing file " + file.FileName);
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);
            stream.Position = 0;
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {

                var result = reader.AsDataSet();
                if (result.Tables.Count == 0)
                {
                    return;
                }
                var table = result.Tables[0]!;
                var students = new List<Student>();

                for (int i = 1; i < table.Rows.Count; i++)
                {

                    string FName = table.Rows[i][3].ToString()!;
                    string LName = table.Rows[i][2].ToString()!;
                    string School = table.Rows[i][1].ToString()!;
                    string BirthDateString = table.Rows[i][4].ToString()!;
                    string EligibilityExpirationString = table.Rows[i][8].ToString()!;
                    string DueDateString = table.Rows[i][10].ToString()!;

                    DateTime BirthDate;
                    if (!DateTime.TryParse(BirthDateString, out BirthDate))
                    {
                        continue;
                    }

                    DateTime EligibilityExpiration;
                    if (!DateTime.TryParse(EligibilityExpirationString, out EligibilityExpiration))
                    {
                        continue;
                    }

                    DateTime DueDate;
                    if (!DateTime.TryParse(DueDateString, out DueDate))
                    {
                        continue;
                    }

                    int TeacherId = teacherId;
                    var student = new Student
                    {
                        FName = AesHelper.Encrypt(FName, _aes),
                        LName = AesHelper.Encrypt(LName, _aes),
                        School = AesHelper.Encrypt(School, _aes),
                        TeacherId = TeacherId,
                        BirthDate = BirthDate,
                        EligibilityExpiration = EligibilityExpiration,
                        DueDate = DueDate
                    };

                    students.Add(student);

                }

                await _context.Student.AddRangeAsync(students);
                await _context.SaveChangesAsync();
            }
        }

    }
}