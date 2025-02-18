using System;
using System.Collections.Generic;

namespace WorkTrackingSystem.Models​;

public partial class User
{
    public long Id { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public long? EmployeeId { get; set; }

    public bool? IsDelete { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public long? CreateBy { get; set; }

    public long? UpdateBy { get; set; }

    public virtual Employee? Employee { get; set; }
}
