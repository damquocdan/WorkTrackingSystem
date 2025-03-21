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
    public class ScoresController : Controller
    {
        private readonly WorkTrackingSystemContext _context;

        public ScoresController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: ProjectManager/Scores
        public async Task<IActionResult> Index(
    string searchText = "",
    string month = "",
    string status = "",
    string categoryId = "",
    string sortOrder = "",
    bool showCompletedZeroReview = false,
    bool dueToday = false,
    long? jobId = null)
        {
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");
            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
            }

            var manager = await _context.Users
                .Where(u => u.UserName == managerUsername)
                .Include(u => u.Employee)
                .Select(u => u.Employee)
                .FirstOrDefaultAsync();
            if (manager == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var managedDepartments = await _context.Departments
                .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3))
                .Select(d => d.Id)
                .ToListAsync();

            var employeesInManagedDepartments = await _context.Employees
                .Where(e => e.DepartmentId.HasValue && managedDepartments.Contains(e.DepartmentId.Value))
                .Select(e => new { Value = e.Id.ToString(), Text = $"{e.Code} {e.FirstName} {e.LastName}", Avatar = e.Avatar ?? "/images/default-avatar.png" })
                .ToListAsync();
            ViewBag.EmployeeList = employeesInManagedDepartments;

            ViewData["Categories"] = await _context.Categories.ToListAsync();

            // Lấy danh sách JobId đã được gán cho bất kỳ nhân viên nào (EmployeeId không null)
            var assignedJobIds = await _context.Jobmapemployees
                .Where(jme => jme.EmployeeId != null)
                .Select(jme => jme.JobId)
                .Distinct()
                .ToListAsync();

            var scoresQuery = _context.Scores
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.Employee)
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.Job)
                        .ThenInclude(j => j.Category)
                .Where(s => s.JobMapEmployee != null && s.JobMapEmployee.Job != null)
                .Where(s =>
                    (s.JobMapEmployee.EmployeeId.HasValue && employeesInManagedDepartments.Select(e => long.Parse(e.Value)).Contains(s.JobMapEmployee.EmployeeId.Value))
                    || (s.JobMapEmployee.EmployeeId == null && s.CreateBy == managerUsername && !assignedJobIds.Contains(s.JobMapEmployee.JobId)));

            // Apply the filter only if jobId is provided (keeping the original jobId-specific logic)
            if (jobId.HasValue)
            {
                bool hasAssignedJob = await _context.Jobmapemployees
                    .Where(jme => jme.JobId == jobId.Value && jme.EmployeeId.HasValue)
                    .AnyAsync();

                if (hasAssignedJob)
                {
                    scoresQuery = scoresQuery.Where(s =>
                        !(s.JobMapEmployee.JobId == jobId.Value && s.JobMapEmployee.EmployeeId == null));
                }
            }

            // 1. Tìm kiếm theo mã / tên nhân viên / công việc
            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.ToLower();
                scoresQuery = scoresQuery.Where(s =>
                    (s.JobMapEmployee.Employee != null && s.JobMapEmployee.Employee.Code.ToLower().Contains(searchText)) ||
                    (s.JobMapEmployee.Employee != null && s.JobMapEmployee.Employee.FirstName.ToLower().Contains(searchText)) ||
                    (s.JobMapEmployee.Employee != null && s.JobMapEmployee.Employee.LastName.ToLower().Contains(searchText)) ||
                    s.JobMapEmployee.Job.Name.ToLower().Contains(searchText));
            }

            // 2. Lọc theo tháng
            if (!string.IsNullOrEmpty(month) && DateTime.TryParse(month + "-01", out DateTime selectedMonth))
            {
                scoresQuery = scoresQuery.Where(s => s.Time.HasValue && s.Time.Value.Year == selectedMonth.Year && s.Time.Value.Month == selectedMonth.Month);
            }

            // 3. Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status) && byte.TryParse(status, out byte statusValue))
            {
                scoresQuery = scoresQuery.Where(s => s.Status == statusValue);
            }

            // 4. Lọc theo danh mục
            if (!string.IsNullOrEmpty(categoryId) && long.TryParse(categoryId, out long catId))
            {
                scoresQuery = scoresQuery.Where(s => s.JobMapEmployee.Job.CategoryId == catId);
            }

            // 5. Hiển thị công việc hoàn thành nhưng chưa đánh giá
            if (showCompletedZeroReview)
            {
                scoresQuery = scoresQuery.Where(s => s.Status == 1 && (!s.SummaryOfReviews.HasValue || s.SummaryOfReviews == 0));
            }

            var scores = await scoresQuery.ToListAsync();

            return View(scores);
        }
        [HttpGet]
        public async Task<IActionResult> GetJobsByEmployee(long employeeId)
        {
            try
            {
                var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");
                if (string.IsNullOrEmpty(managerUsername))
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập." });
                }

                var manager = await _context.Users
                    .Where(u => u.UserName == managerUsername)
                    .Select(u => u.Employee)
                    .FirstOrDefaultAsync();

                if (manager == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin quản lý." });
                }

                var managedDepartments = await _context.Departments
                    .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3))
                    .Select(d => d.Id)
                    .ToListAsync();

                // Lấy danh sách JobId đã được gán cho bất kỳ nhân viên nào (Employee_Id không null)
                var assignedJobIds = await _context.Jobmapemployees // Truy cập trực tiếp bảng JOBMAPEMPLOYEE
                    .Where(jme => jme.EmployeeId != null)
                    .Select(jme => jme.JobId)
                    .Distinct()
                    .ToListAsync();

                // Công việc chưa giao và không nằm trong danh sách JobId đã gán
                var unassignedJobs = await _context.Scores
                    .Include(s => s.JobMapEmployee)
                        .ThenInclude(jme => jme.Job)
                            .ThenInclude(j => j.Category)
                    .Where(s => s.JobMapEmployee.EmployeeId == null
                             && s.CreateBy == managerUsername
                             && !assignedJobIds.Contains(s.JobMapEmployee.JobId)) // Loại bỏ toàn bộ JobId đã được gán
                    .Select(s => new
                    {
                        Id = s.JobMapEmployee.JobId,
                        Name = s.JobMapEmployee.Job.Name,
                        CategoryName = s.JobMapEmployee.Job.Category != null ? s.JobMapEmployee.Job.Category.Name : "N/A",
                        Time = s.Time,
                        Deadline1 = s.JobMapEmployee.Job.Deadline1,
                        Status = s.Status,
                        CompletionDate = s.CompletionDate
                    })
                    .ToListAsync();

                // Công việc đã giao cho nhân viên cụ thể
                var assignedJobs = await _context.Scores
                    .Include(s => s.JobMapEmployee)
                        .ThenInclude(jme => jme.Job)
                            .ThenInclude(j => j.Category)
                    .Where(s => s.JobMapEmployee.EmployeeId == employeeId)
                    .Select(s => new
                    {
                        Id = s.JobMapEmployee.JobId,
                        Name = s.JobMapEmployee.Job.Name,
                        CategoryName = s.JobMapEmployee.Job.Category != null ? s.JobMapEmployee.Job.Category.Name : "N/A",
                        Time = s.Time,
                        Deadline1 = s.JobMapEmployee.Job.Deadline1,
                        Status = s.Status,
                        CompletionDate = s.CompletionDate
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    unassignedJobs = unassignedJobs,
                    assignedJobs = assignedJobs
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignEmployee(long jobId, long employeeId, string time, string deadline)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null)
            {
                return Json(new { success = false, message = "Job not found" });
            }

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return Json(new { success = false, message = "Employee not found" });
            }

            // Tạo JobMapEmployee
            var jobMapEmployee = new Jobmapemployee
            {
                JobId = jobId,
                EmployeeId = employeeId,
                CreateBy = HttpContext.Session.GetString("ProjectManagerLogin"),
                CreateDate = DateTime.Now,
                IsActive = true,
                IsDelete = false
            };

            _context.Jobmapemployees.Add(jobMapEmployee);
            await _context.SaveChangesAsync();

            // Xử lý ngày bắt đầu và ngày kết thúc
            DateTime? parsedTime = null;
            DateOnly? parsedDeadline = null;

            if (!string.IsNullOrEmpty(time) && DateTime.TryParseExact(time, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime startDate))
            {
                parsedTime = startDate;
            }

            if (!string.IsNullOrEmpty(deadline) && DateOnly.TryParseExact(deadline, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateOnly endDate))
            {
                parsedDeadline = endDate;
                job.Deadline1 = parsedDeadline;
                _context.Jobs.Update(job);
            }

            // Tạo Score
            var score = new Score
            {
                JobMapEmployeeId = jobMapEmployee.Id,
                Time = parsedTime ?? DateTime.Now,
                Status = 0,
                CreateBy = HttpContext.Session.GetString("ProjectManagerLogin")
            };
            _context.Scores.Add(score);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateJobDate(long jobId, string field, string date)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null)
            {
                return Json(new { success = false, message = "Job not found" });
            }

            if (!DateTime.TryParse(date, out DateTime parsedDate))
            {
                return Json(new { success = false, message = "Invalid date format" });
            }

            switch (field.ToLower())
            {
                case "time":
                    var score = await _context.Scores
                        .Where(s => s.JobMapEmployee.JobId == jobId)
                        .FirstOrDefaultAsync();
                    if (score != null)
                    {
                        score.Time = parsedDate;
                        _context.Scores.Update(score);
                    }
                    break;
                //case "deadline1":
                //    job.Deadline1 = parsedDate;
                //    _context.Jobs.Update(job);
                //    break;
                default:
                    return Json(new { success = false, message = "Invalid field" });
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        public IActionResult Createjob()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Createjob");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Createjob([Bind("Id,CategoryId,Name,Description,Deadline1,Deadline2,Deadline3,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Job job)
        {
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");
            if (ModelState.IsValid)
            {
                // Tạo Job
                job.CreateBy = managerUsername;
                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();

                // Tạo JobMapEmployee (Gán JobId)
                var jobMapEmployee = new Jobmapemployee
                {
                    JobId = job.Id,
                    EmployeeId = null, // Chưa có Employee, cần cập nhật sau nếu cần
                    IsDelete = false,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    CreateBy = job.CreateBy,
                    UpdateBy = job.UpdateBy
                };

                _context.Jobmapemployees.Add(jobMapEmployee);
                await _context.SaveChangesAsync(); // Lưu để lấy Id của JobMapEmployee

                // Tạo Score (Gán JobMapEmployeeId)
                var score = new Score
                {
                    JobMapEmployeeId = jobMapEmployee.Id,
                    Status = 0, // Mặc định trạng thái chưa bat dau
                    IsDelete = false,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    CreateBy = job.CreateBy,
                    UpdateBy = job.UpdateBy
                };

                _context.Scores.Add(score);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", job.CategoryId);
            return View(job);
        }
        // GET: ProjectManager/Scores/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var score = await _context.Scores
                .Include(s => s.JobMapEmployee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (score == null)
            {
                return NotFound();
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Details",score);
            }
            return View(score);
        }

        // GET: ProjectManager/Scores/Create
        public IActionResult Create()
        {
            ViewData["JobMapEmployeeId"] = new SelectList(_context.Jobmapemployees, "Id", "Id");

            return View();
        }

        // POST: ProjectManager/Scores/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,JobMapEmployeeId,CompletionDate,Status,VolumeAssessment,ProgressAssessment,QualityAssessment,SummaryOfReviews,Progress,Time,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Score score)
        {
            if (ModelState.IsValid)
            {
                _context.Add(score);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["JobMapEmployeeId"] = new SelectList(_context.Jobmapemployees, "Id", "Id", score.JobMapEmployeeId);
            return View(score);
        }

        // GET: ProjectManager/Scores/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var score = await _context.Scores.FindAsync(id);
            if (score == null)
            {
                return NotFound();
            }
            ViewData["JobMapEmployeeId"] = new SelectList(_context.Jobmapemployees, "Id", "Id", score.JobMapEmployeeId);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Edit", score);
            }
            return View(score);
        }

        // POST: ProjectManager/Scores/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,JobMapEmployeeId,CompletionDate,Status,VolumeAssessment,ProgressAssessment,QualityAssessment,SummaryOfReviews,Progress,Time,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Score score)
        {
            if (id != score.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(score);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScoreExists(score.Id))
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
            ViewData["JobMapEmployeeId"] = new SelectList(_context.Jobmapemployees, "Id", "Id", score.JobMapEmployeeId);
            return View(score);
        }

        // GET: ProjectManager/Scores/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var score = await _context.Scores
                .Include(s => s.JobMapEmployee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (score == null)
            {
                return NotFound();
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Delete", score);
            }
            return View(score);
        }

        // POST: ProjectManager/Scores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var score = await _context.Scores.FindAsync(id);
            if (score != null)
            {
                _context.Scores.Remove(score);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ScoreExists(long id)
        {
            return _context.Scores.Any(e => e.Id == id);
        }
    }
}
public class JobScoreViewModel
{
    public long? JobId { get; set; }
    public string JobName { get; set; }
    public string CategoryName { get; set; }
    public string EmployeeCode { get; set; }
    public string EmployeeFirstName { get; set; }
    public string EmployeeLastName { get; set; }
    public DateTime? Time { get; set; } // Thời gian từ Score hoặc CreateDate từ Job
    public byte? Status { get; set; }
    public DateTime? CompletionDate { get; set; }
    public double? VolumeAssessment { get; set; }
    public double? ProgressAssessment { get; set; }
    public double? QualityAssessment { get; set; }
    public double? SummaryOfReviews { get; set; }
    public bool IsAssigned => EmployeeCode != null; // Kiểm tra xem công việc đã được giao chưa
}