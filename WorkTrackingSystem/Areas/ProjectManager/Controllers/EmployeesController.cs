using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
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
        [HttpGet]
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

            // Xác định tháng, chỉ sử dụng nếu month được truyền vào
            DateTime? selectedMonth = null;
            if (!string.IsNullOrEmpty(month))
            {
                if (DateTime.TryParseExact(month, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime parsedMonth))
                {
                    selectedMonth = parsedMonth;
                }
            }

            // Lấy danh sách công việc của nhân viên
            var jobsQuery = _context.Jobs.Where(j => j.EmployeeId == id);

            // Lọc theo tháng chỉ khi selectedMonth có giá trị
            if (selectedMonth.HasValue)
            {
                jobsQuery = jobsQuery.Where(j => j.Time.HasValue
                    && j.Time.Value.Year == selectedMonth.Value.Year
                    && j.Time.Value.Month == selectedMonth.Value.Month);
            }

            // Áp dụng các bộ lọc khác
            if (!string.IsNullOrEmpty(searchText))
            {
                jobsQuery = jobsQuery.Where(j => j.Name.Contains(searchText) || j.Description.Contains(searchText));
            }

            if (startDate.HasValue)
            {
                jobsQuery = jobsQuery.Where(j => j.Time.HasValue && j.Time.Value >= startDate.Value.ToDateTime(TimeOnly.MinValue));
            }

            if (endDate.HasValue)
            {
                jobsQuery = jobsQuery.Where(j => j.Time.HasValue && j.Time.Value <= endDate.Value.ToDateTime(TimeOnly.MaxValue));
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
                jobsQuery = jobsQuery.Where(j => j.Time.HasValue && j.Time.Value.Date == today.ToDateTime(TimeOnly.MinValue).Date);
            }

            // Sắp xếp
            jobsQuery = sortOrder switch
            {
                "due_asc" => jobsQuery.OrderBy(j => j.Time),
                "due_desc" => jobsQuery.OrderByDescending(j => j.Time),
                "review_asc" => jobsQuery.OrderBy(j => j.SummaryOfReviews),
                "review_desc" => jobsQuery.OrderByDescending(j => j.SummaryOfReviews),
                _ => jobsQuery.OrderBy(j => j.Time ?? DateTime.MaxValue)
            };

            var jobs = jobsQuery.ToList();

            // Lấy đánh giá và phân tích của nhân viên theo tháng (nếu có tháng được chọn)
            Baselineassessment baseline = null;
            Analysis analysis = null;
            if (selectedMonth.HasValue)
            {
                baseline = _context.Baselineassessments
                    .FirstOrDefault(b => b.EmployeeId == id
                        && b.Time.HasValue
                        && b.Time.Value.Year == selectedMonth.Value.Year
                        && b.Time.Value.Month == selectedMonth.Value.Month)
                    ?? new Baselineassessment(); // Khởi tạo mặc định nếu null

                analysis = _context.Analyses
                    .FirstOrDefault(a => a.EmployeeId == id
                        && a.Time.HasValue
                        && a.Time.Value.Year == selectedMonth.Value.Year
                        && a.Time.Value.Month == selectedMonth.Value.Month)
                    ?? new Analysis(); // Khởi tạo mặc định nếu null
            }
            else
            {
                baseline = _context.Baselineassessments
                    .Where(b => b.EmployeeId == id)
                    .OrderByDescending(b => b.Time)
                    .FirstOrDefault()
                    ?? new Baselineassessment(); // Khởi tạo mặc định nếu null

                analysis = _context.Analyses
                    .Where(a => a.EmployeeId == id)
                    .OrderByDescending(a => a.Time)
                    .FirstOrDefault()
                    ?? new Analysis(); // Khởi tạo mặc định nếu null
            }

            // Chuẩn bị dữ liệu cho view
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.SelectedMonth = selectedMonth;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.SearchText = searchText;
            ViewBag.Status = status;
            ViewBag.CategoryId = categoryId;
            ViewBag.SortOrder = sortOrder;
            ViewBag.ShowCompletedZeroReview = showCompletedZeroReview;
            ViewBag.DueToday = dueToday;

            // Tạo ViewModel
            var viewModel = new EmployeeDetailsViewModel
            {
                Employee = employee,
                Jobs = jobs,
                BaselineAssessment = baseline,
                Analysis = analysis,
                SelectedMonth = selectedMonth ?? DateTime.Now // Nếu không chọn tháng, có thể để mặc định hoặc null
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(long id,
    string searchText = null,
    DateOnly? startDate = null,
    DateOnly? endDate = null,
    int? status = null,
    int? categoryId = null,
    string sortOrder = null,
    bool showCompletedZeroReview = false,
    bool dueToday = false,
    string month = null)
        {
            // Lấy thông tin nhân viên
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            // Xác định tháng (mặc định là tháng hiện tại)
            DateTime selectedMonth = DateTime.Now;
            if (!string.IsNullOrEmpty(month))
            {
                if (DateTime.TryParseExact(month, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime parsedMonth))
                {
                    selectedMonth = parsedMonth;
                }
            }

            // Lấy danh sách công việc của nhân viên
            var jobsQuery = _context.Jobs
                .Include(j => j.Category)
                .Include(j => j.Employee)
                .Where(j => j.EmployeeId == id);

            // Lọc theo tháng (luôn áp dụng nếu có month)
            if (!string.IsNullOrEmpty(month))
            {
                jobsQuery = jobsQuery.Where(j => j.Deadline1.HasValue
                    && j.Deadline1.Value.Year == selectedMonth.Year
                    && j.Deadline1.Value.Month == selectedMonth.Month);
            }

            // Áp dụng các bộ lọc khác
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
            jobsQuery = sortOrder switch
            {
                "due_asc" => jobsQuery.OrderBy(j => j.Deadline1),
                "due_desc" => jobsQuery.OrderByDescending(j => j.Deadline1),
                "review_asc" => jobsQuery.OrderBy(j => j.SummaryOfReviews),
                "review_desc" => jobsQuery.OrderByDescending(j => j.SummaryOfReviews),
                _ => jobsQuery.OrderBy(j => j.Deadline1 ?? DateOnly.MaxValue)
            };

            var jobList = await jobsQuery.ToListAsync();

            // Tạo file Excel bằng EPPlus
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Jobs");

                // Xác định tiêu đề báo cáo
                string reportTitle = !string.IsNullOrEmpty(month)
                    ? $"Tổng hợp công việc tháng {selectedMonth:MM/yyyy}"
                    : (startDate.HasValue && endDate.HasValue
                        ? $"Tổng hợp công việc từ {startDate.Value:dd/MM/yyyy} đến {endDate.Value:dd/MM/yyyy}"
                        : "Tổng hợp công việc tất cả");

                // Thêm tiêu đề vào dòng đầu tiên
                worksheet.Cells[1, 1].Value = reportTitle;
                worksheet.Cells[1, 1, 1, 11].Merge = true;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Style.Font.Bold = true;

                // Tiêu đề cột
                worksheet.Cells[2, 1].Value = "STT";
                worksheet.Cells[2, 2].Value = "Nhân viên";
                worksheet.Cells[2, 3].Value = "Hạng mục";
                worksheet.Cells[2, 4].Value = "Công việc";
                worksheet.Cells[2, 5].Value = "Deadline";
                worksheet.Cells[2, 6].Value = "Ngày hoàn thành";
                worksheet.Cells[2, 7].Value = "Trạng thái";
                worksheet.Cells[2, 8].Value = "Đánh giá khối lượng";
                worksheet.Cells[2, 9].Value = "Đánh giá tiến độ";
                worksheet.Cells[2, 10].Value = "Đánh giá chất lượng";
                worksheet.Cells[2, 11].Value = "Đánh giá tổng hợp";

                // Điền dữ liệu công việc
                for (int i = 0; i < jobList.Count; i++)
                {
                    var job = jobList[i];
                    int rowIndex = i + 3; // Bắt đầu từ dòng 3

                    worksheet.Cells[rowIndex, 1].Value = i + 1;
                    worksheet.Cells[rowIndex, 2].Value = $"{job.Employee.Code} {job.Employee.FirstName} {job.Employee.LastName}";
                    worksheet.Cells[rowIndex, 3].Value = job.Category?.Name ?? "N/A";
                    worksheet.Cells[rowIndex, 4].Value = job.Name;
                    worksheet.Cells[rowIndex, 5].Value = job.Deadline1?.ToString("dd/MM/yyyy") ?? "N/A";
                    worksheet.Cells[rowIndex, 6].Value = job.CompletionDate?.ToString("dd/MM/yyyy") ?? "N/A";
                    worksheet.Cells[rowIndex, 7].Value = job.Status switch
                    {
                        1 => "Hoàn thành",
                        2 => "Chưa hoàn thành",
                        3 => "Hoàn thành muộn",
                        4 => "Đang xử lý",
                        0 => "Chưa bắt đầu",
                        _ => "Không xác định"
                    };
                    worksheet.Cells[rowIndex, 8].Value = job.VolumeAssessment ?? 0;
                    worksheet.Cells[rowIndex, 9].Value = job.ProgressAssessment ?? 0;
                    worksheet.Cells[rowIndex, 10].Value = job.QualityAssessment ?? 0;
                    worksheet.Cells[rowIndex, 11].Value = job.SummaryOfReviews ?? 0;
                }

                // Định dạng cột
                worksheet.Cells[2, 1, 2, 11].Style.Font.Bold = true;
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Xuất file Excel
                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Jobs_{employee.Code}_{selectedMonth:yyyyMM}.xlsx");
            }
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
            // Kiểm tra Jobs có null hoặc rỗng không
            if (Jobs == null || !Jobs.Any())
            {
                return BadRequest("Không có dữ liệu công việc để cập nhật.");
            }

            // Lấy thông tin employeeId từ job đầu tiên
            var firstJob = await _context.Jobs.FindAsync(Jobs.First().Id);
            if (firstJob == null)
            {
                return NotFound("Không tìm thấy công việc đầu tiên.");
            }
            long employeeId = firstJob.EmployeeId ?? 0;

            // Cập nhật từng job
            foreach (var jobModel in Jobs)
            {
                var job = await _context.Jobs.FindAsync(jobModel.Id);
                if (job == null)
                {
                    continue; // Bỏ qua nếu không tìm thấy job
                }

                // Cập nhật các giá trị đánh giá
                job.VolumeAssessment = jobModel.VolumeAssessment;
                job.ProgressAssessment = jobModel.ProgressAssessment;
                job.QualityAssessment = jobModel.QualityAssessment;

                // Tính lại SummaryOfReviews nếu tất cả các giá trị đều có
                if (job.VolumeAssessment.HasValue && job.ProgressAssessment.HasValue && job.QualityAssessment.HasValue)
                {
                    job.SummaryOfReviews = (float)((job.VolumeAssessment.Value * 0.6) +
                                                   (job.ProgressAssessment.Value * 0.15) +
                                                   (job.QualityAssessment.Value * 0.25));
                }
                else
                {
                    job.SummaryOfReviews = null;
                }

                _context.Update(job);
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lưu dữ liệu: {ex.Message}");
            }

            // Cập nhật BaselineAssessment
            await UpdateBaselineAssessment(employeeId);

            // Tái tạo viewModel với bộ lọc
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
            {
                return NotFound("Không tìm thấy nhân viên.");
            }

            // Xác định tháng
            DateTime selectedMonth = string.IsNullOrEmpty(month)
                ? DateTime.Now
                : DateTime.TryParseExact(month, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime parsedMonth)
                    ? parsedMonth
                    : DateTime.Now;

            // Lấy danh sách công việc với bộ lọc
            var jobsQuery = _context.Jobs.Where(j => j.EmployeeId == employeeId);

            // Áp dụng bộ lọc mặc định theo tháng nếu không có bộ lọc khác
            bool hasFilters = !string.IsNullOrEmpty(searchText) ||
                             startDate.HasValue ||
                             endDate.HasValue ||
                             status.HasValue ||
                             categoryId.HasValue ||
                             showCompletedZeroReview ||
                             dueToday ||
                             !string.IsNullOrEmpty(sortOrder);

            if (!hasFilters)
            {
                jobsQuery = jobsQuery.Where(j => j.Time.HasValue
                    && j.Time.Value.Year == selectedMonth.Year
                    && j.Time.Value.Month == selectedMonth.Month);
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                jobsQuery = jobsQuery.Where(j => j.Name.Contains(searchText) || j.Description.Contains(searchText));
            }

            if (startDate.HasValue)
            {
                jobsQuery = jobsQuery.Where(j => j.Time.HasValue && j.Time.Value >= startDate.Value.ToDateTime(TimeOnly.MinValue));
            }

            if (endDate.HasValue)
            {
                jobsQuery = jobsQuery.Where(j => j.Time.HasValue && j.Time.Value <= endDate.Value.ToDateTime(TimeOnly.MaxValue));
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
                jobsQuery = jobsQuery.Where(j => j.Time.HasValue && j.Time.Value.Date == today.ToDateTime(TimeOnly.MinValue).Date);
            }

            // Sắp xếp
            jobsQuery = sortOrder switch
            {
                "due_asc" => jobsQuery.OrderBy(j => j.Time),
                "due_desc" => jobsQuery.OrderByDescending(j => j.Time),
                "review_asc" => jobsQuery.OrderBy(j => j.SummaryOfReviews),
                "review_desc" => jobsQuery.OrderByDescending(j => j.SummaryOfReviews),
                _ => jobsQuery.OrderBy(j => j.Time ?? DateTime.MaxValue)
            };

            var jobs = await jobsQuery.ToListAsync();

            // Lấy BaselineAssessment và Analysis
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

            // Chuẩn bị dữ liệu cho view
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.SearchText = searchText;
            ViewBag.Status = status;
            ViewBag.CategoryId = categoryId;
            ViewBag.SortOrder = sortOrder;
            ViewBag.ShowCompletedZeroReview = showCompletedZeroReview;
            ViewBag.DueToday = dueToday;
            ViewBag.SelectedMonth = selectedMonth;

            var viewModel = new EmployeeDetailsViewModel
            {
                Employee = employee,
                Jobs = jobs,
                BaselineAssessment = baseline,
                Analysis = analysis,
                SelectedMonth = selectedMonth
            };

            return View("Details", viewModel);
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


            int ontime = jobs.Count(j => j.Status == 1);
            int late = jobs.Count(j => j.Status == 2);
            int overdue = jobs.Count(j => j.Status == 3);
            int processing = jobs.Count(j => j.Status == 4);
            int total = ontime + late + overdue + processing;
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