using WorkTrackingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net.NetworkInformation;

namespace WorkTrackingSystem.Areas.ProjectManager.Controllers
{
    [Area("ProjectManager")]
    public class DashboardController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public DashboardController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            //// Tổng quan
            ViewBag.TotalJobs = _context.Jobs.Count(j => j.IsActive == true && j.IsDelete == false);
            ViewBag.CompletedJobs = _context.Scores
     .Include(s => s.JobMapEmployee)
     .ThenInclude(jme => jme.Job)
     .Count(s => s.Status == 1 && s.JobMapEmployee.Job.IsActive ==true && !s.JobMapEmployee.Job.IsDelete ==false);

            ViewBag.OverdueJobs = _context.Scores
     .Include(s => s.JobMapEmployee)
     .ThenInclude(jme => jme.Job)
     .Count(s => s.Status == 2 && s.JobMapEmployee.Job.IsActive == true && !s.JobMapEmployee.Job.IsDelete == false);

            ViewBag.TotalCategories = _context.Categories.Count(c => c.IsActive == true && c.IsDelete == false);

            //// Trạng thái công việc cho biểu đồ tròn
            ViewBag.JobStatusOntime = _context.Scores
     .Include(s => s.JobMapEmployee)
     .ThenInclude(jme => jme.Job).Count(s => s.Status == 1);
            ViewBag.JobStatusOverdue = _context.Scores
     .Include(s => s.JobMapEmployee)
     .ThenInclude(jme => jme.Job).Count(s => s.Status ==2);
            ViewBag.JobStatusLate = _context.Scores
     .Include(s => s.JobMapEmployee)
     .ThenInclude(jme => jme.Job).Count(s => s.Status == 3);
            ViewBag.JobStatusProcessing = _context.Scores
     .Include(s => s.JobMapEmployee)
     .ThenInclude(jme => jme.Job).Count(s => s.Status == 4);

            //// Thống kê công việc theo tháng/năm cho biểu đồ cột (client-side)
            var jobsByMonth = _context.Scores
    .Include(s => s.JobMapEmployee)
    .ThenInclude(jme => jme.Job)
    .Where(s => s.Time.HasValue && s.JobMapEmployee.Job.IsActive == true && !s.JobMapEmployee.Job.IsDelete ==false)
    .ToList()
    .GroupBy(s => s.Time.Value.ToString("MM/yyyy"))
    .Select(g => new
    {
        MonthYear = g.Key,
        TotalJobs = g.Count()
    })
    .OrderBy(g => g.MonthYear)
    .ToList();

            ViewBag.JobMonths = jobsByMonth.Select(j => j.MonthYear).ToList();
            ViewBag.JobCounts = jobsByMonth.Select(j => j.TotalJobs).ToList();


            //// Dữ liệu lịch (công việc theo ngày)
            var calendarJobs = _context.Scores
     .Include(s => s.JobMapEmployee)
     .ThenInclude(jme => jme.Job)
     .Where(s => s.Time.HasValue && s.JobMapEmployee.Job.IsActive == true && !s.JobMapEmployee.Job.IsDelete == false)
     .Select(s => new
     {
         Title = s.JobMapEmployee.Job.Name,
         Start = s.Time.Value.ToString("yyyy-MM-dd"),
         Status = s.Status
     })
     .ToList();

            ViewBag.CalendarJobs = Newtonsoft.Json.JsonConvert.SerializeObject(calendarJobs);


            return View();
        }
    }
}
////using System;
//using System.Collections.Generic;

//namespace WorkTrackingSystem.Models;

//public partial class Baselineassessment
//{
//    public long Id { get; set; }

//    public long? EmployeeId { get; set; }

//    public double? VolumeAssessment { get; set; }

//    public double? ProgressAssessment { get; set; }

//    public double? QualityAssessment { get; set; }

//    public double? SummaryOfReviews { get; set; }

//    public DateTime? Time { get; set; }

//    public bool? Evaluate { get; set; }

//    public bool? IsDelete { get; set; }

//    public bool? IsActive { get; set; }

//    public DateTime? CreateDate { get; set; }

//    public DateTime? UpdateDate { get; set; }

//    public string? CreateBy { get; set; }

//    public string? UpdateBy { get; set; }

//    public virtual Employee? Employee { get; set; }
//} using System;
//using System.Collections.Generic;

//namespace WorkTrackingSystem.Models;

