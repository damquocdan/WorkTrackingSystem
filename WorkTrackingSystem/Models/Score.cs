using System;
using System.Collections.Generic;

namespace WorkTrackingSystem.Models​;

public partial class Score
{
    public long Id { get; set; }

    public long? JobMapEmployeeId { get; set; }

    public DateOnly? CompletionDate { get; set; }

    public byte? Status { get; set; }

    public double? VolumeAssessment { get; set; }

    public double? ProgressAssessment { get; set; }

    public double? QualityAssessment { get; set; }

    public double? SummaryOfReviews { get; set; }

    public double? Progress { get; set; }

    public DateTime? Time { get; set; }

    public bool? IsDelete { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? CreateBy { get; set; }

    public string? UpdateBy { get; set; }

    public virtual Jobmapemployee? JobMapEmployee { get; set; }
}
