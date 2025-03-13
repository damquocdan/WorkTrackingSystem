                                                                     using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkTrackingSystem.Models;
using X.PagedList.Extensions;

namespace WorkTrackingSystem.Areas.AdminSystem.Controllers
{
    [Area("AdminSystem")]
    public class JobsController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public JobsController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: AdminSystem/Jobs
        public async Task<IActionResult> Index(int page=1)
        {
            var limit = 5;
            var workTrackingSystemContext = _context.Jobs.Include(j => j.Category).Include(j => j.Employee);
            return View( workTrackingSystemContext.ToPagedList(page,limit));
        }

        // GET: AdminSystem/Jobs/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var job = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        // GET: AdminSystem/Jobs/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["EmployeeId"] = new SelectList(_context.Employees.Include(p => p.Position).Select(e => new
            {
                Id = e.Id,
                FullName = e.FirstName + " " + e.LastName + "-" + e.Position.Name,

            }), "Id", "FullName");
            return View();
        }

        // POST: AdminSystem/Jobs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmployeeId,CategoryId,Name,Description,Deadline1,Deadline2,Deadline3,CompletionDate,Status,VolumeAssessment,ProgressAssessment,QualityAssessment,SummaryOfReviews,Time,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Job job)
        {
            if (ModelState.IsValid)
            {
                _context.Add(job);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", job.CategoryId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", job.EmployeeId);
            return View(job);
        }

        // GET: AdminSystem/Jobs/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", job.CategoryId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees.Include(p => p.Position).Select(e => new
            {
                Id = e.Id,
                FullName = e.FirstName + " " + e.LastName + "-" + e.Position.Name,

            }), "Id", "FullName");
            return View(job);
        }

        // POST: AdminSystem/Jobs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,EmployeeId,CategoryId,Name,Description,Deadline1,Deadline2,Deadline3,CompletionDate,Status,VolumeAssessment,ProgressAssessment,QualityAssessment,SummaryOfReviews,Time,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Job job)
        {
            if (id != job.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(job);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobExists(job.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", job.CategoryId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", job.EmployeeId);
            return View(job);
        }

        // GET: AdminSystem/Jobs/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var job = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        // POST: AdminSystem/Jobs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job != null)
            {
                _context.Jobs.Remove(job);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JobExists(long id)
        {
            return _context.Jobs.Any(e => e.Id == id);
        }
		public IActionResult JobOfEmployee(int page = 1)
		{
			var limit = 10;
			var employees = _context.Employees
				.Include(e => e.Department)
				.Include(e => e.Position)
				.Select(e => new
				{
					Employee = e,
					JobCount = _context.Jobs.Count(j => j.EmployeeId == e.Id) // Đếm công việc của từng nhân viên
				})
				.ToList();

			// Chuyển đổi danh sách sang kiểu IPagedList
			var pagedEmployees = employees.Select(e => e.Employee).ToPagedList(page, limit);

			// Gán danh sách số lượng công việc vào ViewBag
			ViewBag.JobCounts = employees.ToDictionary(e => e.Employee.Id, e => e.JobCount);

			return View(pagedEmployees);
		}

        public IActionResult EmployeeWork(long id, int? page, string searchTerm, string filterStatus)
		{
			int pageSize = 10;
			int pageNumber = page ?? 1;
			searchTerm = searchTerm != null ? Uri.UnescapeDataString(searchTerm) : "";

			var jobs = _context.Jobs.Include(j => j.Category).Include(j => j.Employee).Where(j=> j.EmployeeId== id).AsQueryable();

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

			ViewBag.SearchTerm = searchTerm;
			ViewBag.FilterStatus = filterStatus;

			return View(jobs.ToPagedList(pageNumber, pageSize));
		}

	}
}
