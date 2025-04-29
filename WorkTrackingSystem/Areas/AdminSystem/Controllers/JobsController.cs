using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using WorkTrackingSystem.Models;
using WorkTrackingSystem.Areas.AdminSystem.Models;
using OfficeOpenXml.Drawing.Chart;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using X.PagedList.Extensions;
using X.PagedList;

namespace WorkTrackingSystem.Areas.AdminSystem.Controllers
{
    [Area("AdminSystem")]
    public class JobsController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public JobsController(WorkTrackingSystemContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(
            int? DepartmentId,
string searchText = "",
string month = "",
string day = "",
string year = "", // Thêm tham số year
string quarter = "", // Thêm tham số quarter
string quarterYear = "",
string fromDate = "", // Thêm tham số fromDate
string toDate = "", // Thêm tham số toDate
string status = "",
string categoryId = "",
string sortOrder = "",
bool showCompletedZeroReview = false,
bool dueToday = false,
long? jobId = null,
int page = 1)
        {
            var limit = 10;
           

          

            var employeesInManagedDepartments = await _context.Employees
            
                .Select(e => new
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.Code} {e.FirstName} {e.LastName}",
                    Avatar = e.Avatar ?? "/images/default-avatar.png"
                })
                .ToListAsync();

            ViewBag.EmployeeList = employeesInManagedDepartments;
            ViewData["Categories"] = await _context.Categories.ToListAsync();

            var assignedJobIds = await _context.Jobmapemployees
                .Where(jme => jme.EmployeeId != null)
                .Select(jme => jme.JobId)
                .Distinct()
                .ToListAsync();

