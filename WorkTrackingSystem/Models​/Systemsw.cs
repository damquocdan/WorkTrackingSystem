using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkTrackingSystem.Models;

public partial class Systemsw
{
    public long Id { get; set; }

    [Display(Name = "Tên hệ thống")]
    public string? Name { get; set; }

    [Display(Name = "Giá trị")]
    public string? Vaulue { get; set; } // Consider correcting the typo "Vaulue" to "Value"

    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

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
}