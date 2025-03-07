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
using OfficeOpenXml.Drawing.Chart;

namespace WorkTrackingSystem.Areas.ProjectManager.Controllers
{
    [Area("ProjectManager")]
    public class JobsController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public JobsController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: ProjectManager/Jobs
        public async Task<IActionResult> Index(string searchText, int? status, int? categoryId, bool dueToday, string sortOrder, string month)
        {
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");
            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
            }

            //lấy thông tin nhân viên đăng nhập
            var manager = await _context.Users
                .Where(u => u.UserName == managerUsername)
                .Select(u => u.Employee)
                .FirstOrDefaultAsync();

            if (manager == null)
            {
                return RedirectToAction("Index", "Login");
            }

            //lấy id phòng ban mà nhân viên quản lý
            var managedDepartments = await _context.Departments
                .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 2))
                .Select(d => d.Id)
                .ToListAsync();

            //lấy id phòng ban mà nhân viên quản lý
            var employeesInManagedDepartments = await _context.Employees
                .Where(e => e.DepartmentId.HasValue && managedDepartments.Contains(e.DepartmentId.Value))
                .Select(e => e.Id)
                .ToListAsync();

            var jobs = _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Employee)
                .Where(j => j.EmployeeId.HasValue && employeesInManagedDepartments.Contains(j.EmployeeId.Value));

            // Tìm kiếm theo mã nhân viên, họ tên nhân viên, tên công việc
            if (!string.IsNullOrEmpty(searchText))
            {
                jobs = jobs.Where(j =>
                    j.Employee.Code.Contains(searchText) ||
                    j.Employee.FirstName.Contains(searchText) ||
                    j.Employee.LastName.Contains(searchText) ||
                    j.Name.Contains(searchText));
            }

            // Lọc theo trạng thái công việc
            if (status.HasValue)
            {
                jobs = jobs.Where(j => j.Status == status.Value);
            }

            // Lọc theo danh mục công việc
            if (categoryId.HasValue)
            {
                jobs = jobs.Where(j => j.CategoryId == categoryId.Value);
            }

            // Lọc theo công việc có hạn hoàn thành hôm nay
            if (dueToday)
            {
                jobs = jobs.Where(j => j.Time.HasValue && j.Time.Value.Date == DateTime.Today);
            }

            // Lọc theo tháng được chọn
            if (!string.IsNullOrEmpty(month) && DateTime.TryParse(month + "-01", out DateTime selectedMonth))
            {
                jobs = jobs.Where(j => j.Time.HasValue &&
                                      j.Time.Value.Year == selectedMonth.Year &&
                                      j.Time.Value.Month == selectedMonth.Month);
            }
            // Sắp xếp dữ liệu
            switch (sortOrder)
            {
                case "due_asc":
                    jobs = jobs.OrderBy(j => j.Time);
                    break;
                case "due_desc":
                    jobs = jobs.OrderByDescending(j => j.Time);
                    break;
                case "review_asc":
                    jobs = jobs.OrderBy(j => j.SummaryOfReviews);
                    break;
                case "review_desc":
                    jobs = jobs.OrderByDescending(j => j.SummaryOfReviews);
                    break;
                default:
                    jobs = jobs.OrderByDescending(j => j.Id); // Sắp xếp công việc mới nhất lên đầu
                    break;
            }
            ViewData["TotalCompleted"] = await jobs.CountAsync(j => j.Status == 1);
            ViewData["TotalNotCompleted"] = await jobs.CountAsync(j => j.Status == 2);
            ViewData["TotalLate"] = await jobs.CountAsync(j => j.Status == 3);
            ViewData["TotalProcessing"] = await jobs.CountAsync(j => j.Status == 4);
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            return View(await jobs.ToListAsync());
        }

        public async Task<IActionResult> ExportToExcel(string searchText, int? status, int? categoryId, bool dueToday, string sortOrder, string month)
        {
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");
            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
            }

            var manager = await _context.Users
                .Where(u => u.UserName == managerUsername)
                .Select(u => u.Employee)
                .FirstOrDefaultAsync();

            if (manager == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var managedDepartments = await _context.Departments
                .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 2))
                .Select(d => d.Id)
                .ToListAsync();

            var employeesInManagedDepartments = await _context.Employees
                .Where(e => e.DepartmentId.HasValue && managedDepartments.Contains(e.DepartmentId.Value))
                .Select(e => e.Id)
                .ToListAsync();

            var jobs = _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Employee)
                .Where(j => j.EmployeeId.HasValue && employeesInManagedDepartments.Contains(j.EmployeeId.Value));

            // Áp dụng các bộ lọc giống như trong Index
            if (!string.IsNullOrEmpty(searchText))
            {
                jobs = jobs.Where(j =>
                    j.Employee.Code.Contains(searchText) ||
                    j.Employee.FirstName.Contains(searchText) ||
                    j.Employee.LastName.Contains(searchText) ||
                    j.Name.Contains(searchText));
            }

            if (status.HasValue)
            {
                jobs = jobs.Where(j => j.Status == status.Value);
            }

            if (categoryId.HasValue)
            {
                jobs = jobs.Where(j => j.CategoryId == categoryId.Value);
            }

            if (dueToday)
            {
                jobs = jobs.Where(j => j.Time.HasValue && j.Time.Value.Date == DateTime.Today);
            }

            if (!string.IsNullOrEmpty(month) && DateTime.TryParse(month + "-01", out DateTime selectedMonth))
            {
                jobs = jobs.Where(j => j.Time.HasValue &&
                                      j.Time.Value.Year == selectedMonth.Year &&
                                      j.Time.Value.Month == selectedMonth.Month);
            }

            switch (sortOrder)
            {
                case "due_asc":
                    jobs = jobs.OrderBy(j => j.Time);
                    break;
                case "due_desc":
                    jobs = jobs.OrderByDescending(j => j.Time);
                    break;
                case "review_asc":
                    jobs = jobs.OrderBy(j => j.SummaryOfReviews);
                    break;
                case "review_desc":
                    jobs = jobs.OrderByDescending(j => j.SummaryOfReviews);
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
                    var job = jobList[i];
                    worksheet.Cells[i + 2, 1].Value = i + 1; // STT
                    worksheet.Cells[i + 2, 2].Value = $"{job.Employee.Code} {job.Employee.FirstName} {job.Employee.LastName}";
                    worksheet.Cells[i + 2, 3].Value = job.Category.Name;
                    worksheet.Cells[i + 2, 4].Value = job.Name;
                    worksheet.Cells[i + 2, 5].Value = job.Time?.ToString("dd/MM/yyyy");
                    worksheet.Cells[i + 2, 6].Value = job.Time?.ToString("dd/MM/yyyy"); // Giả sử ngày hoàn thành bằng Deadline
                    worksheet.Cells[i + 2, 7].Value = job.Status switch
                    {
                        1 => "Hoàn thành",
                        2 => "Chưa hoàn thành",
                        3 => "Hoàn thành muộn",
                        _ => "Đang xử lý"
                    };
                    worksheet.Cells[i + 2, 8].Value = job.VolumeAssessment;
                    worksheet.Cells[i + 2, 9].Value = job.ProgressAssessment;
                    worksheet.Cells[i + 2, 10].Value = job.QualityAssessment;
                    worksheet.Cells[i + 2, 11].Value = job.SummaryOfReviews;
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

            var job = await _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (job == null)
            {
                return NotFound();
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Details", job);
            }
            return View(job);
        }

        // GET: ProjectManager/Jobs/Create
        public IActionResult Create()
        {
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");

            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
            }

            var manager = _context.Users
                .Include(u => u.Employee)
                .FirstOrDefault(u => u.UserName == managerUsername)
                ?.Employee;

            if (manager == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var managedDepartments = _context.Departments
                .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 2))
                .Select(d => d.Id)
                .ToList();

            var employeesInManagedDepartments = _context.Employees
                .Where(e => e.DepartmentId.HasValue && managedDepartments.Contains(e.DepartmentId.Value))
                .Select(e => new
                {
                    Id = e.Id,
                    FullName = e.Code + "-" + e.FirstName + " " + e.LastName
                })
                .ToList();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["EmployeeId"] = new SelectList(employeesInManagedDepartments, "Id", "FullName");
            ViewData["Employees"] = new MultiSelectList(employeesInManagedDepartments, "Id", "FullName");

            var newJob = new Job
            {
                Time = DateTime.Now,
                Status = 4 // Đặt mặc định là "Đang xử lý"
            };
            return View(newJob);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    [Bind("Id,EmployeeId,CategoryId,Name,Description,Deadline1,Deadline2,Deadline3,CompletionDate,Status,VolumeAssessment,ProgressAssessment,QualityAssessment,SummaryOfReviews,Time,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Job job,
    List<int> SelectedEmployees,
    int? SingleEmployeeId,
    List<Job> Jobs)
        {
            if (ModelState.IsValid)
            {
                // Logic hiện tại giữ nguyên
                if (SelectedEmployees != null && SelectedEmployees.Count > 0)
                {
                    // Nhiều nhân viên - Một công việc
                    foreach (var empId in SelectedEmployees)
                    {
                        var newJob = new Job
                        {
                            EmployeeId = empId,
                            CategoryId = job.CategoryId,
                            Name = job.Name,
                            Description = job.Description,
                            Deadline1 = job.Deadline1,
                            Deadline2 = job.Deadline2,
                            Deadline3 = job.Deadline3,
                            CompletionDate = job.CompletionDate,
                            Status = 4,
                            Time = DateTime.Now,
                            IsActive = true,
                            IsDelete = false,
                            CreateDate = DateTime.Now,
                            CreateBy = HttpContext.Session.GetString("ProjectManagerLogin")
                        };
                        _context.Jobs.Add(newJob);
                        await _context.SaveChangesAsync();
                        await UpdateAnalysis(empId);
                    }
                }
                else if (SingleEmployeeId.HasValue && Jobs != null && Jobs.Count > 0)
                {
                    // Một nhân viên - Nhiều công việc
                    foreach (var j in Jobs)
                    {
                        j.EmployeeId = SingleEmployeeId.Value;
                        j.Status = 4;
                        j.Time = DateTime.Now;
                        j.IsActive = true;
                        j.IsDelete = false;
                        j.CreateDate = DateTime.Now;
                        j.CreateBy = HttpContext.Session.GetString("ProjectManagerLogin");
                        _context.Jobs.Add(j);
                        await _context.SaveChangesAsync();
                        await UpdateAnalysis(j.EmployeeId.Value);
                    }
                }
                else
                {
                    // Một nhân viên - Một công việc (mặc định)

                    job.Status = 4;
                    job.Time = DateTime.Now;
                    job.IsActive = true;
                    job.IsDelete = false;
                    job.CreateDate = DateTime.Now;
                    job.CreateBy = HttpContext.Session.GetString("ProjectManagerLogin");
                    _context.Jobs.Add(job);
                    await _context.SaveChangesAsync();
                    await UpdateAnalysis(job.EmployeeId.Value);
                }
                return RedirectToAction(nameof(Index));
            }

            // Cập nhật ViewBag ngay cả khi ModelState không hợp lệ
            var employees = _context.Employees
                .Select(e => new { Id = e.Id, FullName = e.FirstName + " " + e.LastName })
                .ToList();
            ViewBag.CategoryId = new SelectList(_context.Categories, "Id", "Name", job.CategoryId);
            ViewBag.EmployeeId = new SelectList(employees, "Id", "FullName", job.EmployeeId); // Đảm bảo giá trị được giữ
            ViewBag.Employees = new MultiSelectList(employees, "Id", "FullName", SelectedEmployees);
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

            ViewData["CategoryId"] = new SelectList(
                _context.Categories.Select(c => new { c.Id, Display = c.Code + " - " + c.Name }),
                "Id", "Display", job.CategoryId
            );

            ViewData["EmployeeId"] = new SelectList(
                _context.Employees.Select(e => new { e.Id, Display = e.Code + " - " + e.FirstName + " " + e.LastName }),
                "Id", "Display", job.EmployeeId
            );
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
                    await UpdateBaselineAssessment(job.EmployeeId);
                    // 🔹 Cập nhật bảng Analysis sau khi chỉnh sửa Job
                    await UpdateAnalysis(job.EmployeeId);
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
            bool evaluate = sumSummary >= 6;

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
            var jobs = await _context.Jobs
                .Where(j => j.EmployeeId == employeeId && j.Time.HasValue &&
                            j.Time.Value.Month == currentMonth && j.Time.Value.Year == currentYear)
                .ToListAsync();

            int total = jobs.Count;
            int ontime = jobs.Count(j => j.Status == 1);
            int late = jobs.Count(j => j.Status == 2);
            int overdue = jobs.Count(j => j.Status == 3);
            int processing = jobs.Count(j => j.Status == 4);

            // Tính trung bình đánh giá của nhân viên
            var averageReview = jobs.Any()
                ? jobs.Average(j => j.SummaryOfReviews ?? 0)
                : 0;

            // Tìm bản ghi Analysis của nhân viên trong tháng hiện tại
            var analysis = await _context.Analyses
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Time.HasValue &&
                                          a.Time.Value.Month == currentMonth && a.Time.Value.Year == currentYear);

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
                analysis.UpdateDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();
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
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Delete", job);
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
