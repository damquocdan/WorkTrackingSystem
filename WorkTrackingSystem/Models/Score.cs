using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkTrackingSystem.Models​;

public partial class Score
{
    public long Id { get; set; }

    public long? JobMapEmployeeId { get; set; }
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
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

	[Display(Name = "Tổng hợp đánh giá")]
	public double? SummaryOfReviews { get; set; }

	[Display(Name = "Tiến độ")]
	public double? Progress { get; set; }

	[Display(Name = "Ngày bắt đầu")]
	[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
	public DateTime? Time { get; set; }

	[Display(Name = "Đã xóa?")]
	public bool? IsDelete { get; set; }

	[Display(Name = "Hoạt động?")]
	public bool? IsActive { get; set; }

	[Display(Name = "Ngày tạo")]
	[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
	public DateTime? CreateDate { get; set; }

	[Display(Name = "Ngày cập nhật")]
	public DateTime? UpdateDate { get; set; }

	[Display(Name = "Người tạo")]
	public string? CreateBy { get; set; }

	[Display(Name = "Người cập nhật")]
	public string? UpdateBy { get; set; }

	public virtual Jobmapemployee? JobMapEmployee { get; set; }
}
