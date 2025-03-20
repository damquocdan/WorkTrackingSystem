//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.CodeAnalysis.Elfie.Diagnostics;
//using Microsoft.Data.SqlClient;
//using Microsoft.EntityFrameworkCore;
//using OfficeOpenXml;
//using WorkTrackingSystem.Models;

//namespace WorkTrackingSystem.Areas.ProjectManager.Controllers
//{
//    [Area("ProjectManager")]
//    public class EmployeesController : BaseController
//    {
//        private readonly WorkTrackingSystemContext _context;

//        public EmployeesController(WorkTrackingSystemContext context)
//        {
//            _context = context;
//        }



//        // GET: ProjectManager/Employees
//        public async Task<IActionResult> Index(string searchTerm, int? positionId)
//        {
//            // Lấy ManagerId từ session
//            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");

//            if (string.IsNullOrEmpty(managerUsername))
//            {
//                return RedirectToAction("Index", "Login");
//            }

//            // Tìm nhân viên đang đăng nhập
//            var manager = await _context.Users
//                .Where(u => u.UserName == managerUsername)
//                .Select(u => u.Employee)
//                .FirstOrDefaultAsync();

//            if (manager == null)
//            {
//                return RedirectToAction("Index", "Login");
//            }

//            // Lấy danh sách phòng ban mà nhân viên này quản lý
//            var managedDepartments = await _context.Departments
//                .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3)) // 3 = Quản lý
//                .Select(d => d.Id)
//                .ToListAsync();

//            // Lọc danh sách nhân viên theo phòng ban mà quản lý này phụ trách
//            var employeesQuery = _context.Employees
//                .Include(e => e.Department)
//                .Include(e => e.Position)
//                .Where(e => e.DepartmentId.HasValue && managedDepartments.Contains(e.DepartmentId.Value));

//            // Lọc theo chức vụ nếu có
//            if (positionId.HasValue && positionId > 0)
//            {
//                employeesQuery = employeesQuery.Where(e => e.PositionId == positionId.Value);
//            }

//            // Lọc theo mã nhân viên, họ và tên
//            if (!string.IsNullOrEmpty(searchTerm))
//            {
//                employeesQuery = employeesQuery.Where(e =>
//                    e.Code.Contains(searchTerm) ||
//                    e.FirstName.Contains(searchTerm) ||
//                    e.LastName.Contains(searchTerm));
//            }

//            // Trả kết quả về View
//            var employees = await employeesQuery.ToListAsync();
//            ViewBag.Positions = await _context.Positions.ToListAsync(); // Gửi danh sách chức vụ để lọc
//            return View(employees);
//        }



//        // GET: ProjectManager/Employees/Details/5
//        [HttpGet]
//        public IActionResult Details(long id,
//    string month = null,
//    string searchText = null,
//    DateOnly? startDate = null,
//    DateOnly? endDate = null,
//    int? status = null,
//    int? categoryId = null,
//    string sortOrder = null,
//    bool showCompletedZeroReview = false,
//    bool dueToday = false)
//        {
//            // Lấy nhân viên
//            var employee = _context.Employees.Find(id);
//            if (employee == null)
//            {
//                return NotFound();
//            }

//            // Xác định tháng
//            DateTime? selectedMonth = null;
//            if (!string.IsNullOrEmpty(month) &&
//                DateTime.TryParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedMonth))
//            {
//                selectedMonth = parsedMonth;
//            }

//            // Tạo truy vấn cơ bản cho Jobs
//            var jobsQuery = _context.Jobs.AsNoTracking()
//                .Where(j => j.EmployeeId == id && (j.IsDelete == false || j.IsDelete == null));

//            // Áp dụng bộ lọc theo tháng nếu có
//            if (selectedMonth.HasValue)
//            {
//                jobsQuery = jobsQuery.Where(j => j.Time.HasValue
//                    && j.Time.Value.Year == selectedMonth.Value.Year
//                    && j.Time.Value.Month == selectedMonth.Value.Month);
//            }

//            // Áp dụng các bộ lọc khác
//            if (!string.IsNullOrEmpty(searchText))
//            {
//                searchText = searchText.Trim().ToLower();
//                jobsQuery = jobsQuery.Where(j => j.Name.ToLower().Contains(searchText)
//                    || (j.Description != null && j.Description.ToLower().Contains(searchText)));
//            }

