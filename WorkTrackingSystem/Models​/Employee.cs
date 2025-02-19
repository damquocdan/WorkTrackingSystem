using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkTrackingSystem.Models;

public partial class Employee
{
    public long Id { get; set; }

    [Display(Name = "Mã phòng ban")]
    public long? DepartmentId { get; set; }

    [Display(Name = "Mã vị trí")]
    public long? PositionId { get; set; }

    [Display(Name = "Mã nhân viên")]
    public string? Code { get; set; }

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

    [Display(Name = "Ngày thuê")]
    public DateOnly? HireDate { get; set; }

    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }

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

    [Display(Name = "Phân tích")]
    public virtual ICollection<Analysis> Analyses { get; set; } = new List<Analysis>();

    [Display(Name = "Đánh giá cơ bản")]
    public virtual ICollection<Baselineassessment> Baselineassessments { get; set; } = new List<Baselineassessment>();

    [Display(Name = "Phòng ban")]
    public virtual Department? Department { get; set; }

    [Display(Name = "Công việc")]
    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    [Display(Name = "Vị trí")]
    public virtual Position? Position { get; set; }

    [Display(Name = "Người dùng")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}