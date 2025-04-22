using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Collections.Generic;
using WorkTrackingSystem.Areas.AdminSystem.Models;
using WorkTrackingSystem.Common;
using WorkTrackingSystem.Models;

namespace WorkTrackingSystem.Areas.AdminSystem.Controllers
{
    [Area("AdminSystem")]
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
            string IdValues = _context.Systemsws.FirstOrDefault(x => x.Name.Equals("AdminSystem")).Value;
           
            var password = SHA.GetSha256Hash(model.Password);
            var dataLogin = _context.Users.Include(x=>x.Employee).FirstOrDefault(x => x.UserName.Equals(model.UserName) && x.Password.Equals(password) && x.Employee.PositionId.ToString().Equals(IdValues));
            if (dataLogin != null )
            {   
                HttpContext.Session.SetString("AdminLogin", model.UserName);
                HttpContext.Session.SetString("AdminUserId", dataLogin.Id.ToString());
                return RedirectToAction("Index", "Dashboard");
            }
            ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không chính xác.");
            return View(model);

        }
        [HttpGet]// thoát đăng nhập, huỷ session
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminLogin"); // huỷ session với key AdminLogin đã lưu trước đó

            return RedirectToAction("Index","Home");
        }
    }
}

