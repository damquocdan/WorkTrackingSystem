using System;
using System.Collections.Generic;

namespace WorkTrackingSystem.Models​;

public partial class Jobmapemployee
{
    public long Id { get; set; }

    public long? EmployeeId { get; set; }

    public long? JobId { get; set; }

    public bool? IsDelete { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? CreateBy { get; set; }

    public string? UpdateBy { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual Job? Job { get; set; }

    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();
}
