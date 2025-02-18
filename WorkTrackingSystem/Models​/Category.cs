using System;
using System.Collections.Generic;

namespace WorkTrackingSystem.Models​;

public partial class Category
{
    public long Id { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public long? IdParent { get; set; }

    public bool? IsDelete { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public long? CreateBy { get; set; }

    public long? UpdateBy { get; set; }

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
