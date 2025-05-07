using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using WorkTrackingSystem.Areas.EmployeeSystem.Models;
using WorkTrackingSystem.Common;
using WorkTrackingSystem.Models;

namespace WorkTrackingSystem.Areas.EmployeeSystem.Controllers
{
    [Area("EmployeeSystem")]
    public class LoginController : Controller
    {
        public WorkTrackingSystemContext _context;
        public LoginController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost] // POST -> khi submit form
        public IActionResult Index(Login model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không hợp lệ.");
                return View(model);
            }
            string IdValues = _context.Systemsws.FirstOrDefault(x => x.Name.Equals("EmployeeSystem")).Value;

            var password = SHA.GetSha256Hash(model.Password);
            var dataLogin = _context.Users.Include(x => x.Employee).FirstOrDefault(x => x.UserName.Equals(model.UserName) && x.Password.Equals(password) && x.Employee.PositionId.ToString().Equals(IdValues));
            if (dataLogin != null)
            {
                HttpContext.Session.SetString("EmployeeSystem", model.UserName);
                HttpContext.Session.SetString("UserId", dataLogin.Id.ToString());
                HttpContext.Session.SetString("EmployeeId", dataLogin.EmployeeId.ToString());
                HttpContext.Session.SetString("UserAvatar", dataLogin.Employee?.Avatar ?? "~/images/avatar.png");
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không chính xác.");
                return View(model);
            }

        }
        [HttpGet]// thoát đăng nhập, huỷ session
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("EmployeeSystem"); // huỷ session với key AdminLogin đã lưu trước đó

            return RedirectToAction("Index", "Home", new { area = "" });
        }




    }
}

