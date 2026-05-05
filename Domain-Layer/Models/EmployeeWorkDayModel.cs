namespace Domain_Layer.Models;

public class EmployeeWorkDayModel
{
    public Guid Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
