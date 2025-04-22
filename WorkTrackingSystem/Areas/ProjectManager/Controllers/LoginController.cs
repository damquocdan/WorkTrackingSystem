using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using WorkTrackingSystem.Areas.ProjectManager.Models;
using WorkTrackingSystem.Common;
using WorkTrackingSystem.Models;

namespace WorkTrackingSystem.Areas.ProjectManager.Controllers
{
    [Area("ProjectManager")]
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
            //&& x.Employee.PositionId == 2);
            string IdValues = _context.Systemsws.FirstOrDefault(x => x.Name.Equals("ProjectManager")).Value;

            var password = SHA.GetSha256Hash(model.Password);
            var dataLogin = _context.Users.Include(x => x.Employee).FirstOrDefault(x => x.UserName.Equals(model.UserName) && x.Password.Equals(password) && x.Employee.PositionId.ToString().Equals(IdValues));
            if (dataLogin != null)
            {
                HttpContext.Session.SetString("ProjectManagerLogin", model.UserName);
                HttpContext.Session.SetString("ProjectManageUserId", dataLogin.Id.ToString());
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
            HttpContext.Session.Remove("ProjectManagerLogin"); // huỷ session với key AdminLogin đã lưu trước đó

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}

