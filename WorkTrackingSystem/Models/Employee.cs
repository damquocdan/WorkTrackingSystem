using System;
using System.Collections.Generic;

namespace WorkTrackingSystem.Models;

public partial class Employee
{
    public long Id { get; set; }

    public long? DepartmentId { get; set; }

    public long? PositionId { get; set; }

    public string? Code { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Gender { get; set; }

    public DateOnly? Birthday { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public DateOnly? HireDate { get; set; }

    public string? Address { get; set; }

    public string? Avatar { get; set; }

    public bool? IsDelete { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public long? CreateBy { get; set; }

    public long? UpdateBy { get; set; }

    public virtual ICollection<Analysis> Analyses { get; set; } = new List<Analysis>();

    public virtual ICollection<Baselineassessment> Baselineassessments { get; set; } = new List<Baselineassessment>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    public virtual Position? Position { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
