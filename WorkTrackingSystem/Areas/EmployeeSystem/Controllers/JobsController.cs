using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using WorkTrackingSystem.Areas.EmployeeSystem.Models;
using WorkTrackingSystem.Models;
using X.PagedList.Extensions;

namespace WorkTrackingSystem.Areas.EmployeeSystem.Controllers
{
	[Area("EmployeeSystem")]
	public class JobsController : BaseController
	{
		private readonly WorkTrackingSystemContext _context;

		public JobsController(WorkTrackingSystemContext context)
		{
			_context = context;
		}

		// GET: EmployeeSystem/Jobs
		public async Task<IActionResult> Index(int? page , string? selectedMonth = null
			, DateTime? startDate = null, DateTime? endDate = null,
			string? searchTerm= null,string? filterStatus = null
            , DateTime? deadlineStartDate = null, DateTime? deadlineEndDate = null)
		{
			Console.WriteLine("SearchTerm: " + searchTerm);
			int limit = 9;
			if(page == null)
			{
				page = 1;
			}
			
			var pageIndex= page.HasValue ? Convert.ToInt32(page) : 1;
			var sessionUserId = HttpContext.Session.GetString("UserId");

			if (string.IsNullOrEmpty(sessionUserId))
			{
				return RedirectToAction("Login", "Account");
			}

			if (!long.TryParse(sessionUserId, out long userId))
			{
				return BadRequest("UserId trong session không hợp lệ.");
			}

			var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
			if (user == null || user.EmployeeId == null)
			{
				return NotFound("Không tìm thấy nhân viên tương ứng.");
			}

			// Lấy danh sách công việc
			var jobs = _context.Jobs
				.Include(j => j.Category)
				.Include(j => j.Employee)
				.Where(j => j.EmployeeId == user.EmployeeId);
			if (!string.IsNullOrEmpty(searchTerm))
			{
				jobs = jobs.Where(j => j.Name.Contains(searchTerm));
			}
			if (!string.IsNullOrEmpty(filterStatus))
			{
				int statusValue;
				if (int.TryParse(filterStatus, out statusValue))
				{
					jobs = jobs.Where(j => j.Status == statusValue);
				}
			}
			// Nếu có giá trị tháng, lọc theo tháng
			if (!string.IsNullOrEmpty(selectedMonth))
			{
				if (DateOnly.TryParseExact(selectedMonth + "-01", "yyyy-MM-dd", out var startOfMonth))
				{
					var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
					jobs = jobs.Where(j => j.Deadline1 >= startOfMonth && j.Deadline1 <= endOfMonth);
				}
			}

			// Nếu có bộ lọc trạng thái, lọc theo trạng thái
			if (!string.IsNullOrEmpty(filterStatus) && int.TryParse(filterStatus, out int status))
			{
				jobs = jobs.Where(j => j.Status == status);
			}

			// Lọc theo khoảng thời gian hoàn thành công việc
			if (startDate.HasValue || endDate.HasValue)
			{
				jobs = jobs.Where(j => j.CompletionDate.HasValue &&
									  (!startDate.HasValue || j.CompletionDate.Value >= DateOnly.FromDateTime(startDate.Value)) &&
									  (!endDate.HasValue || j.CompletionDate.Value <= DateOnly.FromDateTime(endDate.Value)));
			}
            if (deadlineStartDate.HasValue || deadlineEndDate.HasValue)
            {
                jobs = jobs.Where(j => j.Deadline1.HasValue &&
                                      (!deadlineStartDate.HasValue || j.Deadline1.Value >= DateOnly.FromDateTime(deadlineStartDate.Value)) &&
                                      (!deadlineEndDate.HasValue || j.Deadline1.Value <= DateOnly.FromDateTime(deadlineEndDate.Value)));
            }

            var jobList = await jobs.OrderBy(j => j.Deadline1).ToListAsync();
			

			var pagedJobs = jobList.ToPagedList(pageIndex, limit);

			// Lưu lại giá trị để hiển thị lại trên View
			ViewBag.SelectedMonth = selectedMonth;
			ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
			ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.deadlineStartDate = deadlineStartDate?.ToString("yyyy-MM-dd");
            ViewBag.deadlineEndDate = deadlineEndDate?.ToString("yyyy-MM-dd");
            ViewBag.SearchTerm = searchTerm;
			ViewBag.FilterStatus = filterStatus;
			return View(pagedJobs);
		}
		[HttpPost]
		public async Task<IActionResult> UpdateProgress([FromBody] UpdateProgressRequest request)
		{
			if (request == null || request.Id <= 0)
			{
				return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });
			}

