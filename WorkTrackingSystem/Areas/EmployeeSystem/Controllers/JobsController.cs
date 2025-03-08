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
		public async Task<IActionResult> Index(int page=1)
		{
			var limit = 9;
			//var userCount = await _context.Users.CountAsync();
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
			var jobs =  _context.Jobs
				.Include(j => j.Category)
				.Include(j => j.Employee)
				.Where(j => j.EmployeeId == user.EmployeeId)
				.ToPagedList(page,limit);

			ViewBag.Jobs = jobs;
			return View(jobs);
		}
		
		[HttpPost]
		public IActionResult UpdateCompletionDate([FromBody] UpdateCompletionDateModel model)
		{
			var job = _context.Jobs.Find(model.JobId);
			if (job == null)
			{
				return Json(new { success = false, message = "Không tìm thấy công việc" });
			}

			if (DateOnly.TryParse(model.CompletionDate, out DateOnly completionDate))
			{
				job.CompletionDate = completionDate;
				_context.SaveChanges();
				return Json(new { success = true });
			}
			else
			{
				return Json(new { success = false, message = "Ngày không hợp lệ" });
			}
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

			// Lấy thời gian hiện tại để so sánh với deadline
			var today = DateOnly.FromDateTime(DateTime.Now);
			bool isPastDeadline = job.Deadline1.HasValue && job.Deadline1.Value < today;

			// Cập nhật tiến độ
			job.Progress = request.Progress;

            // Cập nhật trạng thái dựa trên tiến độ và deadline
            if (job.Progress == 100 && !isPastDeadline)
            {
                job.Status = 1; // Hoàn thành đúng hạn
            }
            else if (isPastDeadline && job.Progress < 100)
            {
                job.Status = 2; // Chưa hoàn thành (quá hạn mà chưa đạt 100%)
            }
            else if (isPastDeadline && job.Progress == 100)
            {
                job.Status = 3; // Hoàn thành muộn (quá hạn nhưng đã đạt 100%)
            }
            else if (!isPastDeadline && job.Progress > 0 && job.Progress < 100)
            {
                job.Status = 4; // Đang xử lý (chưa đến hạn nhưng tiến độ > 0)
            }
            else if (job.Progress == 0)
            {
                job.Status = 5; // Chưa bắt đầu
            }


            _context.Entry(job).State = EntityState.Modified;
			await _context.SaveChangesAsync();

			return Ok(new { success = true, newStatus = job.Status });
		}

	
		private bool JobExists(long id)
		{
			return _context.Jobs.Any(e => e.Id == id);
		}

	}
}
