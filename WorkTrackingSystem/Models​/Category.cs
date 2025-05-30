﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkTrackingSystem.Models​;

public partial class Category
{
	public long Id { get; set; }
	[Display(Name = "Mã danh mục")]

	public string Code { get; set; } = null!;

	[Display(Name = "Tên danh mục")]
	public string? Name { get; set; }

	[Display(Name = "Mô tả")]
	public string? Description { get; set; }

	[Display(Name = "ID danh mục cha")]
	public long? IdParent { get; set; }

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

	public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
}