			var job = await _context.Jobs.FindAsync(request.Id);
			if (job == null)
			{
				return NotFound(new { success = false, message = "Không tìm thấy công việc" });
			}

			// Nếu Progress là null, gán thành 0
			job.Progress ??= 0;
			job.Status ??= 0;
			// Lấy thời gian hiện tại để so sánh với deadline
			var today = DateOnly.FromDateTime(DateTime.Now);
			bool isPastDeadline = job.Deadline1.HasValue && job.Deadline1.Value < today;

			// Cập nhật tiến độ
			job.Progress = request.Progress;

			// Cập nhật trạng thái dựa trên tiến độ và deadline
			if (job.Progress == 100 && !isPastDeadline)
			{
				job.Status = 1; // Hoàn thành đúng hạn
				job.CompletionDate = today;
			}
			else if (isPastDeadline && job.Progress < 100)
			{
				job.Status = 2; // Chưa hoàn thành (quá hạn mà chưa đạt 100%)
				job.CompletionDate = null;
			}
			else if (isPastDeadline && job.Progress == 100)
			{
				job.Status = 3; // Hoàn thành muộn (quá hạn nhưng đã đạt 100%)
				job.CompletionDate = today;
			}
			else if (!isPastDeadline && job.Progress > 0 && job.Progress < 100)
			{
				job.Status = 4; // Đang xử lý (chưa đến hạn nhưng tiến độ > 0)
				job.CompletionDate = null;
			}
			else if (job.Progress == 0)
			{
				job.Status = 0; // Chưa bắt đầu
				job.CompletionDate = null;

			}


			_context.Entry(job).State = EntityState.Modified;
			await UpdateAnalysis(job.EmployeeId);
			await _context.SaveChangesAsync();

			return Ok(new { success = true, newStatus = job.Status, completionDate = job.CompletionDate?.ToString("dd/MM/yyyy") });
		}

		private async Task UpdateAnalysis(long? employeeId)
		{
			if (employeeId == null)
				return;

			var currentMonth = DateTime.Now.Month;
			var currentYear = DateTime.Now.Year;

			// Lấy danh sách công việc của nhân viên trong tháng hiện tại
			var jobs = await _context.Jobs
				.Where(j => j.EmployeeId == employeeId && j.Time.HasValue &&
							j.Time.Value.Month == currentMonth && j.Time.Value.Year == currentYear)
				.ToListAsync();


			int ontime = jobs.Count(j => j.Status == 1);
			int late = jobs.Count(j => j.Status == 2);
			int overdue = jobs.Count(j => j.Status == 3);
			int processing = jobs.Count(j => j.Status == 4);
			int total = ontime + late + overdue + processing;
			// Tính trung bình đánh giá của nhân viên
			var averageReview = jobs.Any()
				? jobs.Average(j => j.SummaryOfReviews ?? 0)
				: 0;

			// Tìm bản ghi Analysis của nhân viên trong tháng hiện tại
			var analysis = await _context.Analyses
				.FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Time.HasValue &&
										  a.Time.Value.Month == currentMonth && a.Time.Value.Year == currentYear);

			if (analysis == null)
			{
				// Nếu chưa có bản ghi trong tháng, tạo mới
				analysis = new Analysis
				{
					EmployeeId = employeeId,
					Total = total,
					Ontime = ontime,
					Late = late,
					Overdue = overdue,
					Processing = processing,
					Time = new DateTime(currentYear, currentMonth, 1),
					CreateDate = DateTime.Now,
					UpdateDate = DateTime.Now
				};
				_context.Analyses.Add(analysis);
			}
			else
			{
				// Nếu đã có bản ghi trong tháng, cập nhật dữ liệu
				analysis.Total = total;
				analysis.Ontime = ontime;
				analysis.Late = late;
				analysis.Overdue = overdue;
				analysis.Processing = processing;
				analysis.UpdateDate = DateTime.Now;
			}

			await _context.SaveChangesAsync();
		}
		private bool JobExists(long id)
		{
			return _context.Jobs.Any(e => e.Id == id);
		}

	}
}
