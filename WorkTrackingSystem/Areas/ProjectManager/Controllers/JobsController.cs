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
    public class JobsController : Controller
    {
        private readonly WorkTrackingSystemContext _context;

        public JobsController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: ProjectManager/Jobs
        public async Task<IActionResult> Index(string searchText, int? status, int? categoryId, bool dueToday, string sortOrder)
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
            }
            ViewData["TotalCompleted"] = await jobs.CountAsync(j => j.Status == 1);
            ViewData["TotalNotCompleted"] = await jobs.CountAsync(j => j.Status == 2);
            ViewData["TotalLate"] = await jobs.CountAsync(j => j.Status == 3);
            ViewData["TotalProcessing"] = await jobs.CountAsync(j => j.Status == 4);
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            return View(await jobs.ToListAsync());
        }
        public async Task<IActionResult> ExportToExcel()
        {
            // Lấy dữ liệu công việc và thống kê theo tháng
            var jobs = await _context.Jobs
                .Include(j => j.Employee)
                .Include(j => j.Category)
                .ToListAsync();

            var jobStats = await _context.Jobs
                .GroupBy(j => new { j.Time.Value.Month, j.Time.Value.Year })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    TotalCompleted = g.Count(j => j.Status == 1),
                    TotalNotCompleted = g.Count(j => j.Status == 2),
                    TotalLate = g.Count(j => j.Status == 3),
                    TotalProcessing = g.Count(j => j.Status == 0)
                })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("DanhSachCongViec");

                // Thiết lập tiêu đề
                worksheet.Cells["A1:G1"].Merge = true;
                worksheet.Cells["A1"].Value = "DANH SÁCH CÔNG VIỆC";
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Header
                worksheet.Cells[3, 1].Value = "STT";
                worksheet.Cells[3, 2].Value = "Nhân viên";
                worksheet.Cells[3, 3].Value = "Tên công việc";
                worksheet.Cells[3, 4].Value = "Trạng thái";
                worksheet.Cells[3, 5].Value = "Đánh giá tổng hợp";
                worksheet.Cells[3, 6].Value = "Hạn hoàn thành";
                worksheet.Cells[3, 7].Value = "Danh mục";

                using (var range = worksheet.Cells["A3:G3"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Dữ liệu công việc
                int row = 4;
                int index = 1;
                foreach (var job in jobs)
                {
                    worksheet.Cells[row, 1].Value = index++;
                    worksheet.Cells[row, 2].Value = job.Employee != null ? $"{job.Employee.Code} - {job.Employee.FirstName} {job.Employee.LastName}" : "Chưa có nhân viên";
                    worksheet.Cells[row, 3].Value = job.Name;
                    worksheet.Cells[row, 4].Value = job.Status == 1 ? "Hoàn thành" :
                                                    job.Status == 2 ? "Chưa hoàn thành" :
                                                    job.Status == 3 ? "Hoàn thành muộn" : job.Status == 4 ?  "Đang xử lý":
                    worksheet.Cells[row, 5].Value = job.SummaryOfReviews;
                    worksheet.Cells[row, 6].Value = job.Time.HasValue ? job.Time.Value.ToString("dd/MM/yyyy") : "N/A";
                    worksheet.Cells[row, 7].Value = job.Category != null ? job.Category.Name : "Chưa có danh mục";

                    row++;
                }

                worksheet.Cells.AutoFitColumns();

                // Thêm sheet thống kê công việc
                var statsSheet = package.Workbook.Worksheets.Add("ThongKe");

                statsSheet.Cells["A1"].Value = "Thống kê công việc theo tháng";
                statsSheet.Cells["A1:E1"].Merge = true;
                statsSheet.Cells["A1"].Style.Font.Size = 14;
                statsSheet.Cells["A1"].Style.Font.Bold = true;
                statsSheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Header thống kê
                statsSheet.Cells[3, 1].Value = "Tháng";
                statsSheet.Cells[3, 2].Value = "Hoàn thành";
                statsSheet.Cells[3, 3].Value = "Chưa hoàn thành";
                statsSheet.Cells[3, 4].Value = "Hoàn thành muộn";
                statsSheet.Cells[3, 5].Value = "Đang xử lý";

                using (var range = statsSheet.Cells["A3:E3"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Đổ dữ liệu thống kê
                int statsRow = 4;
                foreach (var stat in jobStats)
                {
                    statsSheet.Cells[statsRow, 1].Value = $"{stat.Month}/{stat.Year}";
                    statsSheet.Cells[statsRow, 2].Value = stat.TotalCompleted;
                    statsSheet.Cells[statsRow, 3].Value = stat.TotalNotCompleted;
                    statsSheet.Cells[statsRow, 4].Value = stat.TotalLate;
                    statsSheet.Cells[statsRow, 5].Value = stat.TotalProcessing;
                    statsRow++;
                }

                statsSheet.Cells.AutoFitColumns();

                // Thêm biểu đồ thống kê
                var chart = statsSheet.Drawings.AddChart("chart", eChartType.ColumnClustered);
                chart.Title.Text = "Thống kê công việc theo tháng";
                chart.SetPosition(1, 0, 7, 0); // Vị trí biểu đồ
                chart.SetSize(800, 400); // Kích thước

                var series1 = chart.Series.Add(statsSheet.Cells[$"B4:B{statsRow - 1}"], statsSheet.Cells[$"A4:A{statsRow - 1}"]);
                series1.Header = "Hoàn thành";

                var series2 = chart.Series.Add(statsSheet.Cells[$"C4:C{statsRow - 1}"], statsSheet.Cells[$"A4:A{statsRow - 1}"]);
                series2.Header = "Chưa hoàn thành";

                var series3 = chart.Series.Add(statsSheet.Cells[$"D4:D{statsRow - 1}"], statsSheet.Cells[$"A4:A{statsRow - 1}"]);
                series3.Header = "Hoàn thành muộn";

                var series4 = chart.Series.Add(statsSheet.Cells[$"E4:E{statsRow - 1}"], statsSheet.Cells[$"A4:A{statsRow - 1}"]);
                series4.Header = "Đang xử lý";

                // Xuất file Excel
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachCongViec.xlsx");
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
                    FullName = e.FirstName + " " + e.LastName
                })
                .ToList();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");

            // Truyền danh sách nhân viên (hỗ trợ chọn nhiều nhân viên)
            ViewData["EmployeeId"] = new SelectList(employeesInManagedDepartments, "Id", "FullName");
            ViewData["Employees"] = new MultiSelectList(employeesInManagedDepartments, "Id", "FullName");

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
        public async Task<IActionResult> Create(
    [Bind("Id,EmployeeId,CategoryId,Name,Description,Deadline1,Deadline2,Deadline3,CompletionDate,Status,VolumeAssessment,ProgressAssessment,QualityAssessment,SummaryOfReviews,Time,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Job job,
    List<int> SelectedEmployees,
    int? SingleEmployeeId)
        {
            if (ModelState.IsValid)
            {
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
                            CompletionDate = job.CompletionDate,
                            Status = 4,
                            Time = DateTime.Now,
                            IsActive = true,
                            IsDelete = false
                        };
                        _context.Jobs.Add(newJob);
                        await _context.SaveChangesAsync(); // Lưu từng job để có ID trước khi cập nhật analysis
                        await UpdateAnalysis(empId); // Cập nhật bảng Analysis
                    }
                }
                else if (SingleEmployeeId.HasValue)
                {
                    // Một nhân viên - Một công việc
                    job.EmployeeId = SingleEmployeeId.Value;
                    job.Status = 4;
                    job.Time = DateTime.Now;
                    _context.Jobs.Add(job);
                    await _context.SaveChangesAsync();
                    await UpdateAnalysis(job.EmployeeId); // Cập nhật bảng Analysis
                }
                else
                {
                    // Một nhân viên - Một công việc (mặc định)
                    job.Status = 4;
                    job.Time = DateTime.Now;
                    _context.Jobs.Add(job);
                    await _context.SaveChangesAsync();
                    await UpdateAnalysis(job.EmployeeId); // Cập nhật bảng Analysis
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", job.CategoryId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", job.EmployeeId);
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
