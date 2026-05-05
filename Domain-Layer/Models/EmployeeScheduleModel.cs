namespace Domain_Layer.Models
{
    public class EmployeeScheduleModel
    {
        public Guid Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