//public partial class Analysis
//{
//    public long Id { get; set; }

//    public long? EmployeeId { get; set; }

//    public double? Total { get; set; }

//    public int? Ontime { get; set; }

//    public int? Late { get; set; }

//    public int? Overdue { get; set; }

//    public int? Processing { get; set; }

//    public DateTime? Time { get; set; }

//    public bool? IsDelete { get; set; }

//    public bool? IsActive { get; set; }

//    public DateTime? CreateDate { get; set; }

//    public DateTime? UpdateDate { get; set; }

//    public string? CreateBy { get; set; }

//    public string? UpdateBy { get; set; }

//    public virtual Employee? Employee { get; set; }
//} using System;
//using System.Collections.Generic;

//namespace WorkTrackingSystem.Models;

//public partial class Employee
//{
//    public long Id { get; set; }

//    public long? DepartmentId { get; set; }

//    public long? PositionId { get; set; }

//    public string Code { get; set; } = null!;

//    public string? FirstName { get; set; }

//    public string? LastName { get; set; }

//    public string? Gender { get; set; }

//    public DateOnly? Birthday { get; set; }

//    public string? Phone { get; set; }

//    public string? Email { get; set; }

//    public DateOnly? HireDate { get; set; }

//    public string? Address { get; set; }

//    public string? Avatar { get; set; }

//    public bool? IsDelete { get; set; }

//    public bool? IsActive { get; set; }

//    public DateTime? CreateDate { get; set; }

//    public DateTime? UpdateDate { get; set; }

//    public string? CreateBy { get; set; }

//    public string? UpdateBy { get; set; }

//    public virtual ICollection<Analysis> Analyses { get; set; } = new List<Analysis>();

//    public virtual ICollection<Baselineassessment> Baselineassessments { get; set; } = new List<Baselineassessment>();

//    public virtual Department? Department { get; set; }

//    public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

//    public virtual Position? Position { get; set; }

//    public virtual ICollection<User> Users { get; set; } = new List<User>();
//} using System;
//using System.Collections.Generic;

//namespace WorkTrackingSystem.Models;

//public partial class Department
//{
//    public long Id { get; set; }

//    public string Code { get; set; } = null!;

//    public string? Name { get; set; }

//    public string? Description { get; set; }

//    public bool? IsDelete { get; set; }

//    public bool? IsActive { get; set; }

//    public DateTime? CreateDate { get; set; }

//    public DateTime? UpdateDate { get; set; }

//    public string? CreateBy { get; set; }

//    public string? UpdateBy { get; set; }

//    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
//} using System;
//using System.Collections.Generic;

//namespace WorkTrackingSystem.Models;

//public partial class Job
//{
//    public long Id { get; set; }

//    public long? EmployeeId { get; set; }

//    public long? CategoryId { get; set; }

//    public string? Name { get; set; }

//    public string? Description { get; set; }

//    public DateOnly? Deadline1 { get; set; }

//    public DateOnly? Deadline2 { get; set; }

//    public DateOnly? Deadline3 { get; set; }

//    public DateOnly? CompletionDate { get; set; }

//    public byte? Status { get; set; }

//    public double? VolumeAssessment { get; set; }

//    public double? ProgressAssessment { get; set; }

//    public double? QualityAssessment { get; set; }

//    public double? SummaryOfReviews { get; set; }

//    public double? Progress { get; set; }

//    public DateTime? Time { get; set; }

//    public bool? IsDelete { get; set; }

//    public bool? IsActive { get; set; }
//    public DateTime? CreateDate { get; set; }

//    public DateTime? UpdateDate { get; set; }

//    public string? CreateBy { get; set; }
//    public string? UpdateBy { get; set; }

//    public virtual Category? Category { get; set; }

