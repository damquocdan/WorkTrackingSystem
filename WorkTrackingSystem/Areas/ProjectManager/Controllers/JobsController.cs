using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkTrackingSystem.Models;

namespace WorkTrackingSystem.Areas.ProjectManager.Controllers
{
    [Area("ProjectManager")]
    public class JobsController : Controller
    {
        private readonly WorkTrackingSystemContext _context;

        public JobsController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: ProjectManager/Jobs
        public async Task<IActionResult> Index()
        {
            // Lấy ManagerId từ session (đây là UserName, cần tìm EmployeeId)
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");

            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login"); // Chuyển về trang đăng nhập nếu chưa đăng nhập
            }

            // Tìm nhân viên đang đăng nhập
            var manager = await _context.Users
                .Where(u => u.UserName == managerUsername)
                .Select(u => u.Employee)
                .FirstOrDefaultAsync();

            if (manager == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Lấy danh sách phòng ban mà nhân viên này quản lý
            var managedDepartments = await _context.Departments
                .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 2)) // Vị trí 2 là quản lý
                .Select(d => d.Id)
                .ToListAsync();

            // Lấy danh sách nhân viên thuộc các phòng ban đó
            var employeesInManagedDepartments = await _context.Employees
                .Where(e => e.DepartmentId.HasValue && managedDepartments.Contains(e.DepartmentId.Value))
                .Select(e => e.Id)
                .ToListAsync();

            // Lọc danh sách công việc của nhân viên thuộc phòng ban đó
            var workTrackingSystemContext = _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Employee)
                .Where(j => j.EmployeeId.HasValue && employeesInManagedDepartments.Contains(j.EmployeeId.Value));

            return View(await workTrackingSystemContext.ToListAsync());
        }

        // GET: ProjectManager/Jobs/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var job = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        // GET: ProjectManager/Jobs/Create
        public IActionResult Create()
        {
            // Lấy ManagerId từ session
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");

            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
            }

            // Tìm nhân viên đang đăng nhập (quản lý)
            var manager = _context.Users
                .Include(u => u.Employee) // Đảm bảo lấy thông tin Employee
                .FirstOrDefault(u => u.UserName == managerUsername)
                ?.Employee;

            if (manager == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Lấy danh sách phòng ban mà nhân viên này quản lý
            var managedDepartments = _context.Departments
                .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 2)) // 2 là vị trí Quản lý
                .Select(d => d.Id)
                .ToList();

            // Lọc danh sách nhân viên thuộc các phòng ban mà quản lý này phụ trách
            var employeesInManagedDepartments = _context.Employees
                .Where(e => e.DepartmentId.HasValue && managedDepartments.Contains(e.DepartmentId.Value))
                .Select(e => new
                {
                    Id = e.Id,
                    FullName = e.FirstName + " " + e.LastName
                })
                .ToList();

            // Truyền danh sách danh mục công việc
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");

            // Hiển thị đầy đủ họ và tên nhân viên trong dropdown
            ViewData["EmployeeId"] = new SelectList(employeesInManagedDepartments, "Id", "FullName");

            // Mặc định trạng thái "Đang xử lý" và ngày tạo là hôm nay
            var newJob = new Job
            {
                Time = DateTime.Now,
                Status = 0
            };

            return View(newJob);
        }



        // POST: ProjectManager/Jobs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmployeeId,CategoryId,Name,Description,Deadline1,Deadline2,Deadline3,CompletionDate,Status,VolumeAssessment,ProgressAssessment,QualityAssessment,SummaryOfReviews,Time,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Job job)
        {
            if (ModelState.IsValid)
            {
                _context.Add(job);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", job.CategoryId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", job.EmployeeId);
            return View(job);
        }

        // GET: ProjectManager/Jobs/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", job.CategoryId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Name", job.EmployeeId);
            return View(job);
        }

        // POST: ProjectManager/Jobs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,EmployeeId,CategoryId,Name,Description,Deadline1,Deadline2,Deadline3,CompletionDate,Status,VolumeAssessment,ProgressAssessment,QualityAssessment,SummaryOfReviews,Time,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Job job)
        {
            if (id != job.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(job);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!JobExists(job.Id))
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
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", job.CategoryId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", job.EmployeeId);
            return View(job);
        }

        // GET: ProjectManager/Jobs/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var job = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (job == null)
            {
                return NotFound();
            }

            return View(job);
        }

        // POST: ProjectManager/Jobs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job != null)
            {
                _context.Jobs.Remove(job);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool JobExists(long id)
        {
            return _context.Jobs.Any(e => e.Id == id);
        }
    }
}
