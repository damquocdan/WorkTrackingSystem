using WorkTrackingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WorkTrackingSystem.Areas.EmployeeSystem.Controllers {

    public class DashboardController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public DashboardController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index", "Login");
            }
            long id = long.Parse(userId);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            //var employee = await _context.Employees.Include(e=>e.Jobs).FirstOrDefaultAsync(e => e.Id == user.EmployeeId);
            //var jobs = await _context.Employees.Include(e => e.Jobs).FirstOrDefaultAsync(e => e.Id == user.EmployeeId);
            ViewBag.TotalJobs = _context.Jobs.Where(x=> x.EmployeeId == user.EmployeeId).Count(j => j.IsActive == true && j.IsDelete == false);
            ViewBag.CompletedJobs = _context.Jobs.Where(x => x.EmployeeId == user.EmployeeId).Count(j => j.Status == 1 && j.IsActive == true && j.IsDelete == false);
            ViewBag.OverdueJobs = _context.Jobs.Where(x => x.EmployeeId == user.EmployeeId).Count(j => j.Status == 2 && j.IsActive == true && j.IsDelete == false);
            ViewBag.TotalCategories = _context.Categories.Count(c => c.IsActive == true && c.IsDelete == false);

            // Trạng thái công việc cho biểu đồ tròn
            ViewBag.JobStatusOntime = _context.Jobs.Where(x => x.EmployeeId == user.EmployeeId).Count(j => j.Status == 1);
            ViewBag.JobStatusOverdue = _context.Jobs.Where(x => x.EmployeeId == user.EmployeeId).Count(j => j.Status == 2);
            ViewBag.JobStatusLate = _context.Jobs.Where(x => x.EmployeeId == user.EmployeeId).Count(j => j.Status == 3);
            ViewBag.JobStatusProcessing = _context.Jobs.Where(x => x.EmployeeId == user.EmployeeId).Count(j => j.Status == 4);

            // Thống kê công việc theo tháng/năm cho biểu đồ cột (client-side)
            var jobsByMonth = _context.Jobs.Where(x => x.EmployeeId == user.EmployeeId)
                .Where(j => j.Time.HasValue && j.IsActive == true && j.IsDelete == false)
                .ToList() // Chuyển sang client-side
                .GroupBy(j => j.Time.Value.ToString("MM/yyyy"))
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
