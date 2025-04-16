using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using WorkTrackingSystem.Models;
using OfficeOpenXml.Drawing.Chart;
using X.PagedList.Extensions;

namespace WorkTrackingSystem.Areas.ProjectManager.Controllers
{
    [Area("ProjectManager")]
    public class BaselineassessmentsController : BaseController
    {
        private readonly WorkTrackingSystemContext _context;

        public BaselineassessmentsController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: ProjectManager/Baselineassessments
        public async Task<IActionResult> Index(string employeeName, string evaluate, string time, int page = 1)
        {
            var limit = 10;
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

            var managedDepartments = await _context.Employees
                .Where(e => e.DepartmentId == manager.DepartmentId)
                .Select(e => e.Id)
                .ToListAsync();

            var assessments = _context.Baselineassessments
                .Include(b => b.Employee)
                .Where(b => b.EmployeeId.HasValue && managedDepartments.Contains(b.EmployeeId.Value));

            // Lọc theo tên nhân viên
            if (!string.IsNullOrEmpty(employeeName))
            {
                assessments = assessments.Where(b => b.Employee.FirstName.Contains(employeeName) || b.Employee.LastName.Contains(employeeName));
            }

            // Lọc theo trạng thái đánh giá
            if (!string.IsNullOrEmpty(evaluate) && bool.TryParse(evaluate, out bool evaluateVal))
            {
                assessments = assessments.Where(b => b.Evaluate == evaluateVal);
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

            var assessmentsList = await assessments.ToListAsync();
            if (!assessmentsList.Any())
            {
                TempData["NoDataMessage"] = "Không có dữ liệu để hiển thị hoặc xuất Excel.";
            }

            ViewBag.EmployeeName = employeeName;
            ViewBag.Evaluate = evaluate;
            ViewBag.Time = time;

            return View(assessments.OrderByDescending(x => x.Time ?? DateTime.MinValue).ToPagedList(page, limit));
        }

        public async Task<IActionResult> ExportToExcel(string employeeCode, string employeeName, bool? evaluate, string time)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

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
                .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3))
                .Select(d => d.Id)
                .ToListAsync();

            var assessments = _context.Baselineassessments
                .Include(b => b.Employee)
                .Where(b => b.Employee != null &&
                            b.Employee.DepartmentId.HasValue &&
                            managedDepartments.Contains(b.Employee.DepartmentId.Value));

            if (!string.IsNullOrEmpty(employeeCode))
            {
                assessments = assessments.Where(b => b.Employee.Code.Contains(employeeCode));
            }

            if (!string.IsNullOrEmpty(employeeName))
            {
                assessments = assessments.Where(b => b.Employee.FirstName.Contains(employeeName) || b.Employee.LastName.Contains(employeeName));
            }

            if (evaluate.HasValue)
            {
                assessments = assessments.Where(b => b.Evaluate == evaluate.Value);
            }
            string selectedMonth = "Toàn bộ";
            if (!string.IsNullOrEmpty(time))
            {
                if (DateTime.TryParseExact(time, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime selectedTime))
                {
                    selectedMonth = selectedTime.ToString("MM/yyyy");
                    assessments = assessments.Where(b => b.Time.HasValue &&
                                                         b.Time.Value.Year == selectedTime.Year &&
                                                         b.Time.Value.Month == selectedTime.Month);
                }
            }

            var assessmentList = await assessments.OrderByDescending(x => x.Time).ToListAsync();
            if (!assessmentList.Any())
            {
                TempData["NoDataMessage"] = "Không có dữ liệu để xuất Excel.";
                return RedirectToAction("Index");
            }
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh sách đánh giá");

                worksheet.Cells[1, 1, 1, 7].Merge = true;
                worksheet.Cells[1, 1].Value = $"Bảng tổng hợp đánh giá tháng {selectedMonth}";
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 14;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[2, 1].Value = "Mã nhân viên";
                worksheet.Cells[2, 2].Value = "Tên nhân viên";
                worksheet.Cells[2, 3].Value = "Tổng đánh giá khối lượng";
                worksheet.Cells[2, 4].Value = "Tổng đánh giá tiến độ";
                worksheet.Cells[2, 5].Value = "Tổng đánh giá chất lượng";
                worksheet.Cells[2, 6].Value = "Tổng đánh giá tổng hợp";
                worksheet.Cells[2, 7].Value = "Đánh giá theo baseline";

