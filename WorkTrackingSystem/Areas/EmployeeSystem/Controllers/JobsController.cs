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

using X.PagedList;
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
		public async Task<IActionResult> Index(int? page, string? selectedMonth = null,
	DateTime? startDate = null, DateTime? endDate = null,
	string? search=null, string? filterStatus = null,
	DateTime? deadlineStartDate = null, DateTime? deadlineEndDate = null)
		{
			int limit = 9;
			int pageIndex = page ?? 1;
			
			var sessionUserId = HttpContext.Session.GetString("UserId");
			if (string.IsNullOrEmpty(sessionUserId) || !long.TryParse(sessionUserId, out long userId))
			{
				return RedirectToAction("Login", "Account");
			}

			var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
			if (user?.EmployeeId == null)
			{
				return NotFound("Không tìm thấy nhân viên tương ứng.");
			}

			//var jobs = _context.Jobmapemployees
			//	.Include(jm => jm.Job)
			//		.ThenInclude(j => j.Category)
			//	.Include(jm => jm.Employee)
			//	.Include(jm => jm.Scores)
			//	.Where(jm => jm.EmployeeId == user.EmployeeId && jm.IsActive == true && jm.IsDelete == false);
			var jobs = _context.Scores
								.Include(s => s.JobMapEmployee)
									.ThenInclude(jm => jm.Job)
										.ThenInclude(j => j.Category)
								.Include(s => s.JobMapEmployee.Employee)
								.Where(s => s.JobMapEmployee.EmployeeId == user.EmployeeId && s.IsActive == true && s.IsDelete == false);
			if (!string.IsNullOrEmpty(search))
			{
				jobs = jobs.Where(jm => jm.JobMapEmployee.Job.Name.Contains(search));
			}

			if (filterStatus != null && int.TryParse(filterStatus, out int statusValue))
			{
				jobs = jobs.Where(s => s.Status == statusValue);
			}

			if (!string.IsNullOrEmpty(selectedMonth) &&
				DateTime.TryParseExact(selectedMonth + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var startOfMonth))
			{
				var startOfMonthDateOnly = DateOnly.FromDateTime(startOfMonth);
				var endOfMonthDateOnly = startOfMonthDateOnly.AddMonths(1).AddDays(-1);
				jobs = jobs.Where(s => s.CompletionDate.HasValue &&
														   s.CompletionDate.Value >= startOfMonthDateOnly &&
														   s.CompletionDate.Value <= endOfMonthDateOnly);
			}
			if (startDate.HasValue || endDate.HasValue)
			{
				jobs = jobs.Where(j => j.CompletionDate.HasValue &&
									  (!startDate.HasValue || j.CompletionDate.Value >= DateOnly.FromDateTime(startDate.Value)) &&
									  (!endDate.HasValue || j.CompletionDate.Value <= DateOnly.FromDateTime(endDate.Value)));
			}
			if (deadlineStartDate.HasValue || deadlineEndDate.HasValue)
			{
				jobs = jobs.Where(j => j.JobMapEmployee.Job.Deadline1.HasValue &&
									  (!deadlineStartDate.HasValue || j.JobMapEmployee.Job.Deadline1.Value >= DateOnly.FromDateTime(deadlineStartDate.Value)) &&
									  (!deadlineEndDate.HasValue || j.JobMapEmployee.Job.Deadline1.Value <= DateOnly.FromDateTime(deadlineEndDate.Value)));
			}
			var jobList = await jobs.OrderBy(s => s.JobMapEmployee.Job.Deadline1)
									.Select(s => new JobViewModel
									{
										JobId = s.JobMapEmployee.Job.Id,
										JobName = s.JobMapEmployee.Job.Name,
										CategoryName = s.JobMapEmployee.Job.Category.Name,
										EmployeeName = s.JobMapEmployee.Employee.FirstName + " " + s.JobMapEmployee.Employee.LastName,
										ScoreStatus = s.Status ?? 0,
										CompletionDate = s.CompletionDate,
										Deadline = s.JobMapEmployee.Job.Deadline1,
										Progress = s.Progress ?? 0
									})
									.ToListAsync();

			var pagedJobs = jobList.Any() ? jobList.ToPagedList(pageIndex, limit) : new PagedList<JobViewModel>(new List<JobViewModel>(), pageIndex, limit);

			ViewBag.SelectedMonth = selectedMonth;
			ViewBag.Search = search;
			ViewBag.FilterStatus = filterStatus;
			ViewBag.deadlineStartDate = deadlineStartDate?.ToString("yyyy-MM-dd");
			ViewBag.deadlineEndDate = deadlineEndDate?.ToString("yyyy-MM-dd");
			ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
			ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
			return View(pagedJobs);
		}

		[HttpPost]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> UpdateProgress([FromBody] UpdateProgressRequest request)
		{
			if (request == null || request.Id <= 0)
			{
				return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });
			}

			// Tìm JobMapEmployee liên quan đến công việc
			var jobMapEmployee = await _context.Jobmapemployees
				.Include(jm => jm.Job)
				.FirstOrDefaultAsync(jm => jm.JobId == request.Id && jm.IsActive == true && jm.IsDelete == false);

			if (jobMapEmployee == null || jobMapEmployee.Job == null)
			{
				return NotFound(new { success = false, message = "Không tìm thấy công việc" });
			}

			var job = jobMapEmployee.Job;

			// Tìm bản ghi Score liên quan đến công việc
			var score = await _context.Scores
				.FirstOrDefaultAsync(s => s.JobMapEmployeeId == jobMapEmployee.Id && s.IsActive == true && s.IsDelete == false);

			if (score == null)
			{
				return NotFound(new { success = false, message = "Không tìm thấy dữ liệu đánh giá công việc" });
			}

			var today = DateOnly.FromDateTime(DateTime.Now);
			bool isPastDeadline = job.Deadline1.HasValue && job.Deadline1.Value < today;

			// Cập nhật tiến độ công việc
			score.Progress = request.Progress;

			// Cập nhật trạng thái dựa trên tiến độ và deadline
			if (score.Progress == 100 && !isPastDeadline)
			{
				score.Status = 1; // Hoàn thành đúng hạn
				score.CompletionDate = today;
			}
			else if (isPastDeadline && score.Progress < 100)
			{
				score.Status = 2; // Chưa hoàn thành (quá hạn mà chưa đạt 100%)
				score.CompletionDate = null;
			}
			else if (isPastDeadline && score.Progress == 100)
			{
				score.Status = 3; // Hoàn thành muộn (quá hạn nhưng đã đạt 100%)
				score.CompletionDate = today;
			}
			else if (!isPastDeadline && score.Progress > 0 && score.Progress < 100)
			{
				score.Status = 4; // Đang xử lý (chưa đến hạn nhưng tiến độ > 0)
				score.CompletionDate = null;
			}
			else if (score.Progress == 0 || score.Progress==null)
			{
				score.Status = 0; // Chưa bắt đầu
				score.CompletionDate = null;
			}

			// Lưu thay đổi vào database
			_context.Entry(score).State = EntityState.Modified;
			await UpdateAnalysis(score.JobMapEmployee.EmployeeId);
			await _context.SaveChangesAsync();

			return Ok(new { success = true, newStatus = score.Status, completionDate = score.CompletionDate?.ToString("dd/MM/yyyy") });
		}

        
		private async Task UpdateAnalysis(long? employeeId)
		{
			if (employeeId == null)
				return;

			// Lấy tất cả bản ghi Score của nhân viên
			var jobs = await _context.Scores
				.Where(j => j.JobMapEmployee.EmployeeId == employeeId && j.CreateDate.HasValue)
				.ToListAsync();

			if (!jobs.Any())
				return;

			// Nhóm các bản ghi Score theo tháng và năm của CreateDate
			var groupedJobs = jobs
				.GroupBy(j => new { j.CreateDate.Value.Year, j.CreateDate.Value.Month });

			foreach (var group in groupedJobs)
			{
				var year = group.Key.Year;
				var month = group.Key.Month;

				// Tính số lượng công việc theo trạng thái cho tháng/năm này
				int ontime = group.Count(j => j.Status == 1);
				int late = group.Count(j => j.Status == 2);
				int overdue = group.Count(j => j.Status == 3);
				int processing = group.Count(j => j.Status == 4 || j.Status == 0 || j.Status == 5);
				int total = ontime + late + overdue + processing;

				// Tìm bản ghi Analysis cho tháng/năm của CreateDate
				var analysis = await _context.Analyses
					.FirstOrDefaultAsync(a => a.EmployeeId == employeeId
										   && a.Time.HasValue
										   && a.Time.Value.Month == month
										   && a.Time.Value.Year == year);

				if (analysis == null)
				{
					// Nếu chưa có bản ghi, tạo mới
					analysis = new Analysis
					{
						EmployeeId = employeeId,
						Total = total,
						Ontime = ontime,
						Late = late,
						Overdue = overdue,
						Processing = processing,
						Time = new DateTime(year, month, 1), // Dựa trên CreateDate
						CreateDate = DateTime.Now,
						UpdateDate = DateTime.Now
					};
					_context.Analyses.Add(analysis);
				}
				else
				{
					// Nếu đã có bản ghi, cập nhật dữ liệu
					analysis.Total = total;
					analysis.Ontime = ontime;
					analysis.Late = late;
					analysis.Overdue = overdue;
					analysis.Processing = processing;
					analysis.UpdateDate = DateTime.Now;
				}
			}

			await _context.SaveChangesAsync();
		}
		

		private bool JobExists(long id)
		{
			return _context.Jobs.Any(e => e.Id == id);
		}

	}
}
