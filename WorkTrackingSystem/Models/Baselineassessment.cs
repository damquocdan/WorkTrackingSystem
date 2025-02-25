using System;
using System.Collections.Generic;

namespace WorkTrackingSystem.Models;

public partial class Baselineassessment
{
    public long Id { get; set; }

    public long? EmployeeId { get; set; }

    public double? VolumeAssessment { get; set; }

    public double? ProgressAssessment { get; set; }

    public double? QualityAssessment { get; set; }

    public double? SummaryOfReviews { get; set; }

    public DateTime? Time { get; set; }

    public bool? Evaluate { get; set; }

    public bool? IsDelete { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public long? CreateBy { get; set; }

    public long? UpdateBy { get; set; }

    public virtual Employee? Employee { get; set; }
}
