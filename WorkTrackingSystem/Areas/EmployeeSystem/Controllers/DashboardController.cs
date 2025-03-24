using WorkTrackingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WorkTrackingSystem.Areas.EmployeeSystem.Controllers
{

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
			ViewBag.TotalJobs = _context.Jobmapemployees.Include(x => x.Job).Where(x => x.EmployeeId == user.EmployeeId).Count(x => x.IsActive == true && x.IsDelete == false);

			//ViewBag.TotalJobs = _context.Jobs.Where(x=> x.EmployeeId == user.EmployeeId).Count(j => j.IsActive == true && j.IsDelete == false);
			ViewBag.CompletedJobs = _context.Jobmapemployees.Include(x => x.Job).Include(x => x.Scores).Where(x => x.EmployeeId == user.EmployeeId).Count(x => x.Scores.Any(s => s.Status == 1) && x.IsActive == true && x.IsDelete == false);
			ViewBag.OverdueJobs = _context.Jobmapemployees.Include(x => x.Job).Where(x => x.EmployeeId == user.EmployeeId).Count(x => x.Scores.Any(s => s.Status == 2) && x.IsActive == true && x.IsDelete == false);
			ViewBag.TotalCategories = _context.Categories.Count(c => c.IsActive == true && c.IsDelete == false);

			// Trạng thái công việc cho biểu đồ tròn
			ViewBag.JobStatusOntime = _context.Jobmapemployees.Include(x => x.Job).Where(x => x.EmployeeId == user.EmployeeId).Count(x => x.Scores.Any(s => s.Status == 1));
			ViewBag.JobStatusOverdue = _context.Jobmapemployees.Include(x => x.Job).Where(x => x.EmployeeId == user.EmployeeId).Count(x => x.Scores.Any(s => s.Status == 2));
			ViewBag.JobStatusLate = _context.Jobmapemployees.Include(x => x.Job).Where(x => x.EmployeeId == user.EmployeeId).Count(x => x.Scores.Any(s => s.Status == 3));
			ViewBag.JobStatusProcessing = _context.Jobmapemployees.Include(x => x.Job).Where(x => x.EmployeeId == user.EmployeeId).Count(x => x.Scores.Any(s => s.Status == 4));

			//Thống kê công việc theo tháng/ năm cho biểu đồ cột(client-side)
			var jobsByMonth = _context.Jobmapemployees.Include(x => x.Job).Where(x => x.EmployeeId == user.EmployeeId)
				.Where(x => x.Scores.Any(s => s.CreateDate.HasValue) && x.IsActive == true && x.IsDelete == false)
				.ToList() // Chuyển sang client-side
				 .SelectMany(x => x.Scores) // Lấy từng Score từ Jobmapemployees
			   .Where(s => s.CreateDate.HasValue) // Chỉ lấy những Score có Time
				.GroupBy(s => s.CreateDate.Value.ToString("MM/yyyy")) // Nhóm theo tháng/năm
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
	.Include(s => s.JobMapEmployee) // Kết nối với JobMapEmployee
		.ThenInclude(jm => jm.Job) // Kết nối tiếp đến bảng Job
	.Where(s => s.Time.HasValue && s.IsActive == true && s.IsDelete == false)
	.Select(s => new
	{
		Title = s.JobMapEmployee.Job.Name, // Lấy tên công việc từ bảng Job
		Start = s.Time.Value.ToString("yyyy-MM-dd"), // Lấy thời gian từ bảng Score
		Status = s.Status // Lấy trạng thái từ bảng Score
	})
	.ToList();

			ViewBag.CalendarJobs = Newtonsoft.Json.JsonConvert.SerializeObject(calendarJobs);

			return View();
		}
	}

}
