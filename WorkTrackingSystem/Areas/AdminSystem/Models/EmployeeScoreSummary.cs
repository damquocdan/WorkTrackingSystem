namespace WorkTrackingSystem.Areas.AdminSystem.Models
{
    public class EmployeeScoreSummary
    {
        public string EmployeeName { get; set; }
        public int OnTimeCount { get; set; }
        public int LateCount { get; set; }
        public int OverdueCount { get; set; }
        public int ProcessingCount { get; set; }
        public int TotalJobs { get; set; }
    }
}
