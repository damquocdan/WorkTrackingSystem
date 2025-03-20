using WorkTrackingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WorkTrackingSystem.Areas.AdminSystem.Controllers {

    public class DashboardController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public DashboardController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

		public async Task<IActionResult> Index()
		{
			// 1️⃣ Thống kê tổng số công việc và danh mục
			ViewBag.TotalJobs = await _context.Jobs.CountAsync(j => j.IsActive == true && j.IsDelete==false);
			ViewBag.TotalCategories = await _context.Categories.CountAsync(c => c.IsActive == true && c.IsDelete == false);

			// 2️⃣ Đếm số công việc hoàn thành, trễ hạn từ bảng `Score`
			ViewBag.CompletedJobs = await _context.Scores
				.CountAsync(s => s.Status == 1 && s.IsActive == true && s.IsDelete == false);
			ViewBag.OverdueJobs = await _context.Scores
				.CountAsync(s => s.Status == 2 && s.IsActive == true && s.IsDelete == false);

			// 3️⃣ Trạng thái công việc (biểu đồ tròn)
			ViewBag.JobStatusOntime = await _context.Scores.CountAsync(s => s.Status == 1);
			ViewBag.JobStatusOverdue = await _context.Scores.CountAsync(s => s.Status == 2);
			ViewBag.JobStatusLate = await _context.Scores.CountAsync(s => s.Status == 3);
			ViewBag.JobStatusProcessing = await _context.Scores.CountAsync(s => s.Status == 4);

            // 4️⃣ Thống kê công việc theo tháng/năm (biểu đồ cột)
            // 4️⃣ Thống kê công việc theo tháng/năm (biểu đồ cột)
            var jobsByMonth = await _context.Scores
                .Where(s => s.Time.HasValue && s.IsActive == true && s.IsDelete == false)
                .GroupBy(s => new { Year = s.Time.Value.Year, Month = s.Time.Value.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalJobs = g.Count()
                })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ToListAsync();

            // Chuyển đổi định dạng MM/yyyy
            var formattedJobsByMonth = jobsByMonth
                .Select(j => new {
                    MonthYear = $"{j.Month:D2}/{j.Year}", // Định dạng MM/yyyy
                    TotalJobs = j.TotalJobs
                }).ToList();

            // Kiểm tra kết quả trong Console
            Console.WriteLine("=== Jobs By Month ===");
            foreach (var item in formattedJobsByMonth)
            {
                Console.WriteLine($"{item.MonthYear}: {item.TotalJobs} jobs");
            }

            // Truyền vào ViewBag
            ViewBag.JobMonths = formattedJobsByMonth.Select(j => j.MonthYear).ToList();
            ViewBag.JobCounts = formattedJobsByMonth.Select(j => j.TotalJobs).ToList();

            // 5️⃣ Dữ liệu lịch (công việc theo ngày)
            var calendarJobs = await _context.Scores
				.Include(s => s.JobMapEmployee)
					.ThenInclude(jme => jme.Job) // Lấy Job từ JobMapEmployee
				.Where(s => s.Time.HasValue && s.IsActive == true && s.IsDelete == false)
				.Select(s => new
				{
					Title = s.JobMapEmployee.Job.Name,
					Start = s.Time.Value.ToString("yyyy-MM-dd"),
					Status = s.Status
				})
				.ToListAsync();

			ViewBag.CalendarJobs = Newtonsoft.Json.JsonConvert.SerializeObject(calendarJobs);

			return View();
		}

	}

}
