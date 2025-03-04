using WorkTrackingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WorkTrackingSystem.Areas.HRManager.Controllers {

    public class DashboardController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public DashboardController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalEmployees = _context.Employees.Count(e => e.IsActive == true && e.IsDelete == false);
            ViewBag.TotalDepartments = _context.Departments.Count(d => d.IsActive == true && d.IsDelete == false);
            ViewBag.TotalCompletedJobs = _context.Jobs.Count(j => j.Status == 1 && j.IsActive == true && j.IsDelete == false);
            ViewBag.TotalEvaluated = _context.Baselineassessments.Count(b => b.Evaluate == true && b.IsActive == true && b.IsDelete == false);

            // Trạng thái công việc
            ViewBag.JobStatusOntime = _context.Jobs.Count(j => j.Status == 1);
            ViewBag.JobStatusOverdue = _context.Jobs.Count(j => j.Status == 2);
            ViewBag.JobStatusLate = _context.Jobs.Count(j => j.Status == 3);
            ViewBag.JobStatusProcessing = _context.Jobs.Count(j => j.Status == 4);

            // Danh sách phòng ban
            ViewBag.Departments = _context.Departments
                .Include(d => d.Employees)
                .Where(d => d.IsActive == true && d.IsDelete == false)
                .ToList();

            // Nhân viên gần đây
            ViewBag.RecentEmployees = _context.Employees
                .Where(e => e.IsActive == true && e.IsDelete == false)
                .OrderByDescending(e => e.CreateDate)
                .Take(5)
                .ToList();

            // Thống kê đánh giá cơ bản theo tháng (chuyển sang client-side)
            var assessmentsByMonth = _context.Baselineassessments
                .Where(b => b.Time.HasValue)
                .ToList() // Chuyển sang client-side
                .GroupBy(b => b.Time.Value.ToString("MM/yyyy"))
                .Select(g => new
                {
                    Month = g.Key,
                    AvgVolume = g.Average(b => b.VolumeAssessment ?? 0),
                    AvgProgress = g.Average(b => b.ProgressAssessment ?? 0),
                    AvgQuality = g.Average(b => b.QualityAssessment ?? 0)
                })
                .OrderBy(g => g.Month)
                .ToList();
            //        var assessmentsByMonth = _context.Baselineassessments
            //.Where(b => b.Time.HasValue)
            //.GroupBy(b => new { Year = b.Time.Value.Year, Month = b.Time.Value.Month }) // Group theo số nguyên
            //.Select(g => new
            //{
            //    Month = g.Key.Month + "-" + g.Key.Year, // Chuyển đổi sang chuỗi ở bước Select
            //    AvgVolume = g.Average(b => b.VolumeAssessment ?? 0),
            //    AvgProgress = g.Average(b => b.ProgressAssessment ?? 0),
            //    AvgQuality = g.Average(b => b.QualityAssessment ?? 0)
            //})
            //.OrderBy(g => g.Month)
            //.ToList();

            ViewBag.AssessmentMonths = assessmentsByMonth.Select(a => a.Month).ToList();
            ViewBag.AssessmentVolumes = assessmentsByMonth.Select(a => a.AvgVolume).ToList();
            ViewBag.AssessmentProgress = assessmentsByMonth.Select(a => a.AvgProgress).ToList();
            ViewBag.AssessmentQualities = assessmentsByMonth.Select(a => a.AvgQuality).ToList();

            return View();
        }
    }

}
