using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkTrackingSystem.Models;

namespace WorkTrackingSystem.Areas.AdminSystem.Controllers
{
    [Area("AdminSystem")]
    public class AnalysesController : Controller
    {
        private readonly WorkTrackingSystemContext _context;

        public AnalysesController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: AdminSystem/Analyses
        public async Task<IActionResult> Index()
        {
            var workTrackingSystemContext = _context.Analyses.Include(a => a.Employee);
            return View(await workTrackingSystemContext.ToListAsync());
        }

        // GET: AdminSystem/Analyses/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var analysis = await _context.Analyses
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (analysis == null)
            {
                return NotFound();
            }

            return View(analysis);
        }

        // GET: AdminSystem/Analyses/Create
        public IActionResult Create()
        {
            
            ViewData["EmployeeId"] = new SelectList(_context.Employees.Include(p => p.Position).Select(e => new
            {
                Id = e.Id,
                FullName = e.FirstName + " " + e.LastName + "-" + e.Position.Name,

            }), "Id", "FullName");
            return View();
        }

        // POST: AdminSystem/Analyses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmployeeId,Total,Ontime,Late,Overdue,Processing,Time,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Analysis analysis)
        {
            if (ModelState.IsValid)
            {
                _context.Add(analysis);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", analysis.EmployeeId);
            return View(analysis);
        }

        // GET: AdminSystem/Analyses/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var analysis = await _context.Analyses.FindAsync(id);
            if (analysis == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", analysis.EmployeeId);
            return View(analysis);
        }

        // POST: AdminSystem/Analyses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,EmployeeId,Total,Ontime,Late,Overdue,Processing,Time,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Analysis analysis)
        {
            if (id != analysis.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(analysis);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnalysisExists(analysis.Id))
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
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", analysis.EmployeeId);
            return View(analysis);
        }

        // GET: AdminSystem/Analyses/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var analysis = await _context.Analyses
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (analysis == null)
            {
                return NotFound();
            }

            return View(analysis);
        }

        // POST: AdminSystem/Analyses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var analysis = await _context.Analyses.FindAsync(id);
            if (analysis != null)
            {
                _context.Analyses.Remove(analysis);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnalysisExists(long id)
        {
            return _context.Analyses.Any(e => e.Id == id);
        }
    }
}
