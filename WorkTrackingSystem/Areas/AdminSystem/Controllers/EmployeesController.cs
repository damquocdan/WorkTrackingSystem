using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkTrackingSystem.Common;
using WorkTrackingSystem.Models;
using X.PagedList.Extensions;

namespace WorkTrackingSystem.Areas.AdminSystem.Controllers
{
    [Area("AdminSystem")]
    public class EmployeesController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public EmployeesController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

		// GET: AdminSystem/Employees
		//public async Task<IActionResult> Index(int page = 1)
		//{
		//    var limit = 12;
		//    var workTrackingSystemContext = _context.Employees.Include(e => e.Department).Include(e => e.Position);
		//    return View( workTrackingSystemContext.ToPagedList(page,limit));
		//}
		public async Task<IActionResult> Index(string search, int? DepartmentId, int page = 1)
		{
			var limit = 5;
			var query = _context.Employees.Include(e => e.Department).Include(e => e.Position).AsQueryable();

			if (DepartmentId > 0)
			{
				query = query.Where(e => e.DepartmentId == DepartmentId);
			}

			if (!string.IsNullOrEmpty(search))
			{
				var searchLower = search.ToLower();
				query = query.Where(e => (e.FirstName + " " + e.LastName).ToLower().Contains(searchLower));
			}
			if (!string.IsNullOrEmpty(search) && DepartmentId > 0)
			{

				query = query.Where(e => (e.FirstName + " " + e.LastName).ToLower().Contains(search.ToLower()) && e.DepartmentId == DepartmentId);
			}
			var employees = query.ToPagedList(page, limit);
			ViewBag.Department = new SelectList(_context.Departments, "Id", "Name");

			return View(employees);


		}
		// GET: AdminSystem/Employees/Details/5
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

        // GET: AdminSystem/Employees/Create
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

        // POST: AdminSystem/Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DepartmentId,PositionId,Code,FirstName,LastName,Gender,Birthday,Phone,Email,HireDate,Address,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                employee.CreateBy = HttpContext.Session.GetString("AdminLogin");

               
                await _context.SaveChangesAsync();
                var lastInsertedId = employee.Id;
                var users = new User()
                {
                    EmployeeId = lastInsertedId,
                    UserName = employee.Email,
                    Password = "Devmaster@6789",
                    CreateBy = HttpContext.Session.GetString("AdminLogin")
                };
                _context.AddAsync(users);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name", employee.PositionId);
			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return PartialView("_Create", employee);
			}
			return View(employee);
        }

        // GET: AdminSystem/Employees/Edit/5
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

        // POST: AdminSystem/Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,DepartmentId,PositionId,Code,FirstName,LastName,Gender,Birthday,Phone,Email,HireDate,Address,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid) 
            {
                try
                {
                    employee.UpdateDate = DateTime.Now;
                    employee.UpdateBy = HttpContext.Session.GetString("AdminLogin");
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
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name", employee.PositionId);
            return View(employee);
        }

        // GET: AdminSystem/Employees/Delete/5
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

        // POST: AdminSystem/Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
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
