namespace TokenTest.Models;
public class Student
{
    public required string FName { get; set; }
    public int Id { get; set; }
    public required string LName { get; set; }
    public int TeacherId { get; set; }
    public User? Teacher { get; set; }
    public required string School { get; set; }
    public DateTime DueDate { get; set; }
    public required DateTime BirthDate { get; set; }
    public DateTime EligibilityExpiration { get; set; }

}