//            if (startDate.HasValue)
//            {
//                var startDateTime = startDate.Value.ToDateTime(TimeOnly.MinValue);
//                jobsQuery = jobsQuery.Where(j => j.Time.HasValue && j.Time.Value >= startDateTime);
//            }

//            if (endDate.HasValue)
//            {
//                var endDateTime = endDate.Value.ToDateTime(TimeOnly.MaxValue);
//                jobsQuery = jobsQuery.Where(j => j.Time.HasValue && j.Time.Value <= endDateTime);
//            }

//            if (status.HasValue)
//            {
//                jobsQuery = jobsQuery.Where(j => j.Status == status.Value);
//            }

//            if (categoryId.HasValue)
//            {
//                jobsQuery = jobsQuery.Where(j => j.CategoryId == categoryId.Value);
//            }

//            if (showCompletedZeroReview)
//            {
//                jobsQuery = jobsQuery.Where(j => j.Status == 1
//                    && (j.SummaryOfReviews == null || j.SummaryOfReviews == 0));
//            }

//            if (dueToday)
//            {
//                var today = DateOnly.FromDateTime(DateTime.Today);
//                jobsQuery = jobsQuery.Where(j => j.Time.HasValue
//                    && j.Time.Value.Date == today.ToDateTime(TimeOnly.MinValue).Date);
//            }

//            // Sắp xếp
//            jobsQuery = sortOrder switch
//            {
//                "due_asc" => jobsQuery.OrderBy(j => j.Time ?? DateTime.MaxValue),
//                "due_desc" => jobsQuery.OrderByDescending(j => j.Time ?? DateTime.MinValue),
//                "review_asc" => jobsQuery.OrderBy(j => j.SummaryOfReviews ?? 0),
//                "review_desc" => jobsQuery.OrderByDescending(j => j.SummaryOfReviews ?? 0),
//                _ => jobsQuery.OrderBy(j => j.Time ?? DateTime.MaxValue)
//            };

//            // Thực thi truy vấn
//            List<Job> jobs;
//            try
//            {
//                jobs = jobsQuery.ToList();
//            }
//            catch (SqlException ex)
//            {
//                // Ghi log lỗi để debug
//                return StatusCode(500, "An error occurred while retrieving job data.");
//            }

//            // Lấy tất cả BaselineAssessments và Analyses
//            var baselineQuery = _context.Baselineassessments.AsNoTracking()
//                .Where(b => b.EmployeeId == id);
//            var analysisQuery = _context.Analyses.AsNoTracking()
//                .Where(a => a.EmployeeId == id);

//            // Áp dụng bộ lọc thời gian nếu có
//            if (selectedMonth.HasValue)
//            {
//                baselineQuery = baselineQuery.Where(b => b.Time.HasValue
//                    && b.Time.Value.Year == selectedMonth.Value.Year
//                    && b.Time.Value.Month == selectedMonth.Value.Month);
//                analysisQuery = analysisQuery.Where(a => a.Time.HasValue
//                    && a.Time.Value.Year == selectedMonth.Value.Year
//                    && a.Time.Value.Month == selectedMonth.Value.Month);
//            }
//            else if (startDate.HasValue && endDate.HasValue)
//            {
//                var startDateTime = startDate.Value.ToDateTime(TimeOnly.MinValue);
//                var endDateTime = endDate.Value.ToDateTime(TimeOnly.MaxValue);
//                baselineQuery = baselineQuery.Where(b => b.Time.HasValue
//                    && b.Time.Value >= startDateTime
//                    && b.Time.Value <= endDateTime);
//                analysisQuery = analysisQuery.Where(a => a.Time.HasValue
//                    && a.Time.Value >= startDateTime
//                    && a.Time.Value <= endDateTime);
//            }
//            else if (startDate.HasValue)
//            {
//                var startDateTime = startDate.Value.ToDateTime(TimeOnly.MinValue);
//                baselineQuery = baselineQuery.Where(b => b.Time.HasValue
//                    && b.Time.Value >= startDateTime);
//                analysisQuery = analysisQuery.Where(a => a.Time.HasValue
//                    && a.Time.Value >= startDateTime);
//            }
//            else if (endDate.HasValue)
//            {
//                var endDateTime = endDate.Value.ToDateTime(TimeOnly.MaxValue);
//                baselineQuery = baselineQuery.Where(b => b.Time.HasValue
//                    && b.Time.Value <= endDateTime);
//                analysisQuery = analysisQuery.Where(a => a.Time.HasValue
//                    && a.Time.Value <= endDateTime);
//            }

