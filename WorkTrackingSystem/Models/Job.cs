using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkTrackingSystem.Models​;

public partial class Job
{
    public long Id { get; set; }

	[Display(Name = "Mã danh mục")]
	public long? CategoryId { get; set; }

	[Display(Name = "Tên công việc")]
	public string? Name { get; set; }

	[Display(Name = "Mô tả công việc")]
	public string? Description { get; set; }

	[Display(Name = "Hạn chót giai đoạn 1")]
	public DateOnly? Deadline1 { get; set; }

	[Display(Name = "Hạn chót giai đoạn 2")]
	public DateOnly? Deadline2 { get; set; }

	[Display(Name = "Hạn chót giai đoạn 3")]
	public DateOnly? Deadline3 { get; set; }

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


public virtual Category? Category { get; set; }

    public virtual ICollection<Jobmapemployee> Jobmapemployees { get; set; } = new List<Jobmapemployee>();
}
