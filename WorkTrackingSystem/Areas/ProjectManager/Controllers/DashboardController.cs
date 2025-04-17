using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using WorkTrackingSystem.Models;
using Newtonsoft.Json;

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
            ViewBag.CompletedJobs = _context.Scores
                .Include(s => s.JobMapEmployee)
                .ThenInclude(jme => jme.Job)
                .Count(s => s.Status == 1 && s.JobMapEmployee.Job.IsActive == true && s.JobMapEmployee.Job.IsDelete == false);

            ViewBag.OverdueJobs = _context.Scores
                .Include(s => s.JobMapEmployee)
                .ThenInclude(jme => jme.Job)
                .Count(s => s.Status == 2 && s.JobMapEmployee.Job.IsActive == true && s.JobMapEmployee.Job.IsDelete == false);

            ViewBag.TotalCategories = _context.Categories.Count(c => c.IsActive == true && c.IsDelete == false);

            // Trạng thái công việc cho biểu đồ tròn
            ViewBag.JobStatusOntime = _context.Scores
                .Include(s => s.JobMapEmployee)
                .ThenInclude(jme => jme.Job)
                .Count(s => s.Status == 1);
            ViewBag.JobStatusOverdue = _context.Scores
                .Include(s => s.JobMapEmployee)
                .ThenInclude(jme => jme.Job)
                .Count(s => s.Status == 2);
            ViewBag.JobStatusLate = _context.Scores
                .Include(s => s.JobMapEmployee)
                .ThenInclude(jme => jme.Job)
                .Count(s => s.Status == 3);
            ViewBag.JobStatusProcessing = _context.Scores
                .Include(s => s.JobMapEmployee)
                .ThenInclude(jme => jme.Job)
                .Count(s => s.Status == 4);

            // Thống kê công việc theo tháng/năm cho biểu đồ cột
            var jobsByMonth = _context.Scores
                .Include(s => s.JobMapEmployee)
                .ThenInclude(jme => jme.Job)
                .Where(s => s.Time.HasValue && s.JobMapEmployee.Job.IsActive == true && s.JobMapEmployee.Job.IsDelete == false)
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

            // Dữ liệu lịch (công việc theo ngày)
            var calendarJobs = _context.Scores
                .Include(s => s.JobMapEmployee)
                .ThenInclude(jme => jme.Job)
                .Where(s => s.Time.HasValue && s.JobMapEmployee.Job.IsActive == true && s.JobMapEmployee.Job.IsDelete == false)
                .Select(s => new
                {
                    Title = s.JobMapEmployee.Job.Name,
                    Start = s.Time.Value.ToString("yyyy-MM-dd"),
                    Status = s.Status
                })
                .ToList();

            ViewBag.CalendarJobs = JsonConvert.SerializeObject(calendarJobs);

            // Dữ liệu cho biểu đồ đánh giá và tóm tắt công việc
            var assessments = _context.Baselineassessments
                .Include(a => a.Employee)
                .Where(a => a.Time.HasValue && a.IsActive == true && a.IsDelete == false)
                .ToList();

            var groupedData = assessments
                .GroupBy(x => x.Time.Value.ToString("MM/yyyy"))
                .Select(g => new
                {
                    MonthYear = g.Key,
                    SumVolume = g.Sum(x => x.VolumeAssessment ?? 0),
                    SumProgress = g.Sum(x => x.ProgressAssessment ?? 0),
                    SumQuality = g.Sum(x => x.QualityAssessment ?? 0),
                    SumSummary = g.Sum(x => x.SummaryOfReviews ?? 0)
                })
                .OrderBy(g => g.MonthYear)
                .ToList();

            // Prepare chart data
            ViewBag.Labels = groupedData.Select(g => g.MonthYear).ToList();
            ViewBag.SumVolume = groupedData.Select(g => g.SumVolume).ToList();
            ViewBag.SumProgress = groupedData.Select(g => g.SumProgress).ToList();
            ViewBag.SumQuality = groupedData.Select(g => g.SumQuality).ToList();
            ViewBag.SumSummary = groupedData.Select(g => g.SumSummary).ToList();

            ViewBag.Date1 = null; // Set to null or based on filtering logic
            ViewBag.Date2 = null; // Set to null or based on filtering logic

            return View();
        }
    }
}