                using (var range = worksheet.Cells[2, 1, 2, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                int row = 3;
                foreach (var item in assessmentList)
                {
                    worksheet.Cells[row, 1].Value = item.Employee.Code;
                    worksheet.Cells[row, 2].Value = $"{item.Employee.FirstName} {item.Employee.LastName}";
                    worksheet.Cells[row, 3].Value = item.VolumeAssessment;
                    worksheet.Cells[row, 4].Value = item.ProgressAssessment;
                    worksheet.Cells[row, 5].Value = item.QualityAssessment;
                    worksheet.Cells[row, 6].Value = Math.Round(Convert.ToDouble(item.SummaryOfReviews), 2);
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "0.00";

                    worksheet.Cells[row, 7].Value = item.Evaluate.GetValueOrDefault() ? "Đạt baseline" : "Chưa đạt baseline";

                    worksheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 3, row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                    row++;
                }

                worksheet.Cells.AutoFitColumns();

                if (assessmentList.Any())
                {
                    var chart = worksheet.Drawings.AddChart("PieChart", eChartType.Pie3D) as ExcelPieChart;
                    chart.SetPosition(1, 0, 9, 0);
                    chart.SetSize(500, 350);

                    int totalColumnIndex = 3;
                    int nameColumnIndex = 2;

                    var dataRange = worksheet.Cells[3, totalColumnIndex, assessmentList.Count + 2, totalColumnIndex];
                    var labelRange = worksheet.Cells[3, nameColumnIndex, assessmentList.Count + 2, nameColumnIndex];

                    var series = chart.Series.Add(dataRange, labelRange) as ExcelPieChartSerie;
                    chart.Title.Text = "Đánh giá khối lượng";
                    chart.Legend.Position = eLegendPosition.Left;

                    if (series != null)
                    {
                        series.DataLabel.ShowPercent = true;
                        series.DataLabel.Position = eLabelPosition.Center;
                    }
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachDanhGia.xlsx");
            }
        }


        // GET: ProjectManager/Baselineassessments/Details/5
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


        //đánh giá từng công việc cảu nhân viên
        public async Task<IActionResult> JobEaluation(
 string searchText = "",
 string month = "",
 string day = "",
 string status = "",
 string categoryId = "",
 string sortOrder = "",
 bool showCompletedZeroReview = false,
 bool dueToday = false,
 long? jobId = null, int page = 1)
        {
            var limit = 8;
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
                scoresQuery = scoresQuery.Where(s => s.CreateDate.HasValue && s.CreateDate.Value.Year == selectedMonth.Year && s.CreateDate.Value.Month == selectedMonth.Month);
            }
            if (!string.IsNullOrEmpty(day) && DateTime.TryParse(day, out DateTime selectedDay))
            {
                scoresQuery = scoresQuery.Where(s => s.CreateDate.HasValue && s.CreateDate.Value.Date == selectedDay.Date);
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
                scoresQuery = scoresQuery.Where(s => s.SummaryOfReviews == 0);
            }
            if (dueToday)
            {
                scoresQuery = scoresQuery.Where(s => s.Time==s.CreateDate);
            }
            var scores = scoresQuery.ToPagedList(page, limit);
            ViewBag.SearchText = searchText;
            ViewBag.Month = month;
            ViewBag.Day = day;
            ViewBag.Status = status?.ToString();
            ViewBag.CategoryId = categoryId?.ToString();
            ViewBag.SortOrder = sortOrder;
            ViewBag.ShowCompletedZeroReview = showCompletedZeroReview;
            ViewBag.DueToday = dueToday;
            return View(scores);
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
                CreateDate = job.Deadline1?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
                CreateBy = HttpContext.Session.GetString("ProjectManagerLogin")
            };
            _context.Scores.Add(score);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
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
                score.SummaryOfReviews = (score.VolumeAssessment ?? 0) * 0.6f +
                                         (score.ProgressAssessment ?? 0) * 0.15f +
                                         (score.QualityAssessment ?? 0) * 0.25f;

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
                int processing = group.Count(j => j.Status == 4 || j.Status == 0 || j.Status == 5);
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
    }
}