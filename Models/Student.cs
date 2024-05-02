namespace TokenTest.Models;
public class Student
{
    public required string Fname { get; set; }
    public int Id { get; set; }
    public required string LName { get; set; }
    public int TeacherId { get; set; }
    public User? Teacher { get; set; }
    public required string School { get; set; }
    public DateTime DueDate { get; set; }
    public required int Age { get; set; }

}