//    public virtual Employee? Employee { get; set; }
//} từ những dững liệu này của 1 phòng ban nào đó thống kê  cho tôi 1. Thống kê nhân sự
//Tổng số nhân viên trong phòng ban.
//Danh sách nhân viên(họ tên, mã nhân viên, chức vụ, số điện thoại, email...).
//Số nhân viên đang hoạt động(IsActive = true).
//Số nhân viên đã nghỉ việc hoặc không hoạt động(IsActive = false hoặc IsDelete = true).
//Phân bổ nhân sự theo chức vụ(Position).
//2. Thống kê công việc
//Tổng số công việc của phòng ban.
//Số lượng công việc hoàn thành đúng hạn, trễ hạn, đang xử lý.
//Tỷ lệ hoàn thành công việc của từng nhân viên trong phòng.
//Tổng số công việc theo từng loại (Category).
//Số lượng công việc theo các trạng thái (Status).
//3. Thống kê đánh giá hiệu suất
//Điểm đánh giá trung bình của nhân viên trong phòng (tổng hợp từ bảng Baselineassessment).
//Tổng hợp các tiêu chí đánh giá:
//Đánh giá khối lượng công việc(VolumeAssessment).
//Đánh giá tiến độ(ProgressAssessment).
//Đánh giá chất lượng công việc(QualityAssessment).
//Tổng điểm đánh giá(SummaryOfReviews).
//Danh sách nhân viên có hiệu suất cao/thấp.
//Tỷ lệ nhân viên đạt yêu cầu hoặc không đạt yêu cầu(Evaluate).
//4. Thống kê phân tích hiệu suất
//Tổng số công việc đã xử lý của phòng ban(Total từ bảng Analysis).
//Số công việc hoàn thành đúng hạn(Ontime).
//Số công việc trễ hạn(Late).
//Số công việc quá hạn(Overdue).
//Số công việc đang xử lý(Processing).
//So sánh hiệu suất giữa các nhân viên.và 2. Biểu đồ thống kê công việc
//🔹 Biểu đồ cột: Số lượng công việc theo trạng thái
//X-Axis: Trạng thái công việc (Đang xử lý, Hoàn thành, Trễ hạn, Quá hạn)
//Y-Axis: Số lượng công việc
//Mục đích: Kiểm tra khối lượng công việc và tỷ lệ hoàn thành.
//🔹 Biểu đồ đường: Tiến độ hoàn thành công việc theo thời gian
//X-Axis: Ngày/tháng
//Y-Axis: Số lượng công việc hoàn thành
//Mục đích: Xem xu hướng hoàn thành công việc theo thời gian.
//🔹 Biểu đồ cột nhóm: Số lượng công việc theo từng nhân viên
//X-Axis: Tên nhân viên
//Y-Axis: Số lượng công việc (Hoàn thành, Đang làm, Quá hạn)
//Mục đích: So sánh khối lượng công việc giữa các nhân viên.
//3. Biểu đồ thống kê đánh giá hiệu suất
//🔹 Biểu đồ radar: Đánh giá hiệu suất của từng nhân viên
//Các trục:
//Khối lượng công việc (VolumeAssessment)
//Tiến độ (ProgressAssessment)
//Chất lượng (QualityAssessment)
//Tổng điểm đánh giá (SummaryOfReviews)
//Mục đích: Đánh giá toàn diện hiệu suất của nhân viên theo nhiều tiêu chí.
//🔹 Biểu đồ cột: So sánh điểm đánh giá giữa các nhân viên
//X-Axis: Tên nhân viên
//Y-Axis: Điểm đánh giá tổng hợp (SummaryOfReviews)
//Mục đích: Xác định nhân viên có hiệu suất cao nhất/thấp nhất.
//4. Biểu đồ thống kê phân tích hiệu suất công việc
//🔹 Biểu đồ cột: Số lượng công việc hoàn thành đúng hạn, trễ hạn, quá hạn
//X-Axis: Tên nhân viên
//Y-Axis: Số lượng công việc
//Mục đích: So sánh năng suất làm việc của từng nhân viên.
//🔹 Biểu đồ tròn: Tỷ lệ công việc đúng hạn vs trễ hạn
//Phần 1: Công việc đúng hạn (Ontime)
//Phần 2: Công việc trễ hạn (Late)
//Phần 3: Công việc quá hạn (Overdue)
//Mục đích: Đánh giá mức độ tuân thủ deadline trong phòng ban. các biểu đồ theo thời gian có thể  lọc các chức năng làm vào đây cho tôi using WorkTrackingSystem.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Linq;

//namespace WorkTrackingSystem.Areas.ProjectManager.Controllers
//{
//    [Area("ProjectManager")]
//    public class DashboardController : BaseController
//    {
//        private readonly WorkTrackingSystemContext _context;

//        public DashboardController(WorkTrackingSystemContext context)
//        {
//            _context = context;
//        }

//        public IActionResult Index()
//        {

//            return View();
//        }
//    }
//} //