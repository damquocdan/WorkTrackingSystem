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
    public class BaselineassessmentsController : Controller
    {
        private readonly WorkTrackingSystemContext _context;

        public BaselineassessmentsController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: ProjectManager/Baselineassessments
        public async Task<IActionResult> Index()
        {
            var workTrackingSystemContext = _context.Baselineassessments.Include(b => b.Employee);
            return View(await workTrackingSystemContext.ToListAsync());
        }

        // GET: ProjectManager/Baselineassessments/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baselineassessment = await _context.Baselineassessments
                .Include(b => b.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (baselineassessment == null)
            {
                return NotFound();
            }

            return View(baselineassessment);
        }

        // GET: ProjectManager/Baselineassessments/Create
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id");
            return View();
        }

        // POST: ProjectManager/Baselineassessments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmployeeId,VolumeAssessment,ProgressAssessment,QualityAssessment,SummaryOfReviews,Time,Evaluate,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Baselineassessment baselineassessment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(baselineassessment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", baselineassessment.EmployeeId);
            return View(baselineassessment);
        }

        // GET: ProjectManager/Baselineassessments/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baselineassessment = await _context.Baselineassessments.FindAsync(id);
            if (baselineassessment == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", baselineassessment.EmployeeId);
            return View(baselineassessment);
        }

        // POST: ProjectManager/Baselineassessments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,EmployeeId,VolumeAssessment,ProgressAssessment,QualityAssessment,SummaryOfReviews,Time,Evaluate,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Baselineassessment baselineassessment)
        {
            if (id != baselineassessment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(baselineassessment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BaselineassessmentExists(baselineassessment.Id))
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
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", baselineassessment.EmployeeId);
            return View(baselineassessment);
        }

        // GET: ProjectManager/Baselineassessments/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baselineassessment = await _context.Baselineassessments
                .Include(b => b.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (baselineassessment == null)
            {
                return NotFound();
            }

            return View(baselineassessment);
        }

        // POST: ProjectManager/Baselineassessments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var baselineassessment = await _context.Baselineassessments.FindAsync(id);
            if (baselineassessment != null)
            {
                _context.Baselineassessments.Remove(baselineassessment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BaselineassessmentExists(long id)
        {
            return _context.Baselineassessments.Any(e => e.Id == id);
        }
    }
}
