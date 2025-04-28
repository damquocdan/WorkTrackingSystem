using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using WorkTrackingSystem.Common;
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
              
                employee.CreateBy = HttpContext.Session.GetString("HRManagerLogin");
                _context.Add(employee);

                await _context.SaveChangesAsync();
                var lastInsertedId = employee.Id;
                var users = new User()
                {
                    EmployeeId = lastInsertedId,
                    UserName = employee.Email,
                    Password = SHA.GetSha256Hash("Devmaster@6789"),
                    CreateBy = HttpContext.Session.GetString("HRManagerLogin")
                };
                _context.AddAsync(users);
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
		public async Task<IActionResult> Edit(long id, [Bind("Id,DepartmentId,PositionId,Code,FirstName,LastName,Gender,Birthday,Phone,Email,HireDate,Address,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Employee employee, string? img)
		{
			if (id != employee.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var existingEmployee = await _context.Employees
						.Include(e => e.Department)
						.Include(e => e.Position)
						.FirstOrDefaultAsync(e => e.Id == id);

					if (existingEmployee == null)
					{
						return NotFound("Không tìm thấy thông tin nhân viên.");
					}

					// Cập nhật thông tin nhân viên
					existingEmployee.FirstName = employee.FirstName;
					existingEmployee.LastName = employee.LastName;
					existingEmployee.Gender = employee.Gender;
					existingEmployee.Birthday = employee.Birthday;
					existingEmployee.Phone = employee.Phone;
					existingEmployee.Email = employee.Email;
					existingEmployee.HireDate = employee.HireDate;
					existingEmployee.Address = employee.Address;
					existingEmployee.DepartmentId = employee.DepartmentId;
					existingEmployee.PositionId = employee.PositionId;
					existingEmployee.UpdateBy = HttpContext.Session.GetString("HRManagerLogin");
					existingEmployee.UpdateDate = DateTime.Now;

					// Xử lý avatar
					var files = HttpContext.Request.Form.Files;
					if (files.Count > 0 && files[0].Length > 0)
					{
						var file = files[0];
						// Kiểm tra định dạng file
						var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".jfif" };
						var extension = Path.GetExtension(file.FileName).ToLower();
						if (!allowedExtensions.Contains(extension))
						{
							ModelState.AddModelError("", "Chỉ hỗ trợ định dạng ảnh .jpg, .jpeg, .png.");
							ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
							ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name", employee.PositionId);
							return View(employee);
						}

						// Tạo tên file duy nhất với đuôi .jpg
						var fileName = $"{Guid.NewGuid()}.jpg"; // Thay đổi đuôi thành .jpg
						var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/employees", fileName);

						// Xóa ảnh cũ nếu có
						if (!string.IsNullOrEmpty(existingEmployee.Avatar))
						{
							var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingEmployee.Avatar.TrimStart('/'));
							if (System.IO.File.Exists(oldPath))
							{
								System.IO.File.Delete(oldPath);
							}
						}

						// Lưu file với tên mới
						using (var stream = new FileStream(path, FileMode.Create))
						{
							await file.CopyToAsync(stream);
						}

						// Cập nhật đường dẫn avatar
						existingEmployee.Avatar = "/images/employees/" + fileName;
					}
					else
					{
						existingEmployee.Avatar = img;
					}

					_context.Update(existingEmployee);
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
		}// GET: AdminSystem/Employees/Delete/5
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
