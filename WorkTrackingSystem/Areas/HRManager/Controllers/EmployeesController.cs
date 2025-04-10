using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using WorkTrackingSystem.Models;
using X.PagedList.Extensions;

namespace WorkTrackingSystem.Areas.HRManager.Controllers
{
    [Area("HRManager")]
    public class EmployeesController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public EmployeesController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

		// GET: HRManager/Employees
		public async Task<IActionResult> Index(string search, int? DepartmentId, int page = 1)
		{
			var limit = 10;
			var query = _context.Employees.Where(e => e.IsActive == true).Include(e => e.Department).Include(e => e.Position).AsQueryable();

			if (DepartmentId > 0)
			{
				query = query.Where(e => e.DepartmentId == DepartmentId);
			}

			if (!string.IsNullOrEmpty(search))
			{
				var searchLower = search.ToLower();
				query = query.Where(e => (e.Code + " " + e.FirstName + " " + e.LastName).ToLower().Contains(searchLower));
			}
			if (!string.IsNullOrEmpty(search) && DepartmentId > 0)
			{

				query = query.Where(e => (e.FirstName + " " + e.LastName).ToLower().Contains(search.ToLower()) && e.DepartmentId == DepartmentId);
			}
			var employees = query.ToPagedList(page, limit);
			ViewBag.Department = new SelectList(_context.Departments, "Id", "Name");

			return View(employees);


		}

		// GET: HRManager/Employees/Details/5
		public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Details", employee);
            }
            return View(employee);
        }

        // GET: HRManager/Employees/Create
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name");
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Create");
            }
            return View();
        }

        // POST: HRManager/Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DepartmentId,PositionId,Code,FirstName,LastName,Gender,Birthday,Phone,Email,HireDate,Address,Avatar,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Employee employee)
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
               var employeeCr = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Id == user.EmployeeId);

                if (employeeCr.FirstName != null && employeeCr.LastName != null)
                {
                    employee.CreateBy = employeeCr.FirstName+""+ employeeCr.LastName;
                 
                }
                employee.CreateDate = DateTime.Now;
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id", employee.DepartmentId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Id", employee.PositionId);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Create", employee);
            }
            return View(employee);
        }

        // GET: HRManager/Employees/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name", employee.PositionId);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Edit", employee);
            }
            return View(employee);
        }

        // POST: HRManager/Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,DepartmentId,PositionId,Code,FirstName,LastName,Gender,Birthday,Phone,Email,HireDate,Address,Avatar,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Employee employee)
        {
            var userId = HttpContext.Session.GetString("HRUserId");
            var idus = long.Parse(userId);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == idus);
            var employeead = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(e => e.Id == user.EmployeeId);
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    employee.UpdateDate= DateTime.Now;
                    employee.UpdateBy = employeead.FirstName + "" + employeead.LastName;
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
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
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id", employee.DepartmentId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Id", employee.PositionId);
            return View(employee);
        }

        // GET: HRManager/Employees/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
               
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Delete", employee);
            }
            return View(employee);
        }

        // POST: HRManager/Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                employee.IsActive = false;
                employee.IsDelete = true;
                _context.Employees.Update(employee);
                //_context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(long id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
