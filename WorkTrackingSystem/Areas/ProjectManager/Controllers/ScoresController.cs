using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using WorkTrackingSystem.Models;
using X.PagedList.Extensions;
using static System.Formats.Asn1.AsnWriter;

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

        public async Task<IActionResult> Index(
    string searchText = "",
    string month = "",
    string day = "",
    string year = "",
    string quarter = "",
    string quarterYear = "",
    string fromDate = "",
    string toDate = "",
    string status = "",
    string categoryId = "",
    string sortOrder = "",
    bool showCompletedZeroReview = false,
    bool dueToday = false,
    long? jobId = null,
    int page = 1)
        {
            var limit = 10;
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

            // Điều chỉnh scoresQuery để bao gồm JobRepeat
            var scoresQuery = _context.Scores
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.Employee)
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.Job)
                        .ThenInclude(j => j.Category)
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.JobRepeat) // Thêm Include cho JobRepeat
                .Where(s => s.JobMapEmployee != null && s.JobMapEmployee.Job != null)
                .Where(s =>
                    (s.JobMapEmployee.EmployeeId.HasValue && employeesInManagedDepartments.Select(e => long.Parse(e.Value)).Contains(s.JobMapEmployee.EmployeeId.Value)) ||
                    (s.JobMapEmployee.EmployeeId == null && !assignedJobIds.Contains(s.JobMapEmployee.JobId))
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

            // Lọc theo quý
            if (!string.IsNullOrEmpty(quarter) && int.TryParse(quarter, out int selectedQuarter))
            {
                var startMonth = (selectedQuarter - 1) * 3 + 1;
                var endMonth = startMonth + 2;
                scoresQuery = scoresQuery.Where(s =>
                    s.CreateDate.HasValue &&
                    s.CreateDate.Value.Month >= startMonth &&
                    s.CreateDate.Value.Month <= endMonth);

                if (!string.IsNullOrEmpty(quarterYear) && int.TryParse(quarterYear, out int selectedQuarterYear))
                {
                    scoresQuery = scoresQuery.Where(s =>
                        s.CreateDate.HasValue && s.CreateDate.Value.Year == selectedQuarterYear);
                }
            }
            else if (!string.IsNullOrEmpty(quarterYear) && int.TryParse(quarterYear, out int selectedQuarterYear))
            {
                scoresQuery = scoresQuery.Where(s =>
                    s.CreateDate.HasValue && s.CreateDate.Value.Year == selectedQuarterYear);
            }

            // Lọc theo năm
            if (!string.IsNullOrEmpty(year) && int.TryParse(year, out int selectedYear) && string.IsNullOrEmpty(quarterYear))
            {
                scoresQuery = scoresQuery.Where(s =>
                    s.CreateDate.HasValue && s.CreateDate.Value.Year == selectedYear);
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

            // Sắp xếp: sử dụng JobRepeat.Deadline1 nếu có, nếu không thì Job.Deadline1
            switch (sortOrder)
            {
                case "due_asc":
                    scoresQuery = scoresQuery.OrderBy(s =>
                        s.JobMapEmployee.JobRepeat != null && s.JobMapEmployee.JobRepeat.Deadline1.HasValue
                        ? s.JobMapEmployee.JobRepeat.Deadline1.Value
                        : s.JobMapEmployee.Job.Deadline1);
                    break;
                case "due_desc":
                    scoresQuery = scoresQuery.OrderByDescending(s =>
                        s.JobMapEmployee.JobRepeat != null && s.JobMapEmployee.JobRepeat.Deadline1.HasValue
                        ? s.JobMapEmployee.JobRepeat.Deadline1.Value
                        : s.JobMapEmployee.Job.Deadline1);
                    break;
                case "review_asc":
                    scoresQuery = scoresQuery.OrderBy(s => s.SummaryOfReviews ?? 0);
                    break;
                case "review_desc":
                    scoresQuery = scoresQuery.OrderByDescending(s => s.SummaryOfReviews ?? 0);
                    break;
                default:
                    scoresQuery = scoresQuery
                        .OrderBy(s => s.JobMapEmployee.EmployeeId.HasValue)
                        .ThenByDescending(s => s.CreateDate);
                    break;
            }

            var scores = scoresQuery.ToPagedList(page, limit);

            // Gán ViewBag để giữ trạng thái lọc/sắp xếp
            ViewBag.SearchText = searchText;
            ViewBag.Month = month;
            ViewBag.Day = day;
            ViewBag.Year = year;
            ViewBag.Quarter = quarter;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.Status = status?.ToString();
            ViewBag.CategoryId = categoryId?.ToString();
            ViewBag.SortOrder = sortOrder;
            ViewBag.ShowCompletedZeroReview = showCompletedZeroReview;
            ViewBag.DueToday = dueToday;

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
                // Bước 1: Lấy danh sách Score với JobMapEmployee và Job
                var scores = await _context.Scores
                    .Include(s => s.JobMapEmployee)
                        .ThenInclude(jme => jme.Job)
                            .ThenInclude(j => j.Category)
                    .Where(s => s.JobMapEmployee.EmployeeId == employeeId)
                    .ToListAsync();

                // Bước 2: Lấy JobRepeatId và Deadline1 từ JobRepeat nếu cần
                var jobMapEmployeeIds = scores.Select(s => s.JobMapEmployee.Id).ToList();
                var jobMapEmployeesWithRepeats = await _context.Jobmapemployees
                    .Include(jme => jme.JobRepeat)
                    .Where(jme => jobMapEmployeeIds.Contains(jme.Id))
                    .ToDictionaryAsync(jme => jme.Id, jme => jme.JobRepeat);

                // Bước 3: Tạo kết quả
                var assignedJobs = scores.Select(s =>
                {
                    var jobMapEmployee = s.JobMapEmployee;
                    var jobRepeat = jobMapEmployeesWithRepeats.ContainsKey(jobMapEmployee.Id) ? jobMapEmployeesWithRepeats[jobMapEmployee.Id] : null;

                    return new
                    {
                        Id = jobMapEmployee.JobRepeatId.HasValue ? jobMapEmployee.JobRepeatId.Value : jobMapEmployee.JobId,
                        Name = jobMapEmployee.Job.Name,
                        CategoryName = jobMapEmployee.Job.Category != null ? jobMapEmployee.Job.Category.Name : "N/A",
                        Time = s.Time,
                        Deadline1 = jobRepeat != null && jobRepeat.Deadline1.HasValue
                            ? jobRepeat.Deadline1
                            : jobMapEmployee.Job.Deadline1,
                        Status = s.Status,
                        CompletionDate = s.CompletionDate
                    };
                }).ToList();

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
        public async Task<IActionResult> AssignEmployee(long jobId, long employeeId, string time, string deadline, bool recurring, byte? recurrenceType, byte? recurrenceInterval, string recurrenceEndDate)
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

            // Update job recurrence settings
            job.Recurring = recurring;
            if (recurring)
            {
                if (!recurrenceType.HasValue || !recurrenceInterval.HasValue || string.IsNullOrEmpty(recurrenceEndDate))
                {
                    return Json(new { success = false, message = "Recurring job requires type, interval, and end date" });
                }
                job.RecurrenceType = recurrenceType;
                job.RecurrenceInterval = recurrenceInterval;
                if (DateOnly.TryParse(recurrenceEndDate, out DateOnly parsedRecurrenceEndDate))
                {
                    job.RecurrenceEndDate = parsedRecurrenceEndDate;
                }
                else
                {
                    return Json(new { success = false, message = "Invalid recurrence end date format" });
                }
            }
            else
            {
                job.RecurrenceType = null;
                job.RecurrenceInterval = null;
                job.RecurrenceEndDate = null;
            }
            _context.Jobs.Update(job);

            // Parse time and deadline
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

            // Create JobMapEmployee for the main job
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

            // Create Score for the main job
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

            // Handle Recurring Jobs
            if (job.Recurring == true && job.RecurrenceType.HasValue && job.RecurrenceInterval.HasValue && job.RecurrenceEndDate.HasValue)
            {
                var currentDate = job.Deadline1 ?? DateOnly.FromDateTime(DateTime.Now);
                var recurrenceEnd = job.RecurrenceEndDate.Value;

                // Tăng currentDate lên một chu kỳ lặp ngay từ đầu để bỏ qua ngày ban đầu
                switch (job.RecurrenceType)
                {
                    case 1: // Daily
                        currentDate = currentDate.AddDays(job.RecurrenceInterval.Value);
                        break;
                    case 2: // Weekly
                        currentDate = currentDate.AddDays(7 * job.RecurrenceInterval.Value);
                        break;
                    case 3: // Monthly
                        currentDate = currentDate.AddMonths(job.RecurrenceInterval.Value);
                        break;
                    case 4: // Yearly
                        currentDate = currentDate.AddYears(job.RecurrenceInterval.Value);
                        break;
                    default:
                        return Json(new { success = false, message = "Invalid recurrence type" });
                }

                // Tạo các Jobrepeat cho các ngày tiếp theo
                while (currentDate <= recurrenceEnd)
                {
                    var jobRepeat = new Jobrepeat
                    {
                        JobId = job.Id,
                        Name = job.Name,
                        Description = job.Description,
                        Deadline1 = currentDate,
                        Deadline2 = job.Deadline2.HasValue ? currentDate.AddDays((job.Deadline2.Value.ToDateTime(TimeOnly.MinValue) - job.Deadline1.Value.ToDateTime(TimeOnly.MinValue)).Days) : null,
                        Deadline3 = job.Deadline3.HasValue ? currentDate.AddDays((job.Deadline3.Value.ToDateTime(TimeOnly.MinValue) - job.Deadline1.Value.ToDateTime(TimeOnly.MinValue)).Days) : null,
                        IsDelete = false,
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now,
                        CreateBy = job.CreateBy,
                        UpdateBy = job.UpdateBy
                    };

                    _context.Jobrepeats.Add(jobRepeat);
                    await _context.SaveChangesAsync();

                    var jobRepeatMapEmployee = new Jobmapemployee
                    {
                        JobId = job.Id,
                        JobRepeatId = jobRepeat.Id,
                        EmployeeId = employeeId,
                        IsDelete = false,
                        IsActive = true,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now,
                        CreateBy = job.CreateBy,
                        UpdateBy = job.UpdateBy
                    };
                    _context.Jobmapemployees.Add(jobRepeatMapEmployee);
                    await _context.SaveChangesAsync();

                    var jobRepeatScore = new Score
                    {
                        JobMapEmployeeId = jobRepeatMapEmployee.Id,
                        Status = 0,
                        IsDelete = false,
                        IsActive = true,
                        CreateDate = jobRepeat.Deadline1?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
                        Time = parsedTime ?? DateTime.Now,
                        CreateBy = job.CreateBy,
                        UpdateBy = job.UpdateBy
                    };
                    _context.Scores.Add(jobRepeatScore);
                    await _context.SaveChangesAsync();

                    // Tăng currentDate cho chu kỳ lặp tiếp theo
                    switch (job.RecurrenceType)
                    {
                        case 1: // Daily
                            currentDate = currentDate.AddDays(job.RecurrenceInterval.Value);
                            break;
                        case 2: // Weekly
                            currentDate = currentDate.AddDays(7 * job.RecurrenceInterval.Value);
                            break;
                        case 3: // Monthly
                            currentDate = currentDate.AddMonths(job.RecurrenceInterval.Value);
                            break;
                        case 4: // Yearly
                            currentDate = currentDate.AddYears(job.RecurrenceInterval.Value);
                            break;
                    }
                }
            }

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
        public async Task<IActionResult> Createjob([Bind("Id,CategoryId,Name,Description,Deadline1,Deadline2,Deadline3,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy,Recurring,RecurrenceType,RecurrenceInterval,RecurrenceEndDate")] Job job)
        {
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");
            if (ModelState.IsValid)
            {
                // Set audit fields for the Job
                job.CreateBy = managerUsername;
                job.UpdateBy = managerUsername;
                job.CreateDate = DateTime.Now;
                job.UpdateDate = DateTime.Now;
                job.IsDelete = false;
                job.IsActive = true;

                // Add the Job to the context
                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();

                // Create JobMapEmployee for the Job
                var jobMapEmployee = new Jobmapemployee
                {
                    JobId = job.Id,
                    EmployeeId = null, // To be updated later if needed
                    IsDelete = false,
                    IsActive = true,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    CreateBy = job.CreateBy,
                    UpdateBy = job.UpdateBy
                };
                _context.Jobmapemployees.Add(jobMapEmployee);
                await _context.SaveChangesAsync();

                // Create Score for the JobMapEmployee
                var score = new Score
                {
                    JobMapEmployeeId = jobMapEmployee.Id,
                    Status = 0, // Default status (not started)
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

            // If ModelState is invalid, return the view with validation errors
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", job.CategoryId);
            return View(job);
        }

        public async Task<IActionResult> ExportToExcel(
    string searchText = "",
    string month = "",
    string day = "",
    string year = "",
    string quarter = "",
    string quarterYear = "", // Thêm tham số quarterYear
    string fromDate = "",
    string toDate = "",
    int? status = null,
    int? categoryId = null,
    bool dueToday = false,
    string sortOrder = "",
    bool showCompletedZeroReview = false)
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
                .Select(e => e.Id)
                .ToListAsync();

            var scoresQuery = _context.Scores
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.Employee)
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.Job)
                        .ThenInclude(j => j.Category)
                .Where(s => s.JobMapEmployee.EmployeeId.HasValue && employeesInManagedDepartments.Contains(s.JobMapEmployee.EmployeeId.Value));

            // Áp dụng các bộ lọc

            // Tìm kiếm theo mã / tên nhân viên / công việc
            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.ToLower();
                scoresQuery = scoresQuery.Where(s =>
                    (s.JobMapEmployee.Employee != null && s.JobMapEmployee.Employee.Code.ToLower().Contains(searchText)) ||
                    (s.JobMapEmployee.Employee != null && s.JobMapEmployee.Employee.FirstName.ToLower().Contains(searchText)) ||
                    (s.JobMapEmployee.Employee != null && s.JobMapEmployee.Employee.LastName.ToLower().Contains(searchText)) ||
                    s.JobMapEmployee.Job.Name.ToLower().Contains(searchText));
            }

            // Lọc theo năm
            if (!string.IsNullOrEmpty(year) && int.TryParse(year, out int selectedYear))
            {
                scoresQuery = scoresQuery.Where(s => s.CreateDate.HasValue && s.CreateDate.Value.Year == selectedYear);
            }

            // Lọc theo quý
            if (!string.IsNullOrEmpty(quarter) && int.TryParse(quarter, out int selectedQuarter))
            {
                var startMonth = (selectedQuarter - 1) * 3 + 1;
                var endMonth = startMonth + 2;
                scoresQuery = scoresQuery.Where(s => s.CreateDate.HasValue && s.CreateDate.Value.Month >= startMonth && s.CreateDate.Value.Month <= endMonth);

                // Kiểm tra năm của quý
                if (!string.IsNullOrEmpty(quarterYear) && int.TryParse(quarterYear, out int selectedQuarterYear))
                {
                    scoresQuery = scoresQuery.Where(s => s.CreateDate.HasValue && s.CreateDate.Value.Year == selectedQuarterYear);
                }
            }

            // Lọc theo tháng
            if (!string.IsNullOrEmpty(month) && DateTime.TryParse(month + "-01", out DateTime selectedMonth))
            {
                scoresQuery = scoresQuery.Where(s => s.CreateDate.HasValue &&
                                                    s.CreateDate.Value.Year == selectedMonth.Year &&
                                                    s.CreateDate.Value.Month == selectedMonth.Month);
            }

            // Lọc theo ngày
            if (!string.IsNullOrEmpty(day) && DateTime.TryParse(day, out DateTime selectedDay))
            {
                scoresQuery = scoresQuery.Where(s => s.CreateDate.HasValue && s.CreateDate.Value.Date == selectedDay.Date);
            }

            // Lọc theo khoảng thời gian
            if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out DateTime startDate))
            {
                scoresQuery = scoresQuery.Where(s => s.CreateDate.HasValue && s.CreateDate.Value.Date >= startDate.Date);
            }

            if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out DateTime endDate))
            {
                scoresQuery = scoresQuery.Where(s => s.CreateDate.HasValue && s.CreateDate.Value.Date <= endDate.Date);
            }

            // Lọc theo trạng thái
            if (status.HasValue)
            {
                scoresQuery = scoresQuery.Where(s => s.Status == status.Value);
            }

            // Lọc theo danh mục
            if (categoryId.HasValue)
            {
                scoresQuery = scoresQuery.Where(s => s.JobMapEmployee.Job.CategoryId == categoryId.Value);
            }

            // Hiển thị công việc hoàn thành nhưng chưa đánh giá
            if (showCompletedZeroReview)
            {
                scoresQuery = scoresQuery.Where(s => s.SummaryOfReviews == 0);
            }

            // Lọc công việc theo ngày (dueToday)
            if (dueToday)
            {
                scoresQuery = scoresQuery.Where(s => s.Time == s.CreateDate);
            }

            // Sắp xếp
            switch (sortOrder)
            {
                case "due_asc":
                    scoresQuery = scoresQuery.OrderBy(s => s.CreateDate);
                    break;
                case "due_desc":
                    scoresQuery = scoresQuery.OrderByDescending(s => s.CreateDate);
                    break;
                case "review_asc":
                    scoresQuery = scoresQuery.OrderBy(s => s.SummaryOfReviews);
                    break;
                case "review_desc":
                    scoresQuery = scoresQuery.OrderByDescending(s => s.SummaryOfReviews);
                    break;
                default:
                    scoresQuery = scoresQuery.OrderBy(s => s.CreateDate);
                    break;
            }

            var scoreList = await scoresQuery.ToListAsync();

            // Tạo file Excel bằng EPPlus
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Scores");
                worksheet.Cells[1, 1].Value = "STT";
                worksheet.Cells[1, 2].Value = "Người triển khai";
                worksheet.Cells[1, 3].Value = "Hạng mục";
                worksheet.Cells[1, 4].Value = "Công việc";
                worksheet.Cells[1, 5].Value = "Ngày tạo";
                worksheet.Cells[1, 6].Value = "Ngày hoàn thành";
                worksheet.Cells[1, 7].Value = "Trạng thái";
                worksheet.Cells[1, 8].Value = "Đánh giá khối lượng";
                worksheet.Cells[1, 9].Value = "Đánh giá tiến độ";
                worksheet.Cells[1, 10].Value = "Đánh giá chất lượng";
                worksheet.Cells[1, 11].Value = "Đánh giá tổng hợp";

                // Điền dữ liệu
                for (int i = 0; i < scoreList.Count; i++)
                {
                    var score = scoreList[i];
                    worksheet.Cells[i + 2, 1].Value = i + 1; // STT
                    worksheet.Cells[i + 2, 2].Value = score.JobMapEmployee.Employee != null
                        ? $"{score.JobMapEmployee.Employee.FirstName} {score.JobMapEmployee.Employee.LastName}"
                        : "N/A";
                    worksheet.Cells[i + 2, 3].Value = score.JobMapEmployee.Job.Category?.Name ?? "N/A";
                    worksheet.Cells[i + 2, 4].Value = score.JobMapEmployee.Job?.Name ?? "N/A";
                    worksheet.Cells[i + 2, 5].Value = score.CreateDate?.ToString("dd/MM/yyyy");
                    worksheet.Cells[i + 2, 6].Value = score.CompletionDate?.ToString("dd/MM/yyyy");
                    worksheet.Cells[i + 2, 7].Value = score.Status switch
                    {
                        1 => "Hoàn thành",
                        2 => "Chưa hoàn thành",
                        3 => "Hoàn thành muộn",
                        4 => "Đang xử lý",
                        _ => "Chưa bắt đầu"
                    };
                    worksheet.Cells[i + 2, 8].Value = score.VolumeAssessment;
                    worksheet.Cells[i + 2, 9].Value = score.ProgressAssessment;
                    worksheet.Cells[i + 2, 10].Value = score.QualityAssessment;
                    worksheet.Cells[i + 2, 11].Value = score.SummaryOfReviews.HasValue
                        ? score.SummaryOfReviews.Value.ToString("0.00")
                        : "0.00";
                }

                // Định dạng cột
                worksheet.Cells[1, 1, 1, 11].Style.Font.Bold = true;
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Xuất file Excel
                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Scores.xlsx");
            }
        }
        // GET: ProjectManager/Scores/Details/5
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
                return PartialView("_Details",score);
            }
            return View(score);
        }

        // GET: ProjectManager/Scores/Create
        // GET: Create
        public IActionResult Create(int? currentJobMapEmployeeId)
        {
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");
            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
            }

            var manager = _context.Users
                .Where(u => u.UserName == managerUsername)
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
            ViewBag.Job = currentJobMapEmployee.Job;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Create");
            }

            return View();
        }
        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int employeeId, int currentJobMapEmployeeId, DateOnly deadline)
        {
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");
            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
            }

            var currentJobMapEmployee = await _context.Jobmapemployees
                .Include(j => j.Job)
                .Include(j => j.JobRepeat)
                .FirstOrDefaultAsync(j => j.Id == currentJobMapEmployeeId);

            if (currentJobMapEmployee == null)
            {
                return BadRequest("Không tìm thấy công việc hiện tại");
            }

            // Disable old scores
            var oldScores = await _context.Scores
                .Where(s => s.JobMapEmployeeId == currentJobMapEmployeeId)
                .ToListAsync();

            foreach (var oldScore in oldScores)
            {
                oldScore.IsActive = false;
            }

            _context.Scores.UpdateRange(oldScores);

            // Create new JobMapEmployee
            var newJobMapEmployee = new Jobmapemployee
            {
                JobId = currentJobMapEmployee.JobId,
                EmployeeId = employeeId,
                JobRepeatId = currentJobMapEmployee.JobRepeatId
            };

            _context.Jobmapemployees.Add(newJobMapEmployee);
            await _context.SaveChangesAsync();

            // Update deadline
            if (currentJobMapEmployee.JobRepeat != null)
            {
                var jobRepeat = await _context.Jobrepeats
                    .FirstOrDefaultAsync(jr => jr.Id == currentJobMapEmployee.JobRepeatId);
                if (jobRepeat != null)
                {
                    if (jobRepeat.Deadline2 == null)
                    {
                        jobRepeat.Deadline2 = deadline;
                    }
                    else if (jobRepeat.Deadline3 == null)
                    {
                        jobRepeat.Deadline3 = deadline;
                    }
                    _context.Jobrepeats.Update(jobRepeat);
                }
            }
            else
            {
                var job = await _context.Jobs
                    .FirstOrDefaultAsync(j => j.Id == currentJobMapEmployee.JobId);
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
            }

            // Create new Score
            var jobFromMap = currentJobMapEmployee.Job;
            var score = new Score
            {
                CreateBy = managerUsername,
                JobMapEmployeeId = newJobMapEmployee.Id,
                Status = 0,
                Time = jobFromMap.Deadline1.HasValue ? DateTime.Parse(jobFromMap.Deadline1.Value.ToString("yyyy-MM-dd")) : DateTime.Now,
                CreateDate = deadline.ToDateTime(TimeOnly.MinValue),
                IsActive = true,
                IsDelete = false
            };

            // Call update methods
            await UpdateBaselineAssessment(newJobMapEmployee.EmployeeId);
            await UpdateAnalysis(newJobMapEmployee.EmployeeId);

            _context.Scores.Add(score);
            await _context.SaveChangesAsync();

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
                        .ThenInclude(j => j.Category)
                        .Include(s => s.JobMapEmployee)
                        .ThenInclude(jme => jme.JobRepeat)
                        .FirstOrDefaultAsync(s => s.Id == id);

                    if (existingScore == null)
                    {
                        return NotFound();
                    }

                    // Update Score properties
                    existingScore.CreateDate = score.CreateDate;
                    existingScore.CompletionDate = score.CompletionDate;
                    existingScore.Status = score.Status;
                    existingScore.VolumeAssessment = score.VolumeAssessment;
                    existingScore.ProgressAssessment = score.ProgressAssessment;
                    existingScore.QualityAssessment = score.QualityAssessment;
                    existingScore.SummaryOfReviews = score.SummaryOfReviews;
                    existingScore.Progress = score.Progress;
                    existingScore.Time = score.Time;

                    if (existingScore.JobMapEmployee?.EmployeeId != null)
                    {
                        // Update Job properties if it exists
                        var job = existingScore.JobMapEmployee.Job;
                        if (job != null)
                        {
                            job.Name = score.JobMapEmployee?.Job?.Name;
                            job.CategoryId = score.JobMapEmployee?.Job?.CategoryId;
                        }

                        if (score.CreateDate.HasValue)
                        {
                            DateOnly createDateAsDateOnly = DateOnly.FromDateTime(score.CreateDate.Value);

                            // Prioritize JobRepeat if it exists
                            var jobRepeat = existingScore.JobMapEmployee.JobRepeat;
                            if (jobRepeat != null)
                            {
                                // Update JobRepeat deadlines
                                if (jobRepeat.Deadline1.HasValue && jobRepeat.Deadline2.HasValue && jobRepeat.Deadline3.HasValue)
                                {
                                    jobRepeat.Deadline3 = createDateAsDateOnly;
                                }
                                else if (jobRepeat.Deadline1.HasValue && jobRepeat.Deadline2.HasValue)
                                {
                                    jobRepeat.Deadline2 = createDateAsDateOnly;
                                }
                                else if (!jobRepeat.Deadline1.HasValue || (jobRepeat.Deadline1.HasValue && !jobRepeat.Deadline2.HasValue))
                                {
                                    jobRepeat.Deadline1 = createDateAsDateOnly;
                                }
                            }
                            // Fallback to updating Job deadlines if JobRepeat does not exist
                            else if (job != null)
                            {
                                // Update Job deadlines
                                if (job.Deadline1.HasValue && job.Deadline2.HasValue && job.Deadline3.HasValue)
                                {
                                    job.Deadline3 = createDateAsDateOnly;
                                }
                                else if (job.Deadline1.HasValue && job.Deadline2.HasValue)
                                {
                                    job.Deadline2 = createDateAsDateOnly;
                                }
                                else if (!job.Deadline1.HasValue || (job.Deadline1.HasValue && !job.Deadline2.HasValue))
                                {
                                    job.Deadline1 = createDateAsDateOnly;
                                }
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

            // Repopulate ViewData if validation fails
            ViewData["JobMapEmployeeId"] = new SelectList(_context.Jobmapemployees, "Id", "Id", score.JobMapEmployeeId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", score.JobMapEmployee?.Job?.CategoryId);
            return View(score);
        }

        // GET: ProjectManager/Scores/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAssessment(long id, string field, float value)
        {
            var score = await _context.Scores
                .Include(s => s.JobMapEmployee)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (score == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bản ghi." });
            }

            try
            {
                // Update the specified field
                switch (field)
                {
                    case "VolumeAssessment":
                        score.VolumeAssessment = value;
                        break;
                    case "ProgressAssessment":
                        score.ProgressAssessment = value;
                        break;
                    case "QualityAssessment":
                        score.QualityAssessment = value;
                        break;
                    default:
                        return Json(new { success = false, message = "Trường không hợp lệ." });
                }

                // Update SummaryOfReviews automatically
                score.SummaryOfReviews = Math.Round((score.VolumeAssessment ?? 0) * 0.6f +
                                         (score.ProgressAssessment ?? 0) * 0.15f +
                                         (score.QualityAssessment ?? 0) * 0.25f,2);

                score.UpdateDate = DateTime.Now;
                score.UpdateBy = HttpContext.Session.GetString("ProjectManagerLogin");

                // Update related assessments if EmployeeId exists
                if (score.JobMapEmployee != null)
                {
                    await UpdateBaselineAssessment(score.JobMapEmployee.EmployeeId);
                    await UpdateAnalysis(score.JobMapEmployee.EmployeeId);
                }

                _context.Update(score);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        private async Task UpdateBaselineAssessment(long? employeeId)
        {
            if (employeeId == null)
                return;

            // Lấy tất cả bản ghi Score của nhân viên
            var jobs = await _context.Scores
                .Where(j => j.JobMapEmployee.EmployeeId == employeeId && j.CreateDate.HasValue)
                .ToListAsync();

            if (!jobs.Any())
                return;

            // Nhóm các bản ghi Score theo tháng và năm của CreateDate
            var groupedJobs = jobs
                .GroupBy(j => new { j.CreateDate.Value.Year, j.CreateDate.Value.Month });

            foreach (var group in groupedJobs)
            {
                var year = group.Key.Year;
                var month = group.Key.Month;

                // Tính tổng các đánh giá cho tháng/năm này
                double sumVolume = group.Sum(j => j.VolumeAssessment ?? 0);
                double sumProgress = group.Sum(j => j.ProgressAssessment ?? 0);
                double sumQuality = group.Sum(j => j.QualityAssessment ?? 0);
                double sumSummary = group.Sum(j => j.SummaryOfReviews ?? 0);
                bool evaluate = sumSummary >= 45;

                // Tìm bản ghi BaselineAssessment cho tháng/năm của CreateDate
                var baseline = await _context.Baselineassessments
                    .FirstOrDefaultAsync(b => b.EmployeeId == employeeId
                                           && b.Time.HasValue
                                           && b.Time.Value.Month == month
                                           && b.Time.Value.Year == year);

                if (baseline == null)
                {
                    // Nếu chưa có bản ghi, tạo mới
                    baseline = new Baselineassessment
                    {
                        EmployeeId = employeeId,
                        VolumeAssessment = sumVolume,
                        ProgressAssessment = sumProgress,
                        QualityAssessment = sumQuality,
                        SummaryOfReviews = sumSummary,
                        Time = new DateTime(year, month, 1), // Dựa trên CreateDate
                        Evaluate = evaluate,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now,
                        IsActive = true,
                        IsDelete = false
                    };
                    _context.Baselineassessments.Add(baseline);
                }
                else
                {
                    // Nếu đã có bản ghi, cập nhật dữ liệu
                    baseline.VolumeAssessment = sumVolume;
                    baseline.ProgressAssessment = sumProgress;
                    baseline.QualityAssessment = sumQuality;
                    baseline.SummaryOfReviews = sumSummary;
                    baseline.Evaluate = evaluate;
                    baseline.UpdateDate = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
        }
        private async Task UpdateAnalysis(long? employeeId)
        {
            if (employeeId == null)
                return;

            // Lấy tất cả bản ghi Score của nhân viên
            var jobs = await _context.Scores
                .Where(j => j.JobMapEmployee.EmployeeId == employeeId && j.CreateDate.HasValue)
                .ToListAsync();

            if (!jobs.Any())
                return;

            // Nhóm các bản ghi Score theo tháng và năm của CreateDate
            var groupedJobs = jobs
                .GroupBy(j => new { j.CreateDate.Value.Year, j.CreateDate.Value.Month });

            foreach (var group in groupedJobs)
            {
                var year = group.Key.Year;
                var month = group.Key.Month;

                // Tính số lượng công việc theo trạng thái cho tháng/năm này
                int ontime = group.Count(j => j.Status == 1);
                int late = group.Count(j => j.Status == 2);
                int overdue = group.Count(j => j.Status == 3);
                int processing = group.Count(j => j.Status == 4 || j.Status == 0|| j.Status == 5);
                int total = ontime + late + overdue + processing;

                // Tìm bản ghi Analysis cho tháng/năm của CreateDate
                var analysis = await _context.Analyses
                    .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
                                           && a.Time.HasValue
                                           && a.Time.Value.Month == month
                                           && a.Time.Value.Year == year);

                if (analysis == null)
                {
                    // Nếu chưa có bản ghi, tạo mới
                    analysis = new Analysis
                    {
                        EmployeeId = employeeId,
                        Total = total,
                        Ontime = ontime,
                        Late = late,
                        Overdue = overdue,
                        Processing = processing,
                        Time = new DateTime(year, month, 1), // Dựa trên CreateDate
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    };
                    _context.Analyses.Add(analysis);
                }
                else
                {
                    // Nếu đã có bản ghi, cập nhật dữ liệu
                    analysis.Total = total;
                    analysis.Ontime = ontime;
                    analysis.Late = late;
                    analysis.Overdue = overdue;
                    analysis.Processing = processing;
                    analysis.UpdateDate = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
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