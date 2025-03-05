using System;
using System.Collections.Generic;

namespace WorkTrackingSystem.Models;

public partial class Analysis
{
    public long Id { get; set; }

    public long? EmployeeId { get; set; }

    public double? Total { get; set; }

    public int? Ontime { get; set; }

    public int? Late { get; set; }

    public int? Overdue { get; set; }

    public int? Processing { get; set; }

    public DateTime? Time { get; set; }

    public bool? IsDelete { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? CreateBy { get; set; }

    public string? UpdateBy { get; set; }

    public virtual Employee? Employee { get; set; }
}