//            // Sắp xếp theo thời gian
//            var baselines = baselineQuery.OrderBy(b => b.Time ?? DateTime.MinValue).ToList();
//            var analyses = analysisQuery.OrderBy(a => a.Time ?? DateTime.MinValue).ToList();

//            // Chuẩn bị dữ liệu cho View
//            ViewData["Categories"] = new SelectList(_context.Categories.AsNoTracking(), "Id", "Name");
//            ViewBag.SelectedMonth = selectedMonth;
//            ViewBag.StartDate = startDate;
//            ViewBag.EndDate = endDate;
//            ViewBag.SearchText = searchText;
//            ViewBag.Status = status;
//            ViewBag.CategoryId = categoryId;
//            ViewBag.SortOrder = sortOrder;
//            ViewBag.ShowCompletedZeroReview = showCompletedZeroReview;
//            ViewBag.DueToday = dueToday;

//            // Tạo ViewModel
//            var viewModel = new EmployeeDetailsViewModel
//            {
//                Employee = employee,
//                Jobs = jobs,
//                BaselineAssessments = baselines,  // Sử dụng danh sách
//                Analyses = analyses,              // Sử dụng danh sách
//                SelectedMonth = selectedMonth ?? DateTime.Today
//            };

//            return View(viewModel);
//        }

//        [HttpGet]
//        public async Task<IActionResult> ExportToExcel(long id,
//    string searchText = null,
//    DateOnly? startDate = null,
//    DateOnly? endDate = null,
//    int? status = null,
//    int? categoryId = null,
//    string sortOrder = null,
//    bool showCompletedZeroReview = false,
//    bool dueToday = false,
//    string month = null)
//        {
//            // Lấy thông tin nhân viên
//            var employee = await _context.Employees.FindAsync(id);
//            if (employee == null)
//            {
//                return NotFound();
//            }

//            // Xác định tháng (mặc định là tháng hiện tại)
//            DateTime selectedMonth = DateTime.Now;
//            if (!string.IsNullOrEmpty(month))
//            {
//                if (DateTime.TryParseExact(month, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime parsedMonth))
//                {
//                    selectedMonth = parsedMonth;
//                }
//            }

//            // Lấy danh sách công việc của nhân viên
//            var jobsQuery = _context.Jobs
//                .Include(j => j.Category)
//                .Include(j => j.Employee)
//                .Where(j => j.EmployeeId == id);

//            // Lọc theo tháng (luôn áp dụng nếu có month)
//            if (!string.IsNullOrEmpty(month))
//            {
//                jobsQuery = jobsQuery.Where(j => j.Deadline1.HasValue
//                    && j.Deadline1.Value.Year == selectedMonth.Year
//                    && j.Deadline1.Value.Month == selectedMonth.Month);
//            }

//            // Áp dụng các bộ lọc khác
//            if (!string.IsNullOrEmpty(searchText))
//            {
//                jobsQuery = jobsQuery.Where(j => j.Name.Contains(searchText) || j.Description.Contains(searchText));
//            }

//            if (startDate.HasValue)
//            {
//                jobsQuery = jobsQuery.Where(j => j.Deadline1.HasValue && j.Deadline1.Value >= startDate.Value);
//            }

//            if (endDate.HasValue)
//            {
//                jobsQuery = jobsQuery.Where(j => j.Deadline1.HasValue && j.Deadline1.Value <= endDate.Value);
//            }

//            if (status.HasValue)
//            {
//                jobsQuery = jobsQuery.Where(j => j.Status == status.Value);
//            }

//            if (categoryId.HasValue)
//            {
//                jobsQuery = jobsQuery.Where(j => j.CategoryId == categoryId.Value);
//            }

//            if (showCompletedZeroReview)
//            {
//                jobsQuery = jobsQuery.Where(j => j.Status == 1 && (j.SummaryOfReviews == null || j.SummaryOfReviews == 0));
//            }

