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
    public class BaselineassessmentsController : Controller
    {
        private readonly WorkTrackingSystemContext _context;

        public BaselineassessmentsController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: ProjectManager/Baselineassessments
        public async Task<IActionResult> Index()
        {
            // Lấy ManagerId từ session
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");

            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
            }

            // Tìm nhân viên (quản lý) đang đăng nhập
            var manager = await _context.Users
                .Where(u => u.UserName == managerUsername)
                .Select(u => u.Employee)
                .FirstOrDefaultAsync();

            if (manager == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Lấy danh sách phòng ban mà quản lý đang phụ trách
            var managedDepartments = await _context.Departments
                .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 2)) // 2 = Quản lý
                .Select(d => d.Id)
                .ToListAsync();

            // Lọc danh sách đánh giá của nhân viên thuộc các phòng ban mà quản lý phụ trách
            var assessments = _context.Baselineassessments
                .Include(b => b.Employee)
                .Where(b => b.Employee != null &&
                            b.Employee.DepartmentId.HasValue &&
                            managedDepartments.Contains(b.Employee.DepartmentId.Value));

            return View(await assessments.ToListAsync());
        }


        // GET: ProjectManager/Baselineassessments/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baselineassessment = await _context.Baselineassessments
                .Include(b => b.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (baselineassessment == null)
            {
                return NotFound();
            }

            return View(baselineassessment);
        }

        // GET: ProjectManager/Baselineassessments/Create
        public IActionResult Create()
        {
            // Lấy thông tin quản lý đang đăng nhập từ session
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");

            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
            }

            // Tìm nhân viên (quản lý) đang đăng nhập
            var manager = _context.Users
                .Where(u => u.UserName == managerUsername)
                .Select(u => u.Employee)
                .FirstOrDefault();

            if (manager == null || manager.DepartmentId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Lấy danh sách nhân viên thuộc phòng ban của quản lý
            var employees = _context.Employees
                .Where(e => e.DepartmentId == manager.DepartmentId)
                .Select(e => new
                {
                    Id = e.Id,
                    FullName = e.FirstName + " " + e.LastName // Hiển thị họ và tên đầy đủ
                })
                .ToList();

            // Truyền danh sách nhân viên vào ViewData để hiển thị trong dropdown
            ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName");

            return View();
        }


        // POST: ProjectManager/Baselineassessments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmployeeId,VolumeAssessment,ProgressAssessment,QualityAssessment,SummaryOfReviews,Time,Evaluate,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Baselineassessment baselineassessment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(baselineassessment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", baselineassessment.EmployeeId);
            return View(baselineassessment);
        }

        // GET: ProjectManager/Baselineassessments/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baselineassessment = await _context.Baselineassessments.FindAsync(id);
            if (baselineassessment == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", baselineassessment.EmployeeId);
            return View(baselineassessment);
        }

        // POST: ProjectManager/Baselineassessments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,EmployeeId,VolumeAssessment,ProgressAssessment,QualityAssessment,SummaryOfReviews,Time,Evaluate,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Baselineassessment baselineassessment)
        {
            if (id != baselineassessment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(baselineassessment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BaselineassessmentExists(baselineassessment.Id))
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
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", baselineassessment.EmployeeId);
            return View(baselineassessment);
        }

        // GET: ProjectManager/Baselineassessments/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baselineassessment = await _context.Baselineassessments
                .Include(b => b.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (baselineassessment == null)
            {
                return NotFound();
            }

            return View(baselineassessment);
        }

        // POST: ProjectManager/Baselineassessments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var baselineassessment = await _context.Baselineassessments.FindAsync(id);
            if (baselineassessment != null)
            {
                _context.Baselineassessments.Remove(baselineassessment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BaselineassessmentExists(long id)
        {
            return _context.Baselineassessments.Any(e => e.Id == id);
        }
    }
}
