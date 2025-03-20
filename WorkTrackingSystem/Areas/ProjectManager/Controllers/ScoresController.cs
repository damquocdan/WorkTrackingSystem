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
    bool dueToday = false)
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

            // Lấy danh sách danh mục để hiển thị trong form
            ViewData["Categories"] = await _context.Categories.ToListAsync();

            // Truy vấn cơ bản từ Scores
            var scoresQuery = _context.Scores
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.Employee)
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.Job)
                        .ThenInclude(j => j.Category)
                .Where(s => s.JobMapEmployee != null && s.JobMapEmployee.Job != null)
                .Where(s =>
                    (s.JobMapEmployee.EmployeeId.HasValue && employeesInManagedDepartments.Select(e => long.Parse(e.Value)).Contains(s.JobMapEmployee.EmployeeId.Value))
                    || (s.JobMapEmployee.EmployeeId == null && s.CreateBy == managerUsername));

            // Lấy công việc chưa giao từ Jobs
            var unassignedJobsQuery = _context.Jobs
                .Include(j => j.Category)
                .Where(j => j.CreateBy == managerUsername
                    && !_context.Jobmapemployees.Any(jme => jme.JobId == j.Id) // Chưa có JobMapEmployee
                    && !_context.Scores.Any(s => s.JobMapEmployee != null && s.JobMapEmployee.JobId == j.Id)); // Chưa có Score

            // Áp dụng bộ lọc từ form

            // 1. Tìm kiếm theo mã / tên nhân viên / công việc
            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.ToLower();
                scoresQuery = scoresQuery.Where(s =>
                    (s.JobMapEmployee.Employee != null && s.JobMapEmployee.Employee.Code.ToLower().Contains(searchText)) ||
                    (s.JobMapEmployee.Employee != null && s.JobMapEmployee.Employee.FirstName.ToLower().Contains(searchText)) ||
                    (s.JobMapEmployee.Employee != null && s.JobMapEmployee.Employee.LastName.ToLower().Contains(searchText)) ||
                    s.JobMapEmployee.Job.Name.ToLower().Contains(searchText));

                unassignedJobsQuery = unassignedJobsQuery.Where(j =>
                    j.Name.ToLower().Contains(searchText));
            }

            // 2. Lọc theo tháng
            if (!string.IsNullOrEmpty(month) && DateTime.TryParse(month + "-01", out DateTime selectedMonth))
            {
                scoresQuery = scoresQuery.Where(s => s.Time.HasValue && s.Time.Value.Year == selectedMonth.Year && s.Time.Value.Month == selectedMonth.Month);
                unassignedJobsQuery = unassignedJobsQuery.Where(j => j.CreateDate.HasValue && j.CreateDate.Value.Year == selectedMonth.Year && j.CreateDate.Value.Month == selectedMonth.Month);
            }

            // 3. Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status) && byte.TryParse(status, out byte statusValue))
            {
                scoresQuery = scoresQuery.Where(s => s.Status == statusValue);
                // Công việc chưa giao không có trạng thái, nên không cần lọc unassignedJobsQuery
            }

            // 4. Lọc theo danh mục
            if (!string.IsNullOrEmpty(categoryId) && long.TryParse(categoryId, out long catId))
            {
                scoresQuery = scoresQuery.Where(s => s.JobMapEmployee.Job.CategoryId == catId);
                unassignedJobsQuery = unassignedJobsQuery.Where(j => j.CategoryId == catId);
            }

            // 5. Hiển thị công việc hoàn thành nhưng chưa đánh giá
            if (showCompletedZeroReview)
            {
                scoresQuery = scoresQuery.Where(s => s.Status == 1 && (!s.SummaryOfReviews.HasValue || s.SummaryOfReviews == 0));
                // Công việc chưa giao không áp dụng điều kiện này
            }

            // Thực thi truy vấn Scores
            var scores = await scoresQuery.ToListAsync();

            // Thực thi truy vấn Jobs và tạo Score giả
            var unassignedJobs = await unassignedJobsQuery.ToListAsync();
            var fakeScores = unassignedJobs.Select(j => new Score
            {
                Id = 0, // ID giả
                JobMapEmployee = new Jobmapemployee
                {
                    JobId = j.Id,
                    Job = j,
                    EmployeeId = null,
                    Employee = null
                },
                Time = j.CreateDate ?? DateTime.Now,
                Status = null,
                CompletionDate = null,
                VolumeAssessment = null,
                ProgressAssessment = null,
                QualityAssessment = null,
                SummaryOfReviews = null,
                CreateBy = managerUsername
            }).ToList();

            // Kết hợp danh sách
            var combinedScores = scores.Concat(fakeScores);

            // 7. Sắp xếp
            switch (sortOrder.ToLower())
            {
                case "due_asc":
                    combinedScores = combinedScores.OrderBy(s => s.JobMapEmployee.Job.Deadline1);
                    break;
                case "due_desc":
                    combinedScores = combinedScores.OrderByDescending(s => s.JobMapEmployee.Job.Deadline1);
                    break;
                case "review_asc":
                    combinedScores = combinedScores.OrderBy(s => s.SummaryOfReviews ?? double.MaxValue); // Đưa null xuống cuối
                    break;
                case "review_desc":
                    combinedScores = combinedScores.OrderByDescending(s => s.SummaryOfReviews ?? double.MinValue); // Đưa null lên đầu
                    break;
                default:
                    combinedScores = combinedScores.OrderByDescending(s => s.Time);
                    break;
            }

            return View(combinedScores.ToList());
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

                // Công việc chưa giao
                var unassignedJobs = await _context.Jobs
                    .Include(j => j.Category)
                    .Where(j => j.CreateBy == managerUsername
                        && !_context.Jobmapemployees.Any(jme => jme.JobId == j.Id))
                    .Select(j => new
                    {
                        Id = j.Id,
                        Name = j.Name,
                        CategoryName = j.Category != null ? j.Category.Name : "N/A",
                        Time = j.CreateDate,
                        Deadline1 = j.Deadline1,
                        Status = (byte?)null,
                        CompletionDate = (DateTime?)null
                    })
                    .ToListAsync();

                // Công việc đã giao cho nhân viên
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
        public async Task<IActionResult> AssignEmployee(long jobId, long employeeId)
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

            var score = new Score
            {
                JobMapEmployeeId = jobMapEmployee.Id,
                Time = DateTime.Now,
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
                    Time = DateTime.Now,
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