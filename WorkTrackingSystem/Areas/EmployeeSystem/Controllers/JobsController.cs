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

namespace WorkTrackingSystem.Areas.EmployeeSystem.Controllers
{
    [Area("EmployeeSystem")]
    public class JobsController : Controller
    {
        private readonly WorkTrackingSystemContext _context;

        public JobsController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: EmployeeSystem/Jobs
        public async Task<IActionResult> Index()
        {
            var sessionUserId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(sessionUserId))
            {
                return RedirectToAction("Login", "Account");
            }

           
            if (!int.TryParse(sessionUserId, out int userId))
            {
                return BadRequest("UserId trong session không hợp lệ.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.EmployeeId == null)
            {
                return NotFound("Không tìm thấy nhân viên tương ứng.");
            }

            var jobs = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Employee)
                .Where(j => j.EmployeeId == user.EmployeeId)
                .ToListAsync();
            ViewBag.Jobs = jobs;
            return View(jobs);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusViewModel model)
        {
            if (model == null || model.JobId <= 0)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var job = await _context.Jobs.FindAsync(model.JobId);
            if (job == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy công việc" });
            }

            job.Status = model.Status ?? job.Status; // Đảm bảo không bị null
            _context.Entry(job).State = EntityState.Modified; // Đánh dấu là có thay đổi
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật thành công" });
        }

        [HttpPost]
        public IActionResult UpdateProgress(UpdateProgressRequest request)
        {
            if (request == null || request.Id <= 0)
            {
                return BadRequest(new { success = false, Message = "Dữ liệu không hợp lệ" });
            }
            var job = _context.Jobs.Find(request.Id);
            if (job == null)
            {
                return NotFound(new {success= false, Message="Không tìm thấy công việc "});
            }
            job.ProgressAssessment = request.Progress;
            _context.SaveChanges();
            return Ok(new { success = true });
        }
            // GET: EmployeeSystem/Jobs/Details/5
        //public async Task<IActionResult> Details(long? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var job = await _context.Jobs
        //        .Include(j => j.Category)
        //        .Include(j => j.Employee)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (job == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(job);
        //}

        // GET: EmployeeSystem/Jobs/Create
        //public IActionResult Create()
        //{
        //    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id");
        //    ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id");
        //    return View();
        //}

        // POST: EmployeeSystem/Jobs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,EmployeeId,CategoryId,Name,Description,Deadline1,Deadline2,Deadline3,CompletionDate,Status,VolumeAssessment,ProgressAssessment,QualityAssessment,SummaryOfReviews,Time,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Job job)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(job);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", job.CategoryId);
        //    ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", job.EmployeeId);
        //    return View(job);
        //}

        //// GET: EmployeeSystem/Jobs/Edit/5
        //public async Task<IActionResult> Edit(long? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var job = await _context.Jobs.FindAsync(id);
        //    if (job == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", job.CategoryId);
        //    ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", job.EmployeeId);
        //    return View(job);
        //}

        //// POST: EmployeeSystem/Jobs/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(long id, [Bind("Id,EmployeeId,CategoryId,Name,Description,Deadline1,Deadline2,Deadline3,CompletionDate,Status,VolumeAssessment,ProgressAssessment,QualityAssessment,SummaryOfReviews,Time,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Job job)
        //{
        //    if (id != job.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(job);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!JobExists(job.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", job.CategoryId);
        //    ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", job.EmployeeId);
        //    return View(job);
        //}

        // GET: EmployeeSystem/Jobs/Delete/5

        private bool JobExists(long id)
        {
            return _context.Jobs.Any(e => e.Id == id);
        }
        
    }
}
