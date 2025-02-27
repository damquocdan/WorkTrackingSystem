using WorkTrackingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WorkTrackingSystem.Areas.ProjectManager.Controllers {

    public class DashboardController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public DashboardController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }
        public async Task<IActionResult> Index1()
        {
            return View();
        }
        public async Task<IActionResult> Index2()
        {
            return View();
        }
        public async Task<IActionResult> Index3()
        {
            return View();
        }
    }

}
