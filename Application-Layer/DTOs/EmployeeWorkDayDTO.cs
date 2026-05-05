namespace Application_Layer.DTOs;

public class EmployeeWorkDayDTO
{
    public string Date { get; set; } = string.Empty;      // "yyyy-MM-dd"
    public string StartTime { get; set; } = string.Empty; // "HH:mm:ss"
    public string EndTime { get; set; } = string.Empty;   // "HH:mm:ss"
}

public class SetWorkDaysDTO
{
    public string From { get; set; } = string.Empty; // "yyyy-MM-dd" start of range
    public string To { get; set; } = string.Empty;   // "yyyy-MM-dd" end of range
    public List<EmployeeWorkDayDTO> WorkDays { get; set; } = new();
}

public class GenerateWorkDaysDTO
{
    public string From { get; set; } = string.Empty; // "yyyy-MM-dd"
    public string To { get; set; } = string.Empty;   // "yyyy-MM-dd"
}
