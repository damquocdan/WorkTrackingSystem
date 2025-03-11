using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using WorkTrackingSystem.Models;

namespace WorkTrackingSystem.Areas.ProjectManager.Controllers
{
    [Area("ProjectManager")]
    public class EmployeesController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public EmployeesController(WorkTrackingSystemContext context)
        {
            _context = context;
        }


   
        // GET: ProjectManager/Employees
        public async Task<IActionResult> Index()
        {
            // Lấy ManagerId từ session
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");

            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
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
                .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3)) // 2 = Quản lý
                .Select(d => d.Id)
                .ToListAsync();

            // Lọc danh sách nhân viên thuộc phòng ban mà quản lý này phụ trách
            var employeesInManagedDepartments = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Where(e => e.DepartmentId.HasValue && managedDepartments.Contains(e.DepartmentId.Value));

            return View(await employeesInManagedDepartments.ToListAsync());
        }


        // GET: ProjectManager/Employees/Details/5
        public IActionResult Details(long id,
    string month = null,
    string searchText = null,
    DateOnly? startDate = null,
    DateOnly? endDate = null,
    int? status = null,
    int? categoryId = null,
    string sortOrder = null,
    bool showCompletedZeroReview = false,
    bool dueToday = false)
        {
            // Lấy nhân viên
            var employee = _context.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }

            // Xác định tháng (nếu không có thì dùng tháng hiện tại làm mặc định cho view)
            DateTime selectedMonth = string.IsNullOrEmpty(month)
                ? DateTime.Now
                : DateTime.Parse(month + "-01");

            // Lấy danh sách công việc của nhân viên
            var jobsQuery = _context.Jobs.Where(j => j.EmployeeId == id);

            // Kiểm tra xem có bộ lọc nào được áp dụng không
            bool hasFilters = !string.IsNullOrEmpty(searchText) ||
                             startDate.HasValue ||
                             endDate.HasValue ||
                             status.HasValue ||
                             categoryId.HasValue ||
                             showCompletedZeroReview ||
                             dueToday ||
                             !string.IsNullOrEmpty(sortOrder);

            // Nếu có month được chỉ định nhưng không có bộ lọc khác, lọc theo tháng
            if (!string.IsNullOrEmpty(month) && !hasFilters)
            {
                jobsQuery = jobsQuery.Where(j => j.Deadline1.HasValue
                    && j.Deadline1.Value.Year == selectedMonth.Year
                    && j.Deadline1.Value.Month == selectedMonth.Month);
            }

            // Áp dụng các bộ lọc nếu có
            if (!string.IsNullOrEmpty(searchText))
            {
                jobsQuery = jobsQuery.Where(j => j.Name.Contains(searchText) || j.Description.Contains(searchText));
            }

            if (startDate.HasValue)
            {
                jobsQuery = jobsQuery.Where(j => j.Deadline1.HasValue && j.Deadline1.Value >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                jobsQuery = jobsQuery.Where(j => j.Deadline1.HasValue && j.Deadline1.Value <= endDate.Value);
            }

            if (status.HasValue)
            {
                jobsQuery = jobsQuery.Where(j => j.Status == status.Value);
            }

            if (categoryId.HasValue)
            {
                jobsQuery = jobsQuery.Where(j => j.CategoryId == categoryId.Value);
            }

            if (showCompletedZeroReview)
            {
                jobsQuery = jobsQuery.Where(j => j.Status == 1 && (j.SummaryOfReviews == null || j.SummaryOfReviews == 0));
            }

            if (dueToday)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                jobsQuery = jobsQuery.Where(j => j.Deadline1.HasValue && j.Deadline1.Value == today);
            }

            // Sắp xếp
            switch (sortOrder)
            {
                case "due_asc":
                    jobsQuery = jobsQuery.OrderBy(j => j.Deadline1);
                    break;
                case "due_desc":
                    jobsQuery = jobsQuery.OrderByDescending(j => j.Deadline1);
                    break;
                case "review_asc":
                    jobsQuery = jobsQuery.OrderBy(j => j.SummaryOfReviews);
                    break;
                case "review_desc":
                    jobsQuery = jobsQuery.OrderByDescending(j => j.SummaryOfReviews);
                    break;
                default:
                    jobsQuery = jobsQuery.OrderBy(j => j.Deadline1 ?? DateOnly.MaxValue);
                    break;
            }

            var jobs = jobsQuery.ToList();

            // Lấy đánh giá của nhân viên theo phạm vi thời gian
            Baselineassessment baseline = null;
            if (startDate.HasValue && endDate.HasValue)
            {
                baseline = _context.Baselineassessments
                    .FirstOrDefault(b => b.EmployeeId == id
                        && b.Time.HasValue
                        && b.Time.Value >= startDate.Value.ToDateTime(TimeOnly.MinValue)
                        && b.Time.Value <= endDate.Value.ToDateTime(TimeOnly.MaxValue));
            }
            else
            {
                baseline = _context.Baselineassessments
                    .FirstOrDefault(b => b.EmployeeId == id
                        && b.Time.HasValue
                        && b.Time.Value.Year == selectedMonth.Year
                        && b.Time.Value.Month == selectedMonth.Month);
            }

            // Lấy phân tích của nhân viên theo phạm vi thời gian
            Analysis analysis = null;
            if (startDate.HasValue && endDate.HasValue)
            {
                analysis = _context.Analyses
                    .FirstOrDefault(a => a.EmployeeId == id
                        && a.Time.HasValue
                        && a.Time.Value >= startDate.Value.ToDateTime(TimeOnly.MinValue)
                        && a.Time.Value <= endDate.Value.ToDateTime(TimeOnly.MaxValue));
            }
            else
            {
                analysis = _context.Analyses
                    .FirstOrDefault(a => a.EmployeeId == id
                        && a.Time.HasValue
                        && a.Time.Value.Year == selectedMonth.Year
                        && a.Time.Value.Month == selectedMonth.Month);
            }

            // Chuẩn bị ViewData cho danh mục
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");

            // Truyền startDate và endDate qua ViewBag
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            // Tạo view model
            var viewModel = new EmployeeDetailsViewModel
            {
                Employee = employee,
                Jobs = jobs,
                BaselineAssessment = baseline,
                Analysis = analysis,
                SelectedMonth = selectedMonth
            };

            // Nếu có bộ lọc hoặc là yêu cầu AJAX, trả về PartialView
            if (hasFilters || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Details", viewModel);
            }

            // Nếu không có bộ lọc và không phải AJAX, trả về View đầy đủ
            return View(viewModel);
        }

        // GET: ProjectManager/Employees/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: ProjectManager/Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(long id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateJobAssessments(
    List<JobUpdateModel> Jobs,
    string month = null,
    string searchText = null,
    DateOnly? startDate = null,
    DateOnly? endDate = null,
    int? status = null,
    int? categoryId = null,
    string sortOrder = null,
    bool showCompletedZeroReview = false,
    bool dueToday = false)
        {
            if (Jobs == null || !Jobs.Any())
            {
                return BadRequest("Không có dữ liệu để cập nhật.");
            }

            var firstJob = await _context.Jobs.FindAsync(Jobs.First().Id);
            if (firstJob == null)
            {
                return NotFound("Không tìm thấy công việc.");
            }
            long employeeId = firstJob.EmployeeId ?? 0;

            foreach (var jobModel in Jobs)
            {
                var job = await _context.Jobs.FindAsync(jobModel.Id);
                if (job == null)
                {
                    continue;
                }

                job.VolumeAssessment = jobModel.VolumeAssessment;
                job.ProgressAssessment = jobModel.ProgressAssessment;
                job.QualityAssessment = jobModel.QualityAssessment;

                if (job.VolumeAssessment.HasValue && job.ProgressAssessment.HasValue && job.QualityAssessment.HasValue)
                {
                    job.SummaryOfReviews = (job.VolumeAssessment.Value * 0.6f) +
                                           (job.ProgressAssessment.Value * 0.15f) +
                                           (job.QualityAssessment.Value * 0.25f);
                }
                else
                {
                    job.SummaryOfReviews = null;
                }

                _context.Update(job);
            }

            await _context.SaveChangesAsync();
            await UpdateBaselineAssessment(employeeId);
            // Tái tạo viewModel với bộ lọc
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return NotFound("Không tìm thấy nhân viên.");
            }

            DateTime selectedMonth = string.IsNullOrEmpty(month)
                ? DateTime.Now
                : DateTime.Parse(month + "-01");

            var jobsQuery = _context.Jobs.Where(j => j.EmployeeId == employeeId);

            bool hasFilters = !string.IsNullOrEmpty(searchText) ||
                             startDate.HasValue ||
                             endDate.HasValue ||
                             status.HasValue ||
                             categoryId.HasValue ||
                             showCompletedZeroReview ||
                             dueToday ||
                             !string.IsNullOrEmpty(sortOrder);

            if (!string.IsNullOrEmpty(month) && !hasFilters)
            {
                jobsQuery = jobsQuery.Where(j => j.Deadline1.HasValue
                    && j.Deadline1.Value.Year == selectedMonth.Year
                    && j.Deadline1.Value.Month == selectedMonth.Month);
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                jobsQuery = jobsQuery.Where(j => j.Name.Contains(searchText) || j.Description.Contains(searchText));
            }

            if (startDate.HasValue)
            {
                jobsQuery = jobsQuery.Where(j => j.Deadline1.HasValue && j.Deadline1.Value >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                jobsQuery = jobsQuery.Where(j => j.Deadline1.HasValue && j.Deadline1.Value <= endDate.Value);
            }

            if (status.HasValue)
            {
                jobsQuery = jobsQuery.Where(j => j.Status == status.Value);
            }

            if (categoryId.HasValue)
            {
                jobsQuery = jobsQuery.Where(j => j.CategoryId == categoryId.Value);
            }

            if (showCompletedZeroReview)
            {
                jobsQuery = jobsQuery.Where(j => j.Status == 1 && (j.SummaryOfReviews == null || j.SummaryOfReviews == 0));
            }

            if (dueToday)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                jobsQuery = jobsQuery.Where(j => j.Deadline1.HasValue && j.Deadline1.Value == today);
            }

            switch (sortOrder)
            {
                case "due_asc":
                    jobsQuery = jobsQuery.OrderBy(j => j.Deadline1);
                    break;
                case "due_desc":
                    jobsQuery = jobsQuery.OrderByDescending(j => j.Deadline1);
                    break;
                case "review_asc":
                    jobsQuery = jobsQuery.OrderBy(j => j.SummaryOfReviews);
                    break;
                case "review_desc":
                    jobsQuery = jobsQuery.OrderByDescending(j => j.SummaryOfReviews);
                    break;
                default:
                    jobsQuery = jobsQuery.OrderBy(j => j.Deadline1 ?? DateOnly.MaxValue);
                    break;
            }

            var jobs = await jobsQuery.ToListAsync();

            var baseline = startDate.HasValue && endDate.HasValue
                ? await _context.Baselineassessments
                    .FirstOrDefaultAsync(b => b.EmployeeId == employeeId
                        && b.Time.HasValue
                        && b.Time.Value >= startDate.Value.ToDateTime(TimeOnly.MinValue)
                        && b.Time.Value <= endDate.Value.ToDateTime(TimeOnly.MaxValue))
                : await _context.Baselineassessments
                    .FirstOrDefaultAsync(b => b.EmployeeId == employeeId
                        && b.Time.HasValue
                        && b.Time.Value.Year == selectedMonth.Year
                        && b.Time.Value.Month == selectedMonth.Month);

            var analysis = startDate.HasValue && endDate.HasValue
                ? await _context.Analyses
                    .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
                        && a.Time.HasValue
                        && a.Time.Value >= startDate.Value.ToDateTime(TimeOnly.MinValue)
                        && a.Time.Value <= endDate.Value.ToDateTime(TimeOnly.MaxValue))
                : await _context.Analyses
                    .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
                        && a.Time.HasValue
                        && a.Time.Value.Year == selectedMonth.Year
                        && a.Time.Value.Month == selectedMonth.Month);

            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            var viewModel = new EmployeeDetailsViewModel
            {
                Employee = employee,
                Jobs = jobs,
                BaselineAssessment = baseline,
                Analysis = analysis,
                SelectedMonth = selectedMonth
            };

            if (hasFilters || Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Details", viewModel);
            }

            // Nếu không có bộ lọc và không phải AJAX, trả về View "Details" đầy đủ
            return View("_Details", viewModel); // Chỉ định rõ tên view "Details"
        }
        public class JobUpdateModel
        {
            public long Id { get; set; }
            public float? VolumeAssessment { get; set; }
            public float? ProgressAssessment { get; set; }
            public float? QualityAssessment { get; set; }
        }
        private async Task UpdateBaselineAssessment(long? employeeId)
        {
            if (employeeId == null)
                return;

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // Lấy danh sách công việc của nhân viên trong tháng hiện tại có đánh giá
            var jobs = await _context.Jobs
                .Where(j => j.EmployeeId == employeeId
                         && j.Time.HasValue
                         && j.Time.Value.Month == currentMonth
                         && j.Time.Value.Year == currentYear
                         && (j.Status == 1 || j.Status == 3)) // Chỉ tính các công việc "Hoàn thành" hoặc "Hoàn thành muộn"
                .ToListAsync();

            if (!jobs.Any())
                return;

            // Tính tổng các đánh giá
            double sumVolume = jobs.Sum(j => j.VolumeAssessment ?? 0);
            double sumProgress = jobs.Sum(j => j.ProgressAssessment ?? 0);
            double sumQuality = jobs.Sum(j => j.QualityAssessment ?? 0);
            double sumSummary = jobs.Sum(j => j.SummaryOfReviews ?? 0);

            // Xác định trạng thái Evaluate (giả sử tổng Summary >= 6 là đạt, bạn có thể điều chỉnh ngưỡng)
            bool evaluate = sumSummary >= 45;

            // Tìm bản ghi BaselineAssessment của nhân viên trong tháng hiện tại
            var baseline = await _context.Baselineassessments
                .FirstOrDefaultAsync(b => b.EmployeeId == employeeId
                                       && b.Time.HasValue
                                       && b.Time.Value.Month == currentMonth
                                       && b.Time.Value.Year == currentYear);

            if (baseline == null)
            {
                // Nếu chưa có bản ghi trong tháng, tạo mới
                baseline = new Baselineassessment
                {
                    EmployeeId = employeeId,
                    VolumeAssessment = sumVolume,
                    ProgressAssessment = sumProgress,
                    QualityAssessment = sumQuality,
                    SummaryOfReviews = sumSummary,
                    Time = new DateTime(currentYear, currentMonth, 1),
                    Evaluate = evaluate,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now
                };
                _context.Baselineassessments.Add(baseline);
            }
            else
            {
                // Nếu đã có bản ghi trong tháng, cập nhật dữ liệu
                baseline.VolumeAssessment = sumVolume;
                baseline.ProgressAssessment = sumProgress;
                baseline.QualityAssessment = sumQuality;
                baseline.SummaryOfReviews = sumSummary;
                baseline.Evaluate = evaluate;
                baseline.UpdateDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

    }
}
public class EmployeeDetailsViewModel
{
    public Employee Employee { get; set; }
    public List<Job> Jobs { get; set; }
    public Baselineassessment BaselineAssessment { get; set; }
    public Analysis Analysis { get; set; }
    public DateTime SelectedMonth { get; set; }
}