//            if (dueToday)
//            {
//                var today = DateOnly.FromDateTime(DateTime.Today);
//                jobsQuery = jobsQuery.Where(j => j.Deadline1.HasValue && j.Deadline1.Value == today);
//            }

//            // Sắp xếp
//            jobsQuery = sortOrder switch
//            {
//                "due_asc" => jobsQuery.OrderBy(j => j.Deadline1),
//                "due_desc" => jobsQuery.OrderByDescending(j => j.Deadline1),
//                "review_asc" => jobsQuery.OrderBy(j => j.SummaryOfReviews),
//                "review_desc" => jobsQuery.OrderByDescending(j => j.SummaryOfReviews),
//                _ => jobsQuery.OrderBy(j => j.Deadline1 ?? DateOnly.MaxValue)
//            };

//            var jobList = await jobsQuery.ToListAsync();

//            // Tạo file Excel bằng EPPlus
//            using (var package = new ExcelPackage())
//            {
//                var worksheet = package.Workbook.Worksheets.Add("Jobs");

//                // Xác định tiêu đề báo cáo
//                string reportTitle = !string.IsNullOrEmpty(month)
//                    ? $"Tổng hợp công việc tháng {selectedMonth:MM/yyyy}"
//                    : (startDate.HasValue && endDate.HasValue
//                        ? $"Tổng hợp công việc từ {startDate.Value:dd/MM/yyyy} đến {endDate.Value:dd/MM/yyyy}"
//                        : "Tổng hợp công việc tất cả");

//                // Thêm tiêu đề vào dòng đầu tiên
//                worksheet.Cells[1, 1].Value = reportTitle;
//                worksheet.Cells[1, 1, 1, 11].Merge = true;
//                worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
//                worksheet.Cells[1, 1].Style.Font.Bold = true;

//                // Tiêu đề cột
//                worksheet.Cells[2, 1].Value = "STT";
//                worksheet.Cells[2, 2].Value = "Nhân viên";
//                worksheet.Cells[2, 3].Value = "Hạng mục";
//                worksheet.Cells[2, 4].Value = "Công việc";
//                worksheet.Cells[2, 5].Value = "Deadline";
//                worksheet.Cells[2, 6].Value = "Ngày hoàn thành";
//                worksheet.Cells[2, 7].Value = "Trạng thái";
//                worksheet.Cells[2, 8].Value = "Đánh giá khối lượng";
//                worksheet.Cells[2, 9].Value = "Đánh giá tiến độ";
//                worksheet.Cells[2, 10].Value = "Đánh giá chất lượng";
//                worksheet.Cells[2, 11].Value = "Đánh giá tổng hợp";

//                // Điền dữ liệu công việc
//                for (int i = 0; i < jobList.Count; i++)
//                {
//                    var job = jobList[i];
//                    int rowIndex = i + 3; // Bắt đầu từ dòng 3

//                    worksheet.Cells[rowIndex, 1].Value = i + 1;
//                    worksheet.Cells[rowIndex, 2].Value = $"{job.Employee.Code} {job.Employee.FirstName} {job.Employee.LastName}";
//                    worksheet.Cells[rowIndex, 3].Value = job.Category?.Name ?? "N/A";
//                    worksheet.Cells[rowIndex, 4].Value = job.Name;
//                    worksheet.Cells[rowIndex, 5].Value = job.Deadline1?.ToString("dd/MM/yyyy") ?? "N/A";
//                    worksheet.Cells[rowIndex, 6].Value = job.CompletionDate?.ToString("dd/MM/yyyy") ?? "N/A";
//                    worksheet.Cells[rowIndex, 7].Value = job.Status switch
//                    {
//                        1 => "Hoàn thành",
//                        2 => "Chưa hoàn thành",
//                        3 => "Hoàn thành muộn",
//                        4 => "Đang xử lý",
//                        0 => "Chưa bắt đầu",
//                        _ => "Không xác định"
//                    };
//                    worksheet.Cells[rowIndex, 8].Value = job.VolumeAssessment ?? 0;
//                    worksheet.Cells[rowIndex, 9].Value = job.ProgressAssessment ?? 0;
//                    worksheet.Cells[rowIndex, 10].Value = job.QualityAssessment ?? 0;
//                    worksheet.Cells[rowIndex, 11].Value = job.SummaryOfReviews ?? 0;
//                }

