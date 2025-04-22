namespace WorkTrackingSystem.Areas.AdminSystem.Models
{
    public class ScoreSummary
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal TotalVolume { get; set; }
        public decimal TotalProgress { get; set; }
        public decimal TotalQuality { get; set; }
        public decimal SummaryScore { get; set; }
        public string EvaluationResult { get; set; }
    }
}
