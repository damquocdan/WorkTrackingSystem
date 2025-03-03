using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkTrackingSystem.Models;

namespace WorkTrackingSystem.Areas.ProjectManager.Controllers
{
    [Area("ProjectManager")]
    public class BaselineassessmentsController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public BaselineassessmentsController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: ProjectManager/Baselineassessments
        public async Task<IActionResult> Index(string employeeCode, string employeeName, bool? evaluate, string time)
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

            // Lọc theo mã nhân viên
            if (!string.IsNullOrEmpty(employeeCode))
            {
                assessments = assessments.Where(b => b.Employee.Code.Contains(employeeCode));
            }

            // Lọc theo tên nhân viên
            if (!string.IsNullOrEmpty(employeeName))
            {
                assessments = assessments.Where(b => b.Employee.FirstName.Contains(employeeName) || b.Employee.LastName.Contains(employeeName));
            }

            // Lọc theo trạng thái đánh giá
            if (evaluate.HasValue)
            {
                assessments = assessments.Where(b => b.Evaluate == evaluate.Value);
            }

            // Lọc theo tháng/năm
            if (!string.IsNullOrEmpty(time))
            {
                if (DateTime.TryParseExact(time, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime selectedTime))
                {
                    assessments = assessments.Where(b => b.Time.HasValue &&
                                                         b.Time.Value.Year == selectedTime.Year &&
                                                         b.Time.Value.Month == selectedTime.Month);
                }
            }

            return View(await assessments.OrderByDescending(x => x.Time).ToListAsync());

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
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Details", baselineassessment);
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
            // Lấy tháng và năm hiện tại
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // Lấy danh sách nhân viên chưa có đánh giá trong tháng hiện tại
            var employees = _context.Employees
                .Where(e => e.DepartmentId == manager.DepartmentId &&
                            !_context.Baselineassessments.Any(b => b.EmployeeId == e.Id &&
                                                                   b.Time.HasValue &&
                                                                   b.Time.Value.Month == currentMonth &&
                                                                   b.Time.Value.Year == currentYear))
                .Select(e => new
                {
                    Id = e.Id,
                    FullName = e.Code + " - " + e.FirstName + " " + e.LastName // Hiển thị họ và tên đầy đủ
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
                baselineassessment.SummaryOfReviews = (baselineassessment.VolumeAssessment * 0.6) +
                                         (baselineassessment.ProgressAssessment * 0.15) +
                                         (baselineassessment.QualityAssessment * 0.25);

                // Nếu tổng điểm đánh giá > 50, Evaluate = true
                baselineassessment.Evaluate = baselineassessment.SummaryOfReviews > 50;
                baselineassessment.Time = DateTime.Now;
                baselineassessment.CreateDate = DateTime.Now;
                baselineassessment.IsActive = true;
                baselineassessment.IsDelete = false;
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

            var baselineassessment = await _context.Baselineassessments
                .Include(b => b.Employee) // Load Employee để lấy tên
                .FirstOrDefaultAsync(b => b.Id == id);

            if (baselineassessment == null)
            {
                return NotFound();
            }

            // Lấy tên nhân viên để hiển thị
            ViewBag.EmployeeName = baselineassessment.Employee.Code + " - " + baselineassessment.Employee.FirstName + " " + baselineassessment.Employee.LastName;

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

            var existingRecord = await _context.Baselineassessments.FindAsync(id);
            if (existingRecord == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Chỉ cập nhật các trường cần thiết, giữ nguyên giá trị ban đầu của một số trường
                    existingRecord.VolumeAssessment = baselineassessment.VolumeAssessment;
                    existingRecord.ProgressAssessment = baselineassessment.ProgressAssessment;
                    existingRecord.QualityAssessment = baselineassessment.QualityAssessment;
                    existingRecord.SummaryOfReviews = (baselineassessment.VolumeAssessment * 0.6) +
                                                      (baselineassessment.ProgressAssessment * 0.15) +
                                                      (baselineassessment.QualityAssessment * 0.25);

                    // Nếu tổng điểm đánh giá > 50, Evaluate = true
                    existingRecord.Evaluate = existingRecord.SummaryOfReviews > 50;
                    existingRecord.UpdateDate = DateTime.Now;

                    string userIdStr = HttpContext.Session.GetString("ProjectManagerLogin");
                    if (long.TryParse(userIdStr, out long userId))
                    {
                        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
                        if (user != null && user.EmployeeId.HasValue)
                        {
                            existingRecord.UpdateBy = HttpContext.Session.GetString("ProjectManagerLogin");
                        }
                    }

                    // Giữ nguyên các giá trị không thay đổi
                    // existingRecord.Time = existingRecord.Time; // Không cần vì không cập nhật lại
                    // existingRecord.IsDelete = existingRecord.IsDelete;
                    // existingRecord.IsActive = existingRecord.IsActive;
                    // existingRecord.CreateDate = existingRecord.CreateDate;
                    // existingRecord.CreateBy = existingRecord.CreateBy;

                    _context.Update(existingRecord);
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
