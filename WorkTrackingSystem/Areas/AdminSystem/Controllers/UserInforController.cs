using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkTrackingSystem.Common;
using WorkTrackingSystem.Models;

namespace WorkTrackingSystem.Areas.AdminSystem.Controllers
{
    [Area("AdminSystem")]
    public class UserInforController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public UserInforController(WorkTrackingSystemContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> UserInfor()
        {
            var userId = HttpContext.Session.GetString("AdminUserId");
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

            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            // Lấy UserId từ Session
            var userIdString = HttpContext.Session.GetString("AdminUserId");

            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long userId))
            {
                return RedirectToAction("Login", "Account"); // Chuyển hướng nếu chưa đăng nhập
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new UserEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                //Email = user.Employee?.Email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userIdString = HttpContext.Session.GetString("AdminUserId");
            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out long userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Cập nhật email
            //if (!string.IsNullOrEmpty(model.Email))
            //{
            //    user.Employee.Email = model.Email;
            //}

            // Nếu đổi mật khẩu

            
            if (!string.IsNullOrEmpty(model.CurrentPassword))
            {
                var CurrentPassword = SHA.GetSha256Hash(model.CurrentPassword);
                if (user.Password != CurrentPassword)
                {
                    ModelState.AddModelError(string.Empty, "Mật khẩu hiện tại không đúng.");
                    return View(model);
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError(string.Empty, "Mật khẩu mới không khớp.");
                    return View(model);
                }
                if (!string.IsNullOrEmpty(model.NewPassword))
                    user.Password = SHA.GetSha256Hash(model.NewPassword);
            }

            user.UpdateDate = DateTime.Now;
            user.UpdateBy = HttpContext.Session.GetString("Adminl"); ;

            await _context.SaveChangesAsync();

            ViewData["Message"] = "Cập nhật thành công!";
            return View(model);
        }
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
            return View(employee);
        }

        // POST: EmployeeSystem/Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,DepartmentId,PositionId,Code,FirstName,LastName,Gender,Birthday,Phone,Email,HireDate,Address,Avatar,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Employee employee, string? img)
        {
            //employee.Department ??= new Department { Name = DepartmentName ?? "Chưa có dữ liệu" };
            //employee.Position ??= new Position { Name = PositionName ?? "Chưa có dữ liệu" };
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
                ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name", employee.PositionId);
                return View(employee);
            }

            try
            {
                // Lấy UserId từ session
                var userId = HttpContext.Session.GetString("AdminUserId");
                if (string.IsNullOrEmpty(userId) || !long.TryParse(userId, out long idUser))
                {
                    return Unauthorized("User không hợp lệ.");
                }

                // Tìm user trong database
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == idUser);
                if (user == null)
                {
                    return NotFound("Không tìm thấy thông tin người dùng.");
                }

                // Lấy nhân viên dựa vào UserId
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
                existingEmployee.IsActive = employee.IsActive;
                existingEmployee.UpdateBy = employee.FirstName + " " + employee.LastName;
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

					// Lưu ảnh mới
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
                //ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
                //ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name", employee.PositionId);
                _context.Update(existingEmployee);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(UserInfor));
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
        }

        private bool EmployeeExists(long id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
