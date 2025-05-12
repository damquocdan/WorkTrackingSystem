using System;
using System.Collections.Generic;

namespace WorkTrackingSystem.Models​;

public partial class Jobrepeat
{
    public long Id { get; set; }

    public long? JobId { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public DateOnly? Deadline1 { get; set; }

    public DateOnly? Deadline2 { get; set; }

    public DateOnly? Deadline3 { get; set; }

    public bool? IsDelete { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public string? CreateBy { get; set; }

    public string? UpdateBy { get; set; }

    public virtual Job? Job { get; set; }

    public virtual ICollection<Jobmapemployee> Jobmapemployees { get; set; } = new List<Jobmapemployee>();
}
