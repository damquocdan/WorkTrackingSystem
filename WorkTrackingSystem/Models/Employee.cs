using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkTrackingSystem.Models​;

public partial class Employee
{
    public long Id { get; set; }

	[Display(Name = "Mã phòng ban")]
	public long? DepartmentId { get; set; }

	[Display(Name = "Mã chức vụ")]
	public long? PositionId { get; set; }

	[Display(Name = "Mã nhân viên")]
	public string Code { get; set; } = null!;

	[Display(Name = "Họ")]
	public string? FirstName { get; set; }

	[Display(Name = "Tên")]
	public string? LastName { get; set; }

	[Display(Name = "Giới tính")]
	public string? Gender { get; set; }

	[Display(Name = "Ngày sinh")]
	public DateOnly? Birthday { get; set; }

	[Display(Name = "Số điện thoại")]
	public string? Phone { get; set; }

	[Display(Name = "Email")]
	public string? Email { get; set; }

	[Display(Name = "Ngày vào làm")]
	public DateOnly? HireDate { get; set; }

	[Display(Name = "Địa chỉ")]
	public string? Address { get; set; }

	[Display(Name = "Ảnh đại diện")]
	public string? Avatar { get; set; }

	[Display(Name = "Đã xóa?")]
	public bool? IsDelete { get; set; }

	[Display(Name = "Hoạt động?")]
	public bool? IsActive { get; set; }

	[Display(Name = "Ngày tạo")]
	public DateTime? CreateDate { get; set; }

	[Display(Name = "Ngày cập nhật")]
	public DateTime? UpdateDate { get; set; }

	[Display(Name = "Người tạo")]
	public string? CreateBy { get; set; }

	[Display(Name = "Người cập nhật")]
	public string? UpdateBy { get; set; }
	public virtual ICollection<Analysis> Analyses { get; set; } = new List<Analysis>();

    public virtual ICollection<Baselineassessment> Baselineassessments { get; set; } = new List<Baselineassessment>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<Jobmapemployee> Jobmapemployees { get; set; } = new List<Jobmapemployee>();

    public virtual Position? Position { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
