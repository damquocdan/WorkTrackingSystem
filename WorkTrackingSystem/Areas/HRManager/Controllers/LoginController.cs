using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using WorkTrackingSystem.Areas.HRManager.Models;
using WorkTrackingSystem.Common;
using WorkTrackingSystem.Models;

namespace WorkTrackingSystem.Areas.HRManager.Controllers
{
    [Area("HRManager")]
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

            //    var pass = model.Password;
            //    var dataLogin = _context.Users.FirstOrDefault(x =>
            //x.UserName.Equals(model.UserName)
            //&& x.Password.Equals(pass)
            //&& x.Employee.PositionId == 2 && x.Employee.DepartmentId == 1);
            string IdHRManager = _context.Systemsws.FirstOrDefault(x => x.Name.Equals("HRManager")).Value;
            string IdHRDepartment = _context.Systemsws.FirstOrDefault(x => x.Name.Equals("HRDepartment")).Value;
            var password = SHA.GetSha256Hash(model.Password);
            var dataLogin = _context.Users.Include(x => x.Employee).FirstOrDefault(x => x.UserName.Equals(model.UserName) && x.Password.Equals(password) && x.Employee.PositionId.ToString().Equals(IdHRManager) && x.Employee.DepartmentId.ToString().Equals(IdHRDepartment));
            if (dataLogin != null)
            {
                HttpContext.Session.SetString("HRManagerLogin", model.UserName);
                HttpContext.Session.SetString("HRUserId", dataLogin.Id.ToString());
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
            HttpContext.Session.Remove("HRManagerLogin"); // huỷ session với key AdminLogin đã lưu trước đó

            return RedirectToAction("Index");
        }
    }
}

