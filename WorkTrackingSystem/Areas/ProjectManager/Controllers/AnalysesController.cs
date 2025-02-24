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
    public class AnalysesController : Controller
    {
        private readonly WorkTrackingSystemContext _context;

        public AnalysesController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: ProjectManager/Analyses
        public async Task<IActionResult> Index()
        {
            // Lấy thông tin quản lý từ session
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");

            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login"); // Chuyển hướng nếu không có session
            }

            // Tìm nhân viên (quản lý) đang đăng nhập
            var manager = await _context.Users
                .Where(u => u.UserName == managerUsername)
                .Select(u => u.Employee)
                .FirstOrDefaultAsync();

            if (manager == null || manager.DepartmentId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Lấy danh sách nhân viên thuộc phòng ban của quản lý
            var employeeIdsInManagedDepartment = await _context.Employees
                .Where(e => e.DepartmentId == manager.DepartmentId)
                .Select(e => e.Id)
                .ToListAsync();

            // Lọc danh sách phân tích chỉ của nhân viên trong phòng ban
            var workTrackingSystemContext = _context.Analyses
                .Include(a => a.Employee)
                .Where(a => a.EmployeeId.HasValue && employeeIdsInManagedDepartment.Contains(a.EmployeeId.Value));

            return View(await workTrackingSystemContext.ToListAsync());
        }


        // GET: ProjectManager/Analyses/Details/5
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

        // GET: ProjectManager/Analyses/Create
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id");
            return View();
        }

        // POST: ProjectManager/Analyses/Create
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

        // GET: ProjectManager/Analyses/Edit/5
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

        // POST: ProjectManager/Analyses/Edit/5
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

        // GET: ProjectManager/Analyses/Delete/5
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

        // POST: ProjectManager/Analyses/Delete/5
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