            var scoresQuery = _context.Scores
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.Employee)
                    .ThenInclude(e=>e.Department)
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.Job)
                        .ThenInclude(j => j.Category)
                .Where(s => s.JobMapEmployee != null && s.JobMapEmployee.Job != null)
                .Where(s =>
                    (s.JobMapEmployee.EmployeeId.HasValue && employeesInManagedDepartments.Select(e => long.Parse(e.Value)).Contains(s.JobMapEmployee.EmployeeId.Value)) ||
                    (s.JobMapEmployee.EmployeeId == null &&  !assignedJobIds.Contains(s.JobMapEmployee.JobId))
                );

            // Filter theo JobId cụ thể
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

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.ToLower();
                scoresQuery = scoresQuery.Where(s =>
                    (s.JobMapEmployee.Employee != null && (
                        s.JobMapEmployee.Employee.Code.ToLower().Contains(searchText) ||
                        s.JobMapEmployee.Employee.FirstName.ToLower().Contains(searchText) ||
                        s.JobMapEmployee.Employee.LastName.ToLower().Contains(searchText)
                    )) ||
                    s.JobMapEmployee.Job.Name.ToLower().Contains(searchText)
                );
            }
            //lọc theo phòng ban
            if (DepartmentId.HasValue && DepartmentId > 0)
            {
                scoresQuery = scoresQuery.Where(s => s.JobMapEmployee.Employee.DepartmentId == DepartmentId);
            }

            // Lọc theo năm
            if (!string.IsNullOrEmpty(year) && int.TryParse(year, out int selectedYear))
            {
                scoresQuery = scoresQuery.Where(s =>
                    s.CreateDate.HasValue && s.CreateDate.Value.Year == selectedYear);
            }

            // Lọc theo quý
            // Filter by year if selected
            if (!string.IsNullOrEmpty(quarterYear) && int.TryParse(quarterYear, out int selectedQuarterYear))
            {
                scoresQuery = scoresQuery.Where(s =>
                    s.CreateDate.HasValue && s.CreateDate.Value.Year == selectedQuarterYear);
            }

            // Filter by quarter if selected
            if (!string.IsNullOrEmpty(quarter) && int.TryParse(quarter, out int selectedQuarter))
            {
                var startMonth = (selectedQuarter - 1) * 3 + 1;
                var endMonth = startMonth + 2;
                scoresQuery = scoresQuery.Where(s =>
                    s.CreateDate.HasValue &&
                    s.CreateDate.Value.Month >= startMonth &&
                    s.CreateDate.Value.Month <= endMonth);
            }

            // Lọc theo tháng
            if (!string.IsNullOrEmpty(month) && DateTime.TryParse(month + "-01", out DateTime selectedMonth))
            {
                scoresQuery = scoresQuery.Where(s =>
                    s.CreateDate.HasValue &&
                    s.CreateDate.Value.Year == selectedMonth.Year &&
                    s.CreateDate.Value.Month == selectedMonth.Month);
            }

            // Lọc theo ngày
            if (!string.IsNullOrEmpty(day) && DateTime.TryParse(day, out DateTime selectedDay))
            {
                scoresQuery = scoresQuery.Where(s =>
                    s.CreateDate.HasValue && s.CreateDate.Value.Date == selectedDay.Date);
            }

            // Lọc theo khoảng thời gian
            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate) &&
                DateTime.TryParse(fromDate, out DateTime startDate) && DateTime.TryParse(toDate, out DateTime endDate))
            {
                scoresQuery = scoresQuery.Where(s =>
                    s.CreateDate.HasValue && s.CreateDate.Value.Date >= startDate.Date && s.CreateDate.Value.Date <= endDate.Date);
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status) && byte.TryParse(status, out byte statusValue))
            {
                scoresQuery = scoresQuery.Where(s => s.Status == statusValue);
            }

            // Lọc theo danh mục
            if (!string.IsNullOrEmpty(categoryId) && long.TryParse(categoryId, out long catId))
            {
                scoresQuery = scoresQuery.Where(s => s.JobMapEmployee.Job.CategoryId == catId);
            }

            // Công việc hoàn thành nhưng chưa đánh giá
            if (showCompletedZeroReview)
            {
                scoresQuery = scoresQuery.Where(s =>
                    s.Status == 1 && (!s.SummaryOfReviews.HasValue || s.SummaryOfReviews == 0));
            }

            // Công việc hết hạn hôm nay
            if (dueToday)
            {
                scoresQuery = scoresQuery.Where(s => s.Time == s.CreateDate);
            }

            // Sắp xếp: công việc chưa được giao (null) hiển thị lên đầu, sau đó theo ngày mới nhất
            switch (sortOrder)
            {
                case "due_asc":
                    scoresQuery = scoresQuery.OrderBy(s => s.JobMapEmployee.Job.Deadline1);
                    break;
                case "due_desc":
                    scoresQuery = scoresQuery.OrderByDescending(s => s.JobMapEmployee.Job.Deadline1);
                    break;
                case "review_asc":
                    scoresQuery = scoresQuery.OrderBy(s => s.SummaryOfReviews ?? 0); // Handle null reviews
                    break;
                case "review_desc":
                    scoresQuery = scoresQuery.OrderByDescending(s => s.SummaryOfReviews ?? 0); // Handle null reviews
                    break;
                default:
                    scoresQuery = scoresQuery
                        .OrderBy(s => s.JobMapEmployee.EmployeeId.HasValue) // false (null) lên đầu
                        .ThenByDescending(s => s.CreateDate);
                    break;
            }

            var scores = scoresQuery.ToPagedList(page, limit);
			ViewBag.Department = new SelectList(_context.Departments, "Id", "Name");
			// Gán ViewBag để giữ trạng thái lọc/sắp xếp
			ViewBag.SearchText = searchText;
            ViewBag.Month = month;
            ViewBag.Day = day;
            ViewBag.Year = year; // Thêm ViewBag cho năm
            ViewBag.Quarter = quarter; // Thêm ViewBag cho quý
            ViewBag.FromDate = fromDate; // Thêm ViewBag cho từ ngày
            ViewBag.ToDate = toDate; // Thêm ViewBag cho đến ngày
            ViewBag.Status = status?.ToString();
            ViewBag.CategoryId = categoryId?.ToString();
            ViewBag.SortOrder = sortOrder;
            ViewBag.ShowCompletedZeroReview = showCompletedZeroReview;
            ViewBag.DueToday = dueToday;
			ViewBag.DepartmentId = DepartmentId;
			return View(scores);
        }

        public async Task<IActionResult> GetJobsByEmployee(long employeeId)
        {
            try
            {
                //var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");
                //if (string.IsNullOrEmpty(managerUsername))
                //{
                //    return Json(new { success = false, message = "Vui lòng đăng nhập." });
                //}

                //var manager = await _context.Users
                //    .Where(u => u.UserName == managerUsername)
                //    .Select(u => u.Employee)
                //    .FirstOrDefaultAsync();

                //if (manager == null)
                //{
                //    return Json(new { success = false, message = "Không tìm thấy thông tin quản lý." });
                //}

                //var managedDepartments = await _context.Departments
                //    .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3))
                //    .Select(d => d.Id)
                //    .ToListAsync();

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
                             //&& s.CreateBy == managerUsername
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
                CreateBy = HttpContext.Session.GetString("AdminLogin"),
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
                CreateDate = job.Deadline1?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
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
                        score.CreateDate = job.Deadline1?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now;
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
                    CreateDate = null,
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

        public IActionResult Create(int? currentJobMapEmployeeId)
        {
           

            var manager = _context.Users
                
                .Include(u => u.Employee)
                .Select(u => u.Employee)
                .FirstOrDefault();

            if (manager == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var currentJobMapEmployee = _context.Jobmapemployees
                .Include(j => j.Job)
                .FirstOrDefault(j => j.Id == currentJobMapEmployeeId);

            if (currentJobMapEmployee == null)
            {
                return BadRequest("Không tìm thấy công việc hiện tại");
            }

            var managedDepartments = _context.Departments
                .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3))
                .Select(d => d.Id)
                .ToList();

            var currentEmployeeId = currentJobMapEmployee.EmployeeId;
            var employeesInManagedDepartments = _context.Employees
                .Where(e => e.DepartmentId.HasValue &&
                           managedDepartments.Contains(e.DepartmentId.Value) &&
                           e.Id != currentEmployeeId)
                .Select(e => new {
                    Value = e.Id.ToString(),
                    Text = $"{e.Code} {e.FirstName} {e.LastName}",
                    Avatar = e.Avatar ?? "/images/default-avatar.png"
                })
                .ToList();

            ViewBag.EmployeeList = employeesInManagedDepartments;
            ViewBag.CurrentJobMapEmployeeId = currentJobMapEmployeeId;
            // Truyền thêm Job để sử dụng Deadline1 trong view nếu cần
            ViewBag.Job = currentJobMapEmployee.Job;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Create");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int employeeId, DateOnly deadline, int currentJobMapEmployeeId)
        {
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");
            var currentJobMapEmployee = await _context.Jobmapemployees
                .Include(j => j.Job)
                .FirstOrDefaultAsync(j => j.Id == currentJobMapEmployeeId);

            if (currentJobMapEmployee == null)
            {
                return BadRequest("Không tìm thấy công việc hiện tại");
            }

            // Vô hiệu hóa Score cũ của currentJobMapEmployee
            var oldScores = await _context.Scores
               .Where(s => s.JobMapEmployeeId == currentJobMapEmployeeId)
                .ToListAsync();

            foreach (var oldScore in oldScores)
            {
                oldScore.IsActive = false;
            }

            _context.Scores.UpdateRange(oldScores);

            // Tạo JobMapEmployee mới
            var newJobMapEmployee = new Jobmapemployee
            {
                JobId = currentJobMapEmployee.JobId,
                EmployeeId = employeeId,
            };

            _context.Jobmapemployees.Add(newJobMapEmployee);
            await _context.SaveChangesAsync();

            // Cập nhật deadline cho Job (gán vào Deadline2 hoặc Deadline3)
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == currentJobMapEmployee.JobId);
            if (job != null)
            {
                if (job.Deadline2 == null)
                {
                    job.Deadline2 = deadline;
                }
                else if (job.Deadline3 == null)
                {
                    job.Deadline3 = deadline;
                }
                _context.Jobs.Update(job);
            }

            // Tạo Score mới với Time = Deadline1 của Job
            var score = new Score
            {
                CreateBy = managerUsername,
                JobMapEmployeeId = newJobMapEmployee.Id,
                Status = 0,
                Time = job.Deadline1.HasValue ? DateTime.Parse(job.Deadline1.Value.ToString("yyyy-MM-dd")) : DateTime.Now,
                CreateDate = deadline.ToDateTime(TimeOnly.MinValue),

                IsActive = true,
                IsDelete = false
            };

            await UpdateBaselineAssessment(newJobMapEmployee.EmployeeId);
            await UpdateAnalysis(newJobMapEmployee.EmployeeId);

            _context.Add(score);
            await _context.SaveChangesAsync();

            // Trả về JSON để AJAX xử lý
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction("Index");
        }
        // GET: ProjectManager/Scores/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var score = await _context.Scores
                .Include(s => s.JobMapEmployee)
                .ThenInclude(jme => jme.Job)
                .ThenInclude(j => j.Category)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (score == null)
            {
                return NotFound();
            }


            ViewData["JobMapEmployeeId"] = new SelectList(_context.Jobmapemployees, "Id", "Id", score.JobMapEmployeeId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", score.JobMapEmployee?.Job?.CategoryId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Edit", score);
            }
            return View(score);
        }

        // POST: Scores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, Score score)
        {
            if (id != score.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingScore = await _context.Scores
                        .Include(s => s.JobMapEmployee)
                        .ThenInclude(jme => jme.Job)
                        .FirstOrDefaultAsync(s => s.Id == id);

                    if (existingScore == null)
                    {
                        return NotFound();
                    }

                    // Cập nhật các thuộc tính của Score
                    existingScore.CreateDate = score.CreateDate;
                    existingScore.CompletionDate = score.CompletionDate;
                    existingScore.Status = score.Status;
                    existingScore.VolumeAssessment = score.VolumeAssessment;
                    existingScore.ProgressAssessment = score.ProgressAssessment;
                    existingScore.QualityAssessment = score.QualityAssessment;
                    existingScore.SummaryOfReviews = score.SummaryOfReviews;
                    existingScore.Progress = score.Progress;
                    existingScore.Time = score.Time;

                    //// Xử lý logic Deadline
                 
                    if (existingScore.JobMapEmployee?.EmployeeId != null)
                    {
                        var job = existingScore.JobMapEmployee.Job;
                        if (job != null)
                        {
                            job.Name = score.JobMapEmployee?.Job?.Name;
                            job.CategoryId = score.JobMapEmployee?.Job?.CategoryId;
                        }


                        if (job != null && score.CreateDate.HasValue)
                        {
                            // Chuyển CreateDate (DateTime?) sang DateOnly
                            DateOnly createDateAsDateOnly = DateOnly.FromDateTime(score.CreateDate.Value);

                            // Trường hợp 3: Nếu Deadline1, Deadline2, và Deadline3 đều có giá trị
                            if (job.Deadline1.HasValue && job.Deadline2.HasValue && job.Deadline3.HasValue)
                            {
                                job.Deadline3 = createDateAsDateOnly;
                            }
                            // Trường hợp 2: Nếu Deadline1 và Deadline2 có giá trị
                            else if (job.Deadline1.HasValue && job.Deadline2.HasValue)
                            {
                                job.Deadline2 = createDateAsDateOnly;
                            }
                            // Trường hợp 1: Nếu Deadline1 là null hoặc (Deadline1 có giá trị nhưng Deadline2 là null)
                            else if (!job.Deadline1.HasValue || (job.Deadline1.HasValue && !job.Deadline2.HasValue))
                            {
                                job.Deadline1 = createDateAsDateOnly;
                            }
                        }
                    }
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
            return View(score);
        }

        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var score = await _context.Scores
                .Include(s => s.JobMapEmployee).Include(s => s.JobMapEmployee.Job).Include(s => s.JobMapEmployee.Employee).Include(s => s.JobMapEmployee.Job.Category)
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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var score = await _context.Scores
                .Include(s => s.JobMapEmployee)
                .ThenInclude(jme => jme.Job)
                .Include(s => s.JobMapEmployee)
                .ThenInclude(jme => jme.Employee)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (score == null)
            {
                return NotFound();
            }

            var jobMapEmployee = score.JobMapEmployee;
            var job = jobMapEmployee?.Job;

            if (jobMapEmployee == null || job == null)
            {
                return NotFound();
            }

            if (jobMapEmployee.EmployeeId == null)
            {
                // Lấy tất cả JobMapEmployee liên quan đến Job
                var relatedJobMapEmployees = await _context.Jobmapemployees
                    .Where(jme => jme.JobId == job.Id)
                    .ToListAsync();

                // Lấy tất cả Score liên quan đến bất kỳ JobMapEmployee nào trong danh sách
                var jobMapEmployeeIds = relatedJobMapEmployees.Select(jme => jme.Id).ToList();
                var relatedScores = await _context.Scores
                    .Where(s => s.JobMapEmployeeId.HasValue && jobMapEmployeeIds.Contains(s.JobMapEmployeeId.Value))
                    .ToListAsync();
                _context.Scores.RemoveRange(relatedScores);

                // Xóa tất cả JobMapEmployee liên quan đến Job
                _context.Jobmapemployees.RemoveRange(relatedJobMapEmployees);

                // Cuối cùng xóa Job
                //job.IsDelete = true;
                //job.IsActive= false;
                _context.Jobs.Remove(job);
            }
            else
            {
                // Xóa Score trước
                _context.Scores.Remove(score);

                // Xóa JobMapEmployee
                _context.Jobmapemployees.Remove(jobMapEmployee);

                // Kiểm tra và xóa Deadline nếu cần
                if (score.CreateDate.HasValue) // Kiểm tra null cho DateTime?
                {
                    var scoreCreateDate = score.CreateDate.Value; // Lấy giá trị từ nullable DateTime

                    if (job.Deadline1.HasValue && scoreCreateDate.Date == job.Deadline1.Value.ToDateTime(TimeOnly.MinValue))
                    {
                        job.Deadline1 = null;
                    }
                    else if (job.Deadline2.HasValue && scoreCreateDate.Date == job.Deadline2.Value.ToDateTime(TimeOnly.MinValue))
                    {
                        job.Deadline2 = null;
                    }
                    else if (job.Deadline3.HasValue && scoreCreateDate.Date == job.Deadline3.Value.ToDateTime(TimeOnly.MinValue))
                    {
                        job.Deadline3 = null;
                    }

                    // Cập nhật Job nếu có thay đổi Deadline
                    if (job.Deadline1 == null || job.Deadline2 == null || job.Deadline3 == null)
                    {
                        job.UpdateDate = DateTime.Now;
                        _context.Jobs.Update(job);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult JobOfEmployee(string search, int? DepartmentId, int page = 1)
		{
			int limit = 10;

			// Lấy danh sách nhân viên + số công việc của từng nhân viên
			var query = _context.Employees
				.Include(e => e.Department)
				.Include(e => e.Position)
				.Include(e => e.Jobmapemployees)
				.AsQueryable(); // Chưa thực thi, giúp tối ưu truy vấn SQL

			// Lọc theo từ khóa tìm kiếm (mã nhân viên hoặc tên nhân viên)
			if (!string.IsNullOrEmpty(search))
			{
				string searchLower = search.ToLower();
				query = query.Where(e =>
					(e.FirstName + " " + e.LastName).ToLower().Contains(searchLower) ||
					e.Code.ToLower().Contains(searchLower));
			}

			// Lọc theo phòng ban (chỉ áp dụng nếu có chọn phòng ban)
			if (DepartmentId.HasValue && DepartmentId > 0)
			{
				query = query.Where(e => e.DepartmentId == DepartmentId);
			}

			// Chuyển danh sách sang dạng danh sách cần thiết, đồng thời đếm công việc
			var employees = query
				.Select(e => new
				{
					Employee = e,
					JobCount = e.Jobmapemployees.Count() // Sử dụng Count() trực tiếp từ Include để tối ưu
				})
				.ToList(); // Chỉ lấy danh sách nhân viên cần thiết

			// Chuyển danh sách nhân viên về List<Employee>
			var filteredEmployees = employees.Select(e => e.Employee).ToList();

			// Chuyển đổi danh sách sang kiểu IPagedList
			var pagedEmployees = filteredEmployees.ToPagedList(page, limit);

			// Gán danh sách số lượng công việc vào ViewBag
			ViewBag.JobCounts = employees.ToDictionary(e => e.Employee.Id, e => e.JobCount);
			ViewBag.Search = search;
			ViewBag.Department = new SelectList(_context.Departments, "Id", "Name");

			return View(pagedEmployees);
		}

        public async Task<IActionResult> EmployeeWork(long id, int? page, string search, string filterStatus)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            // Xử lý search
            search = string.IsNullOrEmpty(search) ? "" : Uri.UnescapeDataString(search).Trim();

            // Lấy thông tin nhân viên
            var employee = await _context.Employees
                .Include(e => e.Department) 
                .Include(e=>e.Position)// Bao gồm thông tin phòng ban nếu cần
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound("Không tìm thấy nhân viên.");
            }

            // Lấy danh sách công việc
            var jobs = _context.Jobmapemployees
                .Include(jm => jm.Job)
                    .ThenInclude(j => j.Category)
                .Include(jm => jm.Scores)
                .Where(jm => jm.EmployeeId == id && jm.IsActive == true && jm.IsDelete == false);

            // Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                jobs = jobs.Where(jm => jm.Job.Name.ToLower().Contains(searchLower) ||
                                       jm.Job.Category.Name.ToLower().Contains(searchLower));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(filterStatus) && int.TryParse(filterStatus, out int statusValue))
            {
                jobs = jobs.Where(jm => jm.Scores.OrderByDescending(s => s.Id)
                                                .Select(s => s.Status)
                                                .FirstOrDefault() == statusValue);
            }

            // Map dữ liệu sang JobViewModel
            var jobList = jobs.Select(jm => new JobViewModel
            {
                JobId = jm.Job.Id,
                JobName = jm.Job.Name,
                CategoryName = jm.Job.Category.Name,
                Deadline = jm.Job.Deadline1,
                CompletionDate = jm.Scores.OrderByDescending(s => s.Id).Select(s => s.CompletionDate).FirstOrDefault(),
                ScoreStatus = jm.Scores.OrderByDescending(s => s.Id).Select(s => s.Status).FirstOrDefault(),
                Progress = jm.Scores.OrderByDescending(s => s.Id).Select(s => s.Progress).FirstOrDefault(),
                VolumeAssessment = jm.Scores.OrderByDescending(s => s.Id).Select(s => s.VolumeAssessment).FirstOrDefault(),
                ProgressAssessment = jm.Scores.OrderByDescending(s => s.Id).Select(s => s.ProgressAssessment).FirstOrDefault(),
                QualityAssessment = jm.Scores.OrderByDescending(s => s.Id).Select(s => s.QualityAssessment).FirstOrDefault(),
                SummaryOfReviews = jm.Scores.OrderByDescending(s => s.Id).Select(s => s.SummaryOfReviews).FirstOrDefault()
            }).ToPagedList(pageNumber, pageSize);

            // Lưu giá trị cho ViewBag
            ViewBag.EmployeeId = id;
            ViewBag.Search = search;
            ViewBag.FilterStatus = filterStatus;
            ViewBag.Employee = employee; // Truyền thông tin nhân viên

            // Nếu là yêu cầu AJAX, trả về PartialView thay vì toàn bộ View
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_JobTablePartial", jobList);
            }

            return View(jobList);
        }
        public async Task<IActionResult> ExportToExcel(string searchText, int? status, int? categoryId, bool dueToday, string sortOrder, string month)
		{
			//var managerUsername = HttpContext.Session.GetString("AdminLogin");
			//if (string.IsNullOrEmpty(managerUsername))
			//{
			//	return RedirectToAction("Index", "Login");
			//}

			//var manager = await _context.Users
			//	.Where(u => u.UserName == managerUsername)
			//	.Select(u => u.Employee)
			//	.FirstOrDefaultAsync();

			//if (manager == null)
			//{
			//	return RedirectToAction("Index", "Login");
			//}

			//var managedDepartments = await _context.Departments
			//	.Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3))
			//	.Select(d => d.Id)
			//	.ToListAsync();

			var employeesInManagedDepartments = await _context.Employees
				.Where(e => e.DepartmentId.HasValue )
				.Select(e => e.Id)
				.ToListAsync();

			var jobs = _context.Jobmapemployees
				.Include(jm => jm.Employee)
				.Include(jm => jm.Job)
				.ThenInclude(j => j.Category)
				.Include(jm => jm.Job)
				.Include(jm => jm.Scores) // Lấy dữ liệu từ bảng SCORE
				.Where(jm => employeesInManagedDepartments.Contains((long)jm.EmployeeId));

			// Áp dụng bộ lọc tìm kiếm
			if (!string.IsNullOrEmpty(searchText))
			{
				jobs = jobs.Where(jm =>
					jm.Employee.Code.Contains(searchText) ||
					jm.Employee.FirstName.Contains(searchText) ||
					jm.Employee.LastName.Contains(searchText) ||
					jm.Job.Name.Contains(searchText));
			}

			if (status.HasValue)
			{
				jobs = jobs.Where(jm => jm.Scores.Any(s => s.Status == status.Value));
			}

			if (categoryId.HasValue)
			{
				jobs = jobs.Where(jm => jm.Job.CategoryId == categoryId.Value);
			}

			if (dueToday)
			{
				jobs = jobs.Where(jm => jm.Scores.Any(s => s.CompletionDate.HasValue && s.CompletionDate.Value == DateOnly.FromDateTime(DateTime.Today)));
			}

			if (!string.IsNullOrEmpty(month) && DateTime.TryParse(month + "-01", out DateTime selectedMonth))
			{
				jobs = jobs.Where(jm => jm.Scores.Any(s => s.CompletionDate.HasValue &&
															s.CompletionDate.Value.Year == selectedMonth.Year &&
															s.CompletionDate.Value.Month == selectedMonth.Month));
			}

			switch (sortOrder)
			{
				case "due_asc":
					jobs = jobs.OrderBy(jm => jm.Job.Deadline1);
					break;
				case "due_desc":
					jobs = jobs.OrderByDescending(jm => jm.Job.Deadline1);
					break;
				case "review_asc":
					jobs = jobs.OrderBy(jm => jm.Scores.FirstOrDefault().SummaryOfReviews);
					break;
				case "review_desc":
					jobs = jobs.OrderByDescending(jm => jm.Scores.FirstOrDefault().SummaryOfReviews);
					break;
			}

			var jobList = await jobs.ToListAsync();

			// Tạo file Excel bằng EPPlus
			using (var package = new ExcelPackage())
			{
				var worksheet = package.Workbook.Worksheets.Add("Jobs");
				worksheet.Cells[1, 1].Value = "STT";
				worksheet.Cells[1, 2].Value = "Nhân viên";
				worksheet.Cells[1, 3].Value = "Hạng mục";
				worksheet.Cells[1, 4].Value = "Công việc";
				worksheet.Cells[1, 5].Value = "Deadline";
				worksheet.Cells[1, 6].Value = "Ngày hoàn thành";
				worksheet.Cells[1, 7].Value = "Trạng thái";
				worksheet.Cells[1, 8].Value = "Đánh giá khối lượng";
				worksheet.Cells[1, 9].Value = "Đánh giá tiến độ";
				worksheet.Cells[1, 10].Value = "Đánh giá chất lượng";
				worksheet.Cells[1, 11].Value = "Đánh giá tổng hợp";

				// Điền dữ liệu
				for (int i = 0; i < jobList.Count; i++)
				{
					var jobMap = jobList[i];
					var job = jobMap.Job;
					var score = jobMap.Scores.FirstOrDefault();

					worksheet.Cells[i + 2, 1].Value = i + 1; // STT
					worksheet.Cells[i + 2, 2].Value = $"{jobMap.Employee.Code} {jobMap.Employee.FirstName} {jobMap.Employee.LastName}";
					worksheet.Cells[i + 2, 3].Value = job.Category.Name;
					worksheet.Cells[i + 2, 4].Value = job.Name;
					worksheet.Cells[i + 2, 5].Value = job.Deadline1?.ToString("dd/MM/yyyy");
					worksheet.Cells[i + 2, 6].Value = score?.CompletionDate?.ToString("dd/MM/yyyy");
					worksheet.Cells[i + 2, 7].Value = score?.Status switch
					{
						1 => "Hoàn thành",
						2 => "Chưa hoàn thành",
						3 => "Hoàn thành muộn",
						4 => "Đang xử lý",
						_ => "Chưa bắt đầu"
					};
					worksheet.Cells[i + 2, 8].Value = score?.VolumeAssessment;
					worksheet.Cells[i + 2, 9].Value = score?.ProgressAssessment;
					worksheet.Cells[i + 2, 10].Value = score?.QualityAssessment;
					worksheet.Cells[i + 2, 11].Value = score?.SummaryOfReviews;
				}

				// Định dạng cột
				worksheet.Cells[1, 1, 1, 11].Style.Font.Bold = true;
				worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

				// Xuất file Excel
				var stream = new MemoryStream(package.GetAsByteArray());
				return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Jobs.xlsx");
			}
		}

        // GET: ProjectManager/Jobs/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var score = await _context.Scores
                .Include(s => s.JobMapEmployee).Include(s => s.JobMapEmployee.Job).Include(s => s.JobMapEmployee.Employee).Include(s => s.JobMapEmployee.Job.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (score == null)
            {
                return NotFound();
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Details", score);
            }
            return View(score);
        }



  //      public async Task<IActionResult> Create()
		//{
		//	var managerUsername = HttpContext.Session.GetString("AdminLogin");
		//	if (string.IsNullOrEmpty(managerUsername))
		//	{
		//		return RedirectToAction("Index", "Login");
		//	}

		//	var manager = await _context.Users
		//		.Where(u => u.UserName == managerUsername)
		//		.Select(u => u.Employee)
		//		.FirstOrDefaultAsync();

		//	if (manager == null)
		//	{
		//		return RedirectToAction("Index", "Login");
		//	}
		//	ViewBag.CreateBy = managerUsername;
		//	// Lấy danh sách phòng ban mà người quản lý này quản lý (dựa trên PositionId == 3)
		//	var managedDepartments = await _context.Departments
		//		.Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3))
		//		.Select(d => d.Id)
		//		.ToListAsync();

		//	// Lấy danh sách nhân viên thuộc các phòng ban mà người quản lý đang quản lý
		//	var employeesInManagedDepartments = await _context.Employees
		//		.Where(e => e.DepartmentId.HasValue && managedDepartments.Contains(e.DepartmentId.Value))
		//		.Select(e => new
		//		{
		//			Id = e.Id,
		//			FullName = e.Code + " " + e.FirstName + " " + e.LastName
		//		})
		//		.ToListAsync();

		//	// Gán danh sách nhân viên vào ViewBag để hiển thị trong dropdown
		//	//ViewData["EmployeeId"] = new SelectList(employeesInManagedDepartments, "Id", "FullName");
		//	ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
		//	ViewBag.JobNames = await _context.Jobs
		//.Where(j => !string.IsNullOrEmpty(j.Name)) // Lọc bỏ các job không có tên
		//.Select(j => j.Name)
		//.Distinct()
		//.ToListAsync();
		//	if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
		//	{
		//		return PartialView("_Create");
		//	}
		//	return View();
		//}


		//// POST: AdminSystem/Jobs/Create
		//// To protect from overposting attacks, enable the specific properties you want to bind to.
		//// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public async Task<IActionResult> Create([Bind("Id,CategoryId,Name,Description,Deadline1,Deadline2,Deadline3,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Job job)
		//{
		//	var managerUsername = HttpContext.Session.GetString("AdminLogin");
		//	if (string.IsNullOrEmpty(managerUsername))
		//	{
		//		return RedirectToAction("Index", "Login");
		//	}

		//	if (ModelState.IsValid)
		//	{
		//		job.CreateBy = managerUsername; // Gán giá trị CreateBy

		//		_context.Add(job);
		//		await _context.SaveChangesAsync();
		//		return RedirectToAction(nameof(Index));
		//	}

		//	ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", job.CategoryId);
		//	//ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id", job.EmployeeId);
		//	return View(job);
		//}

		
		[HttpPost]
        [ValidateAntiForgeryToken]
        
        private async Task UpdateBaselineAssessment(long? employeeId)
        {
            if (employeeId == null)
                return;

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // Lấy danh sách công việc của nhân viên trong tháng hiện tại có đánh giá
            var jobMaps = await _context.Jobmapemployees
                .Include(jm => jm.Scores) // Bao gồm bảng Score để lấy dữ liệu đánh giá
                .Where(jm => jm.EmployeeId == employeeId
                          && jm.Scores.Any(s => s.CompletionDate.HasValue
                                                && s.CompletionDate.Value.Month == currentMonth
                                                && s.CompletionDate.Value.Year == currentYear
                                                && (s.Status == 1 || s.Status == 3))) // Hoàn thành hoặc Hoàn thành muộn
                .ToListAsync();

            if (!jobMaps.Any())
                return;

            // Tính tổng các đánh giá từ bảng Score
            double sumVolume = jobMaps.Sum(jm => jm.Scores.Sum(s => s.VolumeAssessment ?? 0));
            double sumProgress = jobMaps.Sum(jm => jm.Scores.Sum(s => s.ProgressAssessment ?? 0));
            double sumQuality = jobMaps.Sum(jm => jm.Scores.Sum(s => s.QualityAssessment ?? 0));
            double sumSummary = jobMaps.Sum(jm => jm.Scores.Sum(s => s.SummaryOfReviews ?? 0));

            // Xác định trạng thái Evaluate (giả sử tổng Summary >= 45 là đạt)
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

        private async Task UpdateAnalysis(long? employeeId)
        {
            if (employeeId == null)
                return;

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // Lấy danh sách công việc của nhân viên trong tháng hiện tại
            var jobMaps = await _context.Jobmapemployees
                .Include(jm => jm.Scores) // Lấy dữ liệu từ bảng Score
                .Where(jm => jm.EmployeeId == employeeId
                          && jm.Scores.Any(s => s.CompletionDate.HasValue
                                                && s.CompletionDate.Value.Month == currentMonth
                                                && s.CompletionDate.Value.Year == currentYear))
                .ToListAsync();

            // Tính số lượng công việc theo trạng thái từ bảng Score
            int ontime = jobMaps.Count(jm => jm.Scores.Any(s => s.Status == 1));    // Hoàn thành đúng hạn
            int late = jobMaps.Count(jm => jm.Scores.Any(s => s.Status == 2));      // Hoàn thành muộn
            int overdue = jobMaps.Count(jm => jm.Scores.Any(s => s.Status == 3));   // Quá hạn chưa hoàn thành
            int processing = jobMaps.Count(jm => jm.Scores.Any(s => s.Status == 4));// Đang xử lý
            int total = ontime + late + overdue + processing;

            // Tính trung bình đánh giá của nhân viên
            var averageReview = jobMaps.Any()
                ? jobMaps.Average(jm => jm.Scores.Average(s => s.SummaryOfReviews ?? 0))
                : 0;

            // Tìm bản ghi Analysis của nhân viên trong tháng hiện tại
            var analysis = await _context.Analyses
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
                                          && a.Time.HasValue
                                          && a.Time.Value.Month == currentMonth
                                          && a.Time.Value.Year == currentYear);

            if (analysis == null)
            {
                // Nếu chưa có bản ghi trong tháng, tạo mới
                analysis = new Analysis
                {
                    EmployeeId = employeeId,
                    Total = total,
                    Ontime = ontime,
                    Late = late,
                    Overdue = overdue,
                    Processing = processing,
                    //AverageReview = averageReview,
                    Time = new DateTime(currentYear, currentMonth, 1),
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now
                };
                _context.Analyses.Add(analysis);
            }
            else
            {
                // Nếu đã có bản ghi trong tháng, cập nhật dữ liệu
                analysis.Total = total;
                analysis.Ontime = ontime;
                analysis.Late = late;
                analysis.Overdue = overdue;
                analysis.Processing = processing;
                //analysis.AverageReview = averageReview;
                analysis.UpdateDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }


      

        private bool JobExists(long id)
        {
            return _context.Jobs.Any(e => e.Id == id);
        }
        
        private bool ScoreExists(long id)
        {
            return _context.Scores.Any(e => e.Id == id);
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


}

