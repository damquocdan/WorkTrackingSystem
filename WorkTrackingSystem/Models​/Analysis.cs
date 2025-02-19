using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkTrackingSystem.Models;

public partial class Analysis
{
    public long Id { get; set; }

    [Display(Name = "Mã nhân viên")]
    public long? EmployeeId { get; set; }

    [Display(Name = "Tổng số")]
    public double? Total { get; set; }

    [Display(Name = "Đúng hạn")]
    public int? Ontime { get; set; }

    [Display(Name = "Trễ hạn")]
    public int? Late { get; set; }

    [Display(Name = "Quá hạn")]
    public int? Overdue { get; set; }

    [Display(Name = "Đang xử lý")]
    public int? Processing { get; set; }

    [Display(Name = "Thời gian")]
    public DateTime? Time { get; set; }

    [Display(Name = "Đã xóa")]
    public bool? IsDelete { get; set; }

    [Display(Name = "Đang hoạt động")]
    public bool? IsActive { get; set; }

    [Display(Name = "Ngày tạo")]
    public DateTime? CreateDate { get; set; }

    [Display(Name = "Ngày cập nhật")]
    public DateTime? UpdateDate { get; set; }

    [Display(Name = "Người tạo")]
    public long? CreateBy { get; set; }

    [Display(Name = "Người cập nhật")]
    public long? UpdateBy { get; set; }

    [Display(Name = "Nhân viên")]
    public virtual Employee? Employee { get; set; }
}