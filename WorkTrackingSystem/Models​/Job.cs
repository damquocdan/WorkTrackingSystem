using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkTrackingSystem.Models;

public partial class Job
{
    public long Id { get; set; }

    [Display(Name = "Mã nhân viên")]
    public long? EmployeeId { get; set; }

    [Display(Name = "Mã danh mục")]
    public long? CategoryId { get; set; }

    [Display(Name = "Tên công việc")]
    public string? Name { get; set; }

    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Display(Name = "Hạn chót 1")]
    public DateOnly? Deadline1 { get; set; }

    [Display(Name = "Hạn chót 2")]
    public DateOnly? Deadline2 { get; set; }

    [Display(Name = "Hạn chót 3")]
    public DateOnly? Deadline3 { get; set; }

    [Display(Name = "Ngày hoàn thành")]
    public DateOnly? CompletionDate { get; set; }

    [Display(Name = "Trạng thái")]
    public byte? Status { get; set; }

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

    [Display(Name = "Danh mục")]
    public virtual Category? Category { get; set; }

    [Display(Name = "Nhân viên")]
    public virtual Employee? Employee { get; set; }
}