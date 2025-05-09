using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkTrackingSystem.Models​;

public partial class User
{
	public long Id { get; set; }

	[Display(Name = "Tên đăng nhập")]
	public string UserName { get; set; } = null!;

	[Display(Name = "Mật khẩu")]
	public string? Password { get; set; }

	[Display(Name = "Mã nhân viên")]
	public long? EmployeeId { get; set; }

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
	public virtual Employee? Employee { get; set; }
}
