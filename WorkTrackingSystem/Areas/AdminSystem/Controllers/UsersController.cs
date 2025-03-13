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
    public class UsersController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public UsersController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: AdminSystem/Users
        public async Task<IActionResult> Index(int page= 1 )
        {
            var limit = 12;
            var workTrackingSystemContext = _context.Users.Where(u=>u.IsActive==true).Include(u => u.Employee).ThenInclude(e=>e.Position);

            return View( workTrackingSystemContext.ToPagedList(page,limit));
        }

        // GET: AdminSystem/Users/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }
			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return PartialView("_Details", user);
			}
			return View(user);
        }

        // GET: AdminSystem/Users/Create
        public IActionResult Create()
        {
           
            ViewData["EmployeeId"] = new SelectList(_context.Employees.Include(p=>p.Position).Select(e => new
            {
                Id= e.Id,
                FullName = e.FirstName+" "+e.LastName+"-"+ e.Position.Name,

            }), "Id", "FullName");
			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return PartialView("_Create");
			}
			return View();
        }

        // POST: AdminSystem/Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserName,Password,EmployeeId,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] User user)
        {
            bool employeeAccount = _context.Users.Any(u => u.EmployeeId == user.EmployeeId);
            if (employeeAccount)
            {
                TempData["ErrorMessage"] = "Nhân viên này đã có tài khoản!"; // Lưu thông báo lỗi
                return RedirectToAction(nameof(Create));
            }
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo tài khoản thành công!";
                return RedirectToAction(nameof(Index));
            }
            var employees = _context.Employees
                .Include(e=>e.Position)
                .Where(e=> !_context.Users.Any(u=>u.EmployeeId == e.Id))
                .Select(e=> new
                {
                    Id= e.Id,
                    FullName= e.FirstName+" "+e.LastName+ " - "+ e.Position.Name
                }).ToList();
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", user.EmployeeId);
			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return PartialView("_Create", user);
			}
			return View(user);
        }

        // GET: AdminSystem/Users/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees.Include(p => p.Position).Select(e => new
            {
                Id = e.Id,
                FullName = e.FirstName + " " + e.LastName + "-" + e.Position.Name,

            }), "Id", "FullName");
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return PartialView("_Edit", user);
			}
			return View(user);
        }

        // POST: AdminSystem/Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,UserName,Password,EmployeeId,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
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
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Name", user.EmployeeId);
            return View(user);
        }

        // GET: AdminSystem/Users/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }
			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return PartialView("_Delete", user);
			}
			return View(user);
        }

        // POST: AdminSystem/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsActive = false;
                user.IsDelete = true;
                _context.Users.Update(user);
                //_context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