//                // Định dạng cột
//                worksheet.Cells[2, 1, 2, 11].Style.Font.Bold = true;
//                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

//                // Xuất file Excel
//                var stream = new MemoryStream(package.GetAsByteArray());
//                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
//                    $"Jobs_{employee.Code}_{selectedMonth:yyyyMM}.xlsx");
//            }
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> UpdateJobAssessments(
//    List<JobUpdateModel> Jobs,
//    string month = null,
//    string searchText = null,
//    DateOnly? startDate = null,
//    DateOnly? endDate = null,
//    int? status = null,
//    int? categoryId = null,
//    string sortOrder = null,
//    bool showCompletedZeroReview = false,
//    bool dueToday = false)
//        {
//            // Kiểm tra Jobs có null hoặc rỗng không
//            if (Jobs == null || !Jobs.Any())
//            {
//                return BadRequest("Không có dữ liệu công việc để cập nhật.");
//            }

//            // Lấy thông tin employeeId từ job đầu tiên
//            var firstJob = await _context.Jobs.FindAsync(Jobs.First().Id);
//            if (firstJob == null)
//            {
//                return NotFound("Không tìm thấy công việc đầu tiên.");
//            }
//            long employeeId = firstJob.EmployeeId ?? 0;

//            // Nhóm các job theo tháng để cập nhật BaselineAssessment và Analysis
//            var jobsToUpdate = new List<Job>();
//            foreach (var jobModel in Jobs)
//            {
//                var job = await _context.Jobs.FindAsync(jobModel.Id);
//                if (job == null || !job.Time.HasValue)
//                {
//                    continue; // Bỏ qua nếu không tìm thấy job hoặc job không có Time
//                }

//                // Cập nhật các giá trị đánh giá
//                job.VolumeAssessment = jobModel.VolumeAssessment;
//                job.ProgressAssessment = jobModel.ProgressAssessment;
//                job.QualityAssessment = jobModel.QualityAssessment;

//                // Tính lại SummaryOfReviews nếu tất cả các giá trị đều có
//                if (job.VolumeAssessment.HasValue && job.ProgressAssessment.HasValue && job.QualityAssessment.HasValue)
//                {
//                    job.SummaryOfReviews = (float)((job.VolumeAssessment.Value * 0.6) +
//                                                  (job.ProgressAssessment.Value * 0.15) +
//                                                  (job.QualityAssessment.Value * 0.25));
//                }
//                else
//                {
//                    job.SummaryOfReviews = null;
//                }

//                jobsToUpdate.Add(job);
//                _context.Update(job);
//            }

//            // Lưu thay đổi vào cơ sở dữ liệu
//            try
//            {
//                await _context.SaveChangesAsync();
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, $"Lỗi khi lưu dữ liệu: {ex.Message}");
//            }

//            // Cập nhật hoặc tạo mới BaselineAssessment và Analysis theo từng tháng
//            var jobsByMonth = jobsToUpdate
//                .GroupBy(j => new { j.Time.Value.Year, j.Time.Value.Month })
//                .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.ToList());

//            foreach (var monthGroup in jobsByMonth)
//            {
//                var (year, monthNum) = monthGroup.Key;
//                await UpdateBaselineAssessment(employeeId, year, monthNum, monthGroup.Value);
//                await UpdateAnalysis(employeeId, year, monthNum, monthGroup.Value);
//            }

//            // Chuyển hướng về action Details mà không áp dụng bộ lọc
//            return RedirectToAction("Details", new { id = employeeId });
//        }
//        public class JobUpdateModel
//        {
//            public long Id { get; set; }
//            public float? VolumeAssessment { get; set; }
//            public float? ProgressAssessment { get; set; }
//            public float? QualityAssessment { get; set; }
//        }
//        private async Task UpdateBaselineAssessment(long employeeId, int year, int month, List<Job> jobs)
//        {
//            // Lấy danh sách công việc của nhân viên trong tháng cụ thể có đánh giá
//            var relevantJobs = jobs.Where(j => j.Status == 1 || j.Status == 3).ToList(); // Hoàn thành hoặc Hoàn thành muộn

