namespace TokenTest.Models;
public class DataEntry
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int GoalId { get; set; }
    public int Independent { get; set; }
    public int Prompted { get; set; }
    public int SelfCorrected { get; set; }
    public int Teaching { get; set; }
    public DateTime Date { get; set; }
    public string? Note { get; set; }
}