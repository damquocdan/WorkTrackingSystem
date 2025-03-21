namespace WorkTrackingSystem.Areas.AdminSystem.Models
{
	public class JobViewModel
	{
		public long JobId { get; set; }
		public string JobName { get; set; }
		public string CategoryName { get; set; }
		public string EmployeeName { get; set; }
		public int? ScoreStatus { get; set; }
		public DateOnly? CompletionDate { get; set; }
		public DateOnly? Deadline { get; set; }
		public double? Progress { get; set; }
		public double? VolumeAssessment { get; set; }

		public double? ProgressAssessment { get; set; }

		public double? QualityAssessment { get; set; }

		public double? SummaryOfReviews { get; set; }
	}
}
