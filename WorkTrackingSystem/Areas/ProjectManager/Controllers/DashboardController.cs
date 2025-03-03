using WorkTrackingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

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
            // Tổng quan
            ViewBag.TotalJobs = _context.Jobs.Count(j => j.IsActive == true && j.IsDelete == false);
            ViewBag.CompletedJobs = _context.Jobs.Count(j => j.Status == 1 && j.IsActive == true && j.IsDelete == false);
            ViewBag.OverdueJobs = _context.Jobs.Count(j => j.Status == 2 && j.IsActive == true && j.IsDelete == false);
            ViewBag.TotalCategories = _context.Categories.Count(c => c.IsActive == true && c.IsDelete == false);

            // Trạng thái công việc cho biểu đồ tròn
            ViewBag.JobStatusOntime = _context.Jobs.Count(j => j.Status == 1);
            ViewBag.JobStatusOverdue = _context.Jobs.Count(j => j.Status == 2);
            ViewBag.JobStatusLate = _context.Jobs.Count(j => j.Status == 3);
            ViewBag.JobStatusProcessing = _context.Jobs.Count(j => j.Status == 4);

            // Thống kê công việc theo tháng/năm cho biểu đồ cột (client-side)
            //var jobsByMonth = _context.Jobs
            //    .Where(j => j.Time.HasValue && j.IsActive == true && j.IsDelete == false)
            //    .ToList() // Chuyển sang client-side
            //    .GroupBy(j => j.Time.Value.ToString("MM/yyyy"))
            //    .Select(g => new
            //    {
            //        MonthYear = g.Key,
            //        TotalJobs = g.Count()
            //    })
            //    .OrderBy(g => g.MonthYear)
            //    .ToList();
            var jobsByMonth = _context.Jobs
                 .Where(j => j.CreateDate.HasValue && j.IsActive == true && j.IsDelete == false)
                 .GroupBy(j => new { j.CreateDate.Value.Year, j.CreateDate.Value.Month }) // Group theo số nguyên
                 .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month) // Sắp xếp theo thời gian thực tế
                 .Select(g => new
                 {
                     Month = $"{g.Key.Month}/{g.Key.Year}", // Chuyển sang chuỗi chỉ sau khi lấy dữ liệu
                     TotalJobs = g.Count()
                 })
                 .ToList(); // ToList() sau cùng để tối ưu

            ViewBag.JobMonths = jobsByMonth.Select(j => j.Month).ToList();
            ViewBag.JobCounts = jobsByMonth.Select(j => j.TotalJobs).ToList();

            // Dữ liệu lịch (công việc theo ngày)
            var calendarJobs = _context.Jobs
                .Where(j => j.Time.HasValue && j.IsActive == true && j.IsDelete == false)
                .Select(j => new
                {
                    Title = j.Name,
                    Start = j.Time.Value.ToString("yyyy-MM-dd"),
                    Status = j.Status
                })
                .ToList();

            ViewBag.CalendarJobs = Newtonsoft.Json.JsonConvert.SerializeObject(calendarJobs);

            return View();
        }
    }
}