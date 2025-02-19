using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkTrackingSystem.Models;

public partial class Baselineassessment
{
    public long Id { get; set; }

    [Display(Name = "Mã nhân viên")]
    public long? EmployeeId { get; set; }

    [Display(Name = "Đánh giá khối lượng")]
    public double? VolumeAssessment { get; set; }

    [Display(Name = "Đánh giá tiến độ")]
    public double? ProgressAssessment { get; set; }

    [Display(Name = "Đánh giá chất lượng")]
    public double? QualityAssessment { get; set; }

    [Display(Name = "Tổng đánh giá")]
    public double? SummaryOfReviews { get; set; }

    [Display(Name = "Thời gian")]
    public DateTime? Time { get; set; }

    [Display(Name = "Đã đánh giá")] // Or "Đánh giá" depending on context
    public bool? Evaluate { get; set; }

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