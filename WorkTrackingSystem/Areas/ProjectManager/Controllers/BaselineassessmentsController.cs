﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using WorkTrackingSystem.Models;
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

        public async Task<IActionResult> Index(
            string employeeName,
            string evaluate,
            string time,
            int page = 1)
        {
            var limit = 10;
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");

            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
            }

            var manager = await _context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.UserName == managerUsername);

            if (manager?.Employee?.DepartmentId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var managedEmployeeIds = await _context.Employees
                .Where(e => e.DepartmentId == manager.Employee.DepartmentId)
                .Select(e => e.Id)
                .ToListAsync();

            var assessments = _context.Baselineassessments
                .Include(b => b.Employee)
                .Where(b => b.EmployeeId.HasValue && managedEmployeeIds.Contains(b.EmployeeId.Value));

            // Set default time to current month if not provided
            if (string.IsNullOrEmpty(time))
            {
                time = DateTime.Now.ToString("yyyy-MM");
            }

            // Format display month for the view
            string displayMonth = "Toàn bộ";
            if (DateTime.TryParseExact(time, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime selectedTime))
            {
                displayMonth = $"Tháng {selectedTime:MM/yyyy}";
            }
            ViewBag.DisplayMonth = displayMonth;

            if (!string.IsNullOrEmpty(employeeName))
            {
                assessments = assessments.Where(b => b.Employee.FirstName.Contains(employeeName) || b.Employee.LastName.Contains(employeeName));
            }

            if (!string.IsNullOrEmpty(evaluate) && bool.TryParse(evaluate, out bool evaluateVal))
            {
                assessments = assessments.Where(b => b.Evaluate == evaluateVal);
            }

            if (!string.IsNullOrEmpty(time) && DateTime.TryParseExact(time, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime selectedTimeForFilter))
            {
                assessments = assessments.Where(b => b.Time.HasValue &&
                                                    b.Time.Value.Year == selectedTimeForFilter.Year &&
                                                    b.Time.Value.Month == selectedTimeForFilter.Month);
            }

            var assessmentsList = await assessments.ToListAsync();
            if (!assessmentsList.Any())
            {
                TempData["NoDataMessage"] = "Không có dữ liệu để hiển thị hoặc xuất Excel.";
            }

            ViewBag.EmployeeName = employeeName;
            ViewBag.Evaluate = evaluate;
            ViewBag.Time = time;

            return View(assessments.OrderBy(x => x.EmployeeId).ToPagedList(page, limit));
        }

        public async Task<IActionResult> ExportToExcel(string employeeName, string evaluate, string time)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");
            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
            }

            var manager = await _context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.UserName == managerUsername);

            if (manager?.Employee?.DepartmentId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var managedEmployeeIds = await _context.Employees
                .Where(e => e.DepartmentId == manager.Employee.DepartmentId)
                .Select(e => e.Id)
                .ToListAsync();

            var assessments = _context.Baselineassessments
                .Include(b => b.Employee)
                .Where(b => b.EmployeeId.HasValue && managedEmployeeIds.Contains(b.EmployeeId.Value));

            // Set default time to current month if not provided
            string selectedPeriod = "Toàn bộ";
            if (string.IsNullOrEmpty(time))
            {
                time = DateTime.Now.ToString("yyyy-MM");
            }

            if (!string.IsNullOrEmpty(employeeName))
            {
                assessments = assessments.Where(b => b.Employee.FirstName.Contains(employeeName) || b.Employee.LastName.Contains(employeeName));
            }

            if (!string.IsNullOrEmpty(evaluate) && bool.TryParse(evaluate, out bool evaluateVal))
            {
                assessments = assessments.Where(b => b.Evaluate == evaluateVal);
            }

            if (!string.IsNullOrEmpty(time) && DateTime.TryParseExact(time, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime selectedTime))
            {
                selectedPeriod = selectedTime.ToString("MM/yyyy");
                assessments = assessments.Where(b => b.Time.HasValue &&
                                                    b.Time.Value.Year == selectedTime.Year &&
                                                    b.Time.Value.Month == selectedTime.Month);
            }

            var assessmentList = await assessments.OrderBy(x => x.EmployeeId).ToListAsync();
            if (!assessmentList.Any())
            {
                TempData["NoDataMessage"] = "Không có dữ liệu để xuất Excel.";
                return RedirectToAction("Index");
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh sách đánh giá");

                worksheet.Cells[1, 1, 1, 7].Merge = true;
                worksheet.Cells[1, 1].Value = $"Bảng tổng hợp đánh giá {selectedPeriod}";
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
                    worksheet.Cells[row, 6].Value = Math.Round(item.SummaryOfReviews ?? 0, 2);
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

                    var dataRange = worksheet.Cells[3, 3, assessmentList.Count + 2, 3];
                    var labelRange = worksheet.Cells[3, 2, assessmentList.Count + 2, 2];

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

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DanhSachDanhGia_{selectedPeriod.Replace("/", "_")}.xlsx");
            }
        }

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

        public async Task<IActionResult> JobEvaluation(
    string searchText = "",
    string month = "",
    string day = "",
    string fromDate = "",
    string toDate = "",
    string quarter = "",
    string quarterYear = "",
    string year = "",
    string status = "",
    string categoryId = "",
    string sortOrder = "",
    bool showCompletedZeroReview = false,
    bool dueToday = false,
    long? jobId = null,
    int page = 1)
        {
            var limit = 8;
            var managerUsername = HttpContext.Session.GetString("ProjectManagerLogin");
            if (string.IsNullOrEmpty(managerUsername))
            {
                return RedirectToAction("Index", "Login");
            }

            var manager = await _context.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.UserName == managerUsername);
            if (manager?.Employee == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var managedEmployeeIds = await _context.Employees
                .Where(e => e.DepartmentId == manager.Employee.DepartmentId)
                .Select(e => e.Id)
                .ToListAsync();

            var employeesInManagedDepartments = await _context.Employees
                .Where(e => managedEmployeeIds.Contains(e.Id))
                .Select(e => new { Value = e.Id.ToString(), Text = $"{e.Code} {e.FirstName} {e.LastName}", Avatar = e.Avatar ?? "/images/default-avatar.png" })
                .ToListAsync();
            ViewBag.EmployeeList = employeesInManagedDepartments;

            ViewData["Categories"] = await _context.Categories.ToListAsync();

            var scoresQuery = _context.Scores
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.Employee)
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.Job)
                        .ThenInclude(j => j.Category)
                .Include(s => s.JobMapEmployee)
                    .ThenInclude(jme => jme.JobRepeat) // Include JobRepeat to access Deadline1
                .Where(s => s.JobMapEmployee != null && s.JobMapEmployee.Job != null)
                .Where(s => s.JobMapEmployee.EmployeeId.HasValue && managedEmployeeIds.Contains(s.JobMapEmployee.EmployeeId.Value));

            if (jobId.HasValue)
            {
                scoresQuery = scoresQuery.Where(s => s.JobMapEmployee.JobId == jobId.Value);
            }

            // Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.ToLower();
                scoresQuery = scoresQuery.Where(s =>
                    (s.JobMapEmployee.Employee != null && s.JobMapEmployee.Employee.Code.ToLower().Contains(searchText)) ||
                    (s.JobMapEmployee.Employee != null && s.JobMapEmployee.Employee.FirstName.ToLower().Contains(searchText)) ||
                    (s.JobMapEmployee.Employee != null && s.JobMapEmployee.Employee.LastName.ToLower().Contains(searchText)) ||
                    s.JobMapEmployee.Job.Name.ToLower().Contains(searchText));
            }

            // Lọc theo tháng
            if (!string.IsNullOrEmpty(month) && DateTime.TryParse(month + "-01", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime selectedMonth))
            {
                scoresQuery = scoresQuery.Where(s => s.CreateDate.HasValue && s.CreateDate.Value.Year == selectedMonth.Year && s.CreateDate.Value.Month == selectedMonth.Month);
            }

            // Lọc theo ngày cụ thể
            if (!string.IsNullOrEmpty(day) && DateTime.TryParse(day, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime selectedDay))
            {
                scoresQuery = scoresQuery.Where(s => s.CreateDate.HasValue && s.CreateDate.Value.Date == selectedDay.Date);
            }

            // Lọc theo từ ngày - đến ngày
            if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate))
            {
                scoresQuery = scoresQuery.Where(s => s.CreateDate.HasValue && s.CreateDate.Value.Date >= startDate.Date);
            }

            if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate))
            {
                scoresQuery = scoresQuery.Where(s => s.CreateDate.HasValue && s.CreateDate.Value.Date <= endDate.Date);
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

            // Lọc công việc hoàn thành nhưng chưa đánh giá
            if (showCompletedZeroReview)
            {
                scoresQuery = scoresQuery.Where(s => s.SummaryOfReviews == 0);
            }

            // Lọc công việc có deadline là hôm nay
            if (dueToday)
            {
                scoresQuery = scoresQuery.Where(s => s.CreateDate.HasValue && s.CreateDate.Value.Date == DateTime.Today);
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
                    scoresQuery = scoresQuery.OrderByDescending(s => s.CreateDate);
                    break;
            }

            // Materialize the query asynchronously
            var scoresList = await scoresQuery.ToListAsync();
            // Apply paging synchronously
            var scores = scoresList.ToPagedList(page, limit);

            // Lưu các giá trị tìm kiếm để hiển thị lại trong view
            ViewBag.SearchText = searchText;
            ViewBag.Month = month;
            ViewBag.Day = day;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.Year = year;
            ViewBag.Quarter = quarter;
            ViewBag.QuarterYear = quarterYear;
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

                score.SummaryOfReviews = Math.Round((score.VolumeAssessment ?? 0) * 0.6f +
                                         (score.ProgressAssessment ?? 0) * 0.15f +
                                         (score.QualityAssessment ?? 0) * 0.25f, 2);

                score.UpdateDate = DateTime.Now;
                score.UpdateBy = HttpContext.Session.GetString("ProjectManagerLogin");

                if (score.JobMapEmployee != null)
                {
                    // Cập nhật BaselineAssessment và Analysis
                    await UpdateBaselineAssessment(score.JobMapEmployee.EmployeeId);
                    await UpdateAnalysis(score.JobMapEmployee.EmployeeId);
                }

                // Cập nhật SCORE record
                _context.Update(score);

                // Cập nhật SCOREEMPLOYEE record
                var scoreEmployee = await _context.Scoreemployees
                    .FirstOrDefaultAsync(se => se.JobMapEmployeeId == score.JobMapEmployeeId && se.IsActive == true);

                if (scoreEmployee != null)
                {
                    scoreEmployee.IsActive = false;
                    _context.Update(scoreEmployee);
                }

                // Lưu tất cả thay đổi
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

            var jobs = await _context.Scores
                .Where(j => j.JobMapEmployee.EmployeeId == employeeId && j.CreateDate.HasValue)
                .ToListAsync();

            if (!jobs.Any())
                return;

            var groupedJobs = jobs
                .GroupBy(j => new { j.CreateDate.Value.Year, j.CreateDate.Value.Month });

            foreach (var group in groupedJobs)
            {
                var year = group.Key.Year;
                var month = group.Key.Month;

                double sumVolume = group.Sum(j => j.VolumeAssessment ?? 0);
                double sumProgress = group.Sum(j => j.ProgressAssessment ?? 0);
                double sumQuality = group.Sum(j => j.QualityAssessment ?? 0);
                double sumSummary = group.Sum(j => j.SummaryOfReviews ?? 0);
                bool evaluate = sumSummary >= 45;

                var baseline = await _context.Baselineassessments
                    .FirstOrDefaultAsync(b => b.EmployeeId == employeeId
                                           && b.Time.HasValue
                                           && b.Time.Value.Month == month
                                           && b.Time.Value.Year == year);

                if (baseline == null)
                {
                    baseline = new Baselineassessment
                    {
                        EmployeeId = employeeId,
                        VolumeAssessment = sumVolume,
                        ProgressAssessment = sumProgress,
                        QualityAssessment = sumQuality,
                        SummaryOfReviews = sumSummary,
                        Time = new DateTime(year, month, 1),
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

            var jobs = await _context.Scores
                .Where(j => j.JobMapEmployee.EmployeeId == employeeId && j.CreateDate.HasValue)
                .ToListAsync();

            if (!jobs.Any())
                return;

            var groupedJobs = jobs
                .GroupBy(j => new { j.CreateDate.Value.Year, j.CreateDate.Value.Month });

            foreach (var group in groupedJobs)
            {
                var year = group.Key.Year;
                var month = group.Key.Month;

                int ontime = group.Count(j => j.Status == 1);
                int late = group.Count(j => j.Status == 2);
                int overdue = group.Count(j => j.Status == 3);
                int processing = group.Count(j => j.Status == 4 || j.Status == 0 || j.Status == 5);
                int total = ontime + late + overdue + processing;

                var analysis = await _context.Analyses
                    .FirstOrDefaultAsync(a => a.EmployeeId == employeeId
                                           && a.Time.HasValue
                                           && a.Time.Value.Month == month
                                           && a.Time.Value.Year == year);

                if (analysis == null)
                {
                    analysis = new Analysis
                    {
                        EmployeeId = employeeId,
                        Total = total,
                        Ontime = ontime,
                        Late = late,
                        Overdue = overdue,
                        Processing = processing,
                        Time = new DateTime(year, month, 1),
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now
                    };
                    _context.Analyses.Add(analysis);
                }
                else
                {
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
                    .Include(u => u.Employee)
                    .FirstOrDefaultAsync(u => u.UserName == managerUsername);

                if (manager?.Employee == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin quản lý." });
                }

                var managedEmployeeIds = await _context.Employees
                    .Where(e => e.DepartmentId == manager.Employee.DepartmentId)
                    .Select(e => e.Id)
                    .ToListAsync();

                var assignedJobIds = await _context.Jobmapemployees
                    .Where(jme => jme.EmployeeId != null)
                    .Select(jme => jme.JobId)
                    .Distinct()
                    .ToListAsync();

                var unassignedJobs = await _context.Scores
                    .Include(s => s.JobMapEmployee)
                        .ThenInclude(jme => jme.Job)
                            .ThenInclude(j => j.Category)
                    .Where(s => s.JobMapEmployee.EmployeeId == null
                             && s.CreateBy == managerUsername
                             && !assignedJobIds.Contains(s.JobMapEmployee.JobId))
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

        // Phương thức mới để kiểm tra và cập nhật điểm định kỳ
        [HttpPost]
        public async Task<IActionResult> UpdateScoresForJobRepeats()
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                // Lấy tất cả Jobmapemployee có JobRepeat không null và deadline khớp với hôm nay
                var jobMapEmployeesWithRepeat = await _context.Jobmapemployees
                    .Include(jme => jme.Job)
                    .Include(jme => jme.JobRepeat)
                    .Include(jme => jme.Scores)
                    .Where(jme => jme.JobRepeatId != null && jme.Job.Deadline1 == today)
                    .ToListAsync();

                foreach (var jobMapEmployee in jobMapEmployeesWithRepeat)
                {
                    // Tìm Jobmapemployee ban đầu của Job (không có JobRepeat)
                    var originalJobMapEmployee = await _context.Jobmapemployees
                        .Include(jme => jme.Scores)
                        .FirstOrDefaultAsync(jme => jme.JobId == jobMapEmployee.JobId
                                                 && jme.JobRepeatId == null
                                                 && jme.EmployeeId == jobMapEmployee.EmployeeId);

                    if (originalJobMapEmployee != null)
                    {
                        // Lấy Score của Jobmapemployee có JobRepeat
                        var repeatScore = jobMapEmployee.Scores.FirstOrDefault(s => s.IsActive == true);

                        if (repeatScore != null)
                        {
                            // Tìm hoặc tạo Score cho Jobmapemployee ban đầu
                            var originalScore = originalJobMapEmployee.Scores.FirstOrDefault(s => s.IsActive == true);
                            if (originalScore == null)
                            {
                                originalScore = new Score
                                {
                                    JobMapEmployeeId = originalJobMapEmployee.Id,
                                    CreateDate = DateTime.Now,
                                    CreateBy = jobMapEmployee.CreateBy,
                                    Time = DateTime.Now,
                                    Status = 0,
                                    IsActive = true
                                };
                                _context.Scores.Add(originalScore);
                            }

                            // Cập nhật điểm từ repeatScore sang originalScore
                            originalScore.VolumeAssessment = repeatScore.VolumeAssessment;
                            originalScore.ProgressAssessment = repeatScore.ProgressAssessment;
                            originalScore.QualityAssessment = repeatScore.QualityAssessment;
                            originalScore.SummaryOfReviews = Math.Round(
                                (originalScore.VolumeAssessment ?? 0) * 0.6f +
                                (originalScore.ProgressAssessment ?? 0) * 0.15f +
                                (originalScore.QualityAssessment ?? 0) * 0.25f, 2);
                            originalScore.UpdateDate = DateTime.Now;
                            originalScore.UpdateBy = jobMapEmployee.UpdateBy;

                            // Cập nhật Scoreemployee cho originalJobMapEmployee
                            var originalScoreEmployee = await _context.Scoreemployees
                                .FirstOrDefaultAsync(se => se.JobMapEmployeeId == originalJobMapEmployee.Id && se.IsActive == true);
                            if (originalScoreEmployee != null)
                            {
                                originalScoreEmployee.IsActive = false;
                                _context.Update(originalScoreEmployee);
                            }

                            // Cập nhật BaselineAssessment và Analysis
                            await UpdateBaselineAssessment(originalJobMapEmployee.EmployeeId);
                            await UpdateAnalysis(originalJobMapEmployee.EmployeeId);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}