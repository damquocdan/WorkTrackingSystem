using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkTrackingSystem.Areas.EmployeeSystem.Models;
using WorkTrackingSystem.Models;
using UserEditViewModel = WorkTrackingSystem.Areas.EmployeeSystem.Models.UserEditViewModel;

namespace WorkTrackingSystem.Areas.EmployeeSystem.Controllers
{
    [Area("EmployeeSystem")]
    public class UsersController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public UsersController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: EmployeeSystem/Users
        public async Task<IActionResult> Index()
        {
            var WorkTrackingSystemContext = _context.Users.Include(u => u.Employee);
            return View(await WorkTrackingSystemContext.ToListAsync());
        }
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            // Lấy UserId từ Session
            var userIdString = HttpContext.Session.GetString("UserId");

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
        public async Task<IActionResult> EditProfile(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userIdString = HttpContext.Session.GetString("UserId");
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
                if (user.Password != model.CurrentPassword)
                {
                    ModelState.AddModelError(string.Empty, "Mật khẩu hiện tại không đúng.");
                    return View(model);
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError(string.Empty, "Mật khẩu mới không khớp.");
                    return View(model);
                }

                user.Password = model.NewPassword;
            }

            user.UpdateDate = DateTime.Now;
            //user.UpdateBy = ;

            await _context.SaveChangesAsync();

            ViewData["Message"] = "Cập nhật thành công!";
            return View(model);
        }



        // GET: EmployeeSystem/Users/Details/5
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

            return View(user);
        }

        // GET: EmployeeSystem/Users/Create
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id");
            return View();
        }

        // POST: EmployeeSystem/Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserName,Password,EmployeeId,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", user.EmployeeId);
            return View(user);
        }

        // GET: EmployeeSystem/Users/Edit/5
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
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", user.EmployeeId);
            return View(user);
        }

        // POST: EmployeeSystem/Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(long id, [Bind("Id,UserName,Password,EmployeeId,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] User user)
        //{
        //    if (id != user.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(user);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!UserExists(user.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", user.EmployeeId);
        //    return View(user);
        //}

        //// GET: EmployeeSystem/Users/Delete/5
        //public async Task<IActionResult> Delete(long? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var user = await _context.Users
        //        .Include(u => u.Employee)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(user);
        //}

        //// POST: EmployeeSystem/Users/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(long id)
        //{
        //    var user = await _context.Users.FindAsync(id);
        //    if (user != null)
        //    {
        //        _context.Users.Remove(user);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