//            if (!relevantJobs.Any())
//                return;

//            // Tính tổng các đánh giá
//            double sumVolume = relevantJobs.Sum(j => j.VolumeAssessment ?? 0);
//            double sumProgress = relevantJobs.Sum(j => j.ProgressAssessment ?? 0);
//            double sumQuality = relevantJobs.Sum(j => j.QualityAssessment ?? 0);
//            double sumSummary = relevantJobs.Sum(j => j.SummaryOfReviews ?? 0);

//            // Xác định trạng thái Evaluate (giả sử tổng Summary >= 45 là đạt)
//            bool evaluate = sumSummary >= 45;

//            // Tìm bản ghi BaselineAssessment của nhân viên trong tháng cụ thể
//            var baseline = await _context.Baselineassessments
//                .FirstOrDefaultAsync(b => b.EmployeeId == employeeId
//                                       && b.Time.HasValue
//                                       && b.Time.Value.Month == month
//                                       && b.Time.Value.Year == year);

//            if (baseline == null)
//            {
//                // Nếu chưa có bản ghi trong tháng, tạo mới
//                baseline = new Baselineassessment
//                {
//                    EmployeeId = employeeId,
//                    VolumeAssessment = sumVolume,
//                    ProgressAssessment = sumProgress,
//                    QualityAssessment = sumQuality,
//                    SummaryOfReviews = sumSummary,
//                    Time = new DateTime(year, month, 1),
//                    Evaluate = evaluate,
//                    CreateDate = DateTime.Now,
//                    UpdateDate = DateTime.Now
//                };
//                _context.Baselineassessments.Add(baseline);
//            }
//            else
//            {
//                // Nếu đã có bản ghi trong tháng, cập nhật dữ liệu
//                baseline.VolumeAssessment = sumVolume;
//                baseline.ProgressAssessment = sumProgress;
//                baseline.QualityAssessment = sumQuality;
//                baseline.SummaryOfReviews = sumSummary;
//                baseline.Evaluate = evaluate;
//                baseline.UpdateDate = DateTime.Now;
//            }

//            await _context.SaveChangesAsync();
//        }
//        private async Task UpdateAnalysis(long employeeId, int year, int month, List<Job> jobs)
//        {
//            // Tính các chỉ số phân tích từ danh sách công việc
//            int ontime = jobs.Count(j => j.Status == 1);
//            int late = jobs.Count(j => j.Status == 2);
//            int overdue = jobs.Count(j => j.Status == 3);
//            int processing = jobs.Count(j => j.Status == 4);
//            int total = ontime + late + overdue + processing;

//            // Tìm bản ghi Analysis của nhân viên trong tháng cụ thể
//            var analysis = await _context.Analyses
//                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
//                                       && a.Time.HasValue
//                                       && a.Time.Value.Month == month
//                                       && a.Time.Value.Year == year);

//            if (analysis == null)
//            {
//                // Nếu chưa có bản ghi trong tháng, tạo mới
//                analysis = new Analysis
//                {
//                    EmployeeId = employeeId,
//                    Total = total,
//                    Ontime = ontime,
//                    Late = late,
//                    Overdue = overdue,
//                    Processing = processing,
//                    Time = new DateTime(year, month, 1),
//                    CreateDate = DateTime.Now,
//                    UpdateDate = DateTime.Now
//                };
//                _context.Analyses.Add(analysis);
//            }
//            else
//            {
//                // Nếu đã có bản ghi trong tháng, cập nhật dữ liệu
//                analysis.Total = total;
//                analysis.Ontime = ontime;
//                analysis.Late = late;
//                analysis.Overdue = overdue;
//                analysis.Processing = processing;
//                analysis.UpdateDate = DateTime.Now;
//            }

//            await _context.SaveChangesAsync();
//        }
//    }
//}
//public class EmployeeDetailsViewModel
//{
//    public Employee Employee { get; set; }
//    public List<Job> Jobs { get; set; }
//    public List<Baselineassessment> BaselineAssessments { get; set; } // Đổi từ BaselineAssessment sang danh sách
//    public List<Analysis> Analyses { get; set; }
//    public DateTime SelectedMonth { get; set; }
//}