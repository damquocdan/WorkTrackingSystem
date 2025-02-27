using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkTrackingSystem.Models;

namespace WorkTrackingSystem.Areas.HRManager.Controllers
{
    [Area("HRManager")]
    public class BaselineassessmentsController : Controller
    {
        private readonly WorkTrackingSystemContext _context;

        public BaselineassessmentsController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: HRManager/Baselineassessments
        public async Task<IActionResult> Index(string employeeCode, string employeeName, bool? evaluate, string time)
        {
            // Lấy ManagerId từ session
            var managerUsername = HttpContext.Session.GetString("HRManagerLogin");

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

            var assessments = _context.Baselineassessments
        .Include(b => b.Employee)
        .AsQueryable(); 

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


        // GET: HRManager/Baselineassessments/Details/5
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

        // GET: HRManager/Baselineassessments/Create
        public IActionResult Create()
        {
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id");
            return View();
        }

        // POST: HRManager/Baselineassessments/Create
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

        // GET: HRManager/Baselineassessments/Edit/5
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

        // POST: HRManager/Baselineassessments/Edit/5
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

        // GET: HRManager/Baselineassessments/Delete/5
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

        // POST: HRManager/Baselineassessments/Delete/5
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
