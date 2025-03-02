using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using WorkTrackingSystem.Models;

namespace WorkTrackingSystem.Areas.HRManager.Controllers
{
    [Area("HRManager")]
    public class DepartmentsController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public DepartmentsController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: HRManager/Departments
        public async Task<IActionResult> Index()
        {
            return View(await _context.Departments.ToListAsync());
        }

        // GET: HRManager/Departments/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (department == null)
            {
                return NotFound();
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Details", department);
            }
            return View(department);
        }

        // GET: HRManager/Departments/Create
        public IActionResult Create()
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Create");
            }
            return View();
        }

        // POST: HRManager/Departments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Code,Name,Description,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Department department)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("HRUserId");
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Index", "Login");
                }
                long id = long.Parse(userId);
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

                if (user == null || user.EmployeeId == null)
                {
                    return NotFound("Không tìm thấy thông tin nhân viên.");
                }

                var employee = await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Position)
                    .FirstOrDefaultAsync(e => e.Id == user.EmployeeId);

                if (employee == null)
                {
                    return NotFound("Không tìm thấy thông tin nhân viên.");
                }
                if (employee.FirstName != null && employee.LastName!= null)
                {
                    department.CreateBy = employee.Id;
                    //department.CreateBy = $"{employee.FirstName ?? ""} {employee.LastName ?? ""}".Trim();
                } 
                department.CreateDate= DateTime.Now;
                _context.Add(department);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        // GET: HRManager/Departments/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Edit", department);
            }
            return View(department);
        }

        // POST: HRManager/Departments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Code,Name,Description,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Department department)
        {
            if (id != department.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    department.UpdateDate = DateTime.Now;
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.Id))
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
            return View(department);
        }

        // GET: HRManager/Departments/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var department = await _context.Departments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (department == null)
            {
                return NotFound();
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Delete", department);
            }
            return View(department);
        }

        // POST: HRManager/Departments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DepartmentExists(long id)
        {
            return _context.Departments.Any(e => e.Id == id);
        }
    }
}
