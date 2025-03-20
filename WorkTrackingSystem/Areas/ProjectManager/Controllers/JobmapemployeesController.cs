using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkTrackingSystem.Models;

namespace WorkTrackingSystem.Areas.ProjectManager.Controllers
{
    [Area("ProjectManager")]
    public class JobmapemployeesController : Controller
    {
        private readonly WorkTrackingSystemContext _context;

        public JobmapemployeesController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: ProjectManager/Jobmapemployees
        public async Task<IActionResult> Index()
        {
            var workTrackingSystemContext = _context.Jobmapemployees.Include(j => j.Employee).Include(j => j.Job);
            return View(await workTrackingSystemContext.ToListAsync());
        }

        // GET: ProjectManager/Jobmapemployees/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jobmapemployee = await _context.Jobmapemployees
                .Include(j => j.Employee)
                .Include(j => j.Job)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (jobmapemployee == null)
            {
                return NotFound();
            }

            return View(jobmapemployee);
        }

        // GET: ProjectManager/Jobmapemployees/Create
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id");
            ViewData["JobId"] = new SelectList(_context.Jobs, "Id", "Id");
            return View();
        }

        // POST: ProjectManager/Jobmapemployees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmployeeId,JobId,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Jobmapemployee jobmapemployee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(jobmapemployee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", jobmapemployee.EmployeeId);
            ViewData["JobId"] = new SelectList(_context.Jobs, "Id", "Id", jobmapemployee.JobId);
            return View(jobmapemployee);
        }

        // GET: ProjectManager/Jobmapemployees/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jobmapemployee = await _context.Jobmapemployees.FindAsync(id);
            if (jobmapemployee == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", jobmapemployee.EmployeeId);
            ViewData["JobId"] = new SelectList(_context.Jobs, "Id", "Id", jobmapemployee.JobId);
            return View(jobmapemployee);
        }

        // POST: ProjectManager/Jobmapemployees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,EmployeeId,JobId,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Jobmapemployee jobmapemployee)
        {
            if (id != jobmapemployee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(jobmapemployee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobmapemployeeExists(jobmapemployee.Id))
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
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", jobmapemployee.EmployeeId);
            ViewData["JobId"] = new SelectList(_context.Jobs, "Id", "Id", jobmapemployee.JobId);
            return View(jobmapemployee);
        }

        // GET: ProjectManager/Jobmapemployees/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var jobmapemployee = await _context.Jobmapemployees
                .Include(j => j.Employee)
                .Include(j => j.Job)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (jobmapemployee == null)
            {
                return NotFound();
            }

            return View(jobmapemployee);
        }

        // POST: ProjectManager/Jobmapemployees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var jobmapemployee = await _context.Jobmapemployees.FindAsync(id);
            if (jobmapemployee != null)
            {
                _context.Jobmapemployees.Remove(jobmapemployee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JobmapemployeeExists(long id)
        {
            return _context.Jobmapemployees.Any(e => e.Id == id);
        }
    }
}
