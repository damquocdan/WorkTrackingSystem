using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml;
using WorkTrackingSystem.Areas.ProjectManager.Models;
using WorkTrackingSystem.Models;
using X.PagedList.Extensions;
using OfficeOpenXml.Style;

namespace WorkTrackingSystem.Areas.ProjectManager.Controllers
{
    [Area("ProjectManager")]
    public class EmployeesController : Controller
    {
        private readonly WorkTrackingSystemContext _context;

        public EmployeesController(WorkTrackingSystemContext context)
        {
            _context = context;
        }

        // GET: ProjectManager/Employees
        public async Task<IActionResult> Index(string search, int? positionId, int page = 1)
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

            // Tải dữ liệu từ database trước khi gọi ToPagedList
            var managedDepartments = await _context.Departments
                .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3))
                .Select(d => d.Id)
                .ToListAsync(); // Chuyển thành danh sách thực tế

            var employeesQuery = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Where(e => e.DepartmentId.HasValue && managedDepartments.Contains(e.DepartmentId.Value)
                            && (e.IsDelete == false || e.IsDelete == null));

            if (positionId.HasValue && positionId > 0)
            {
                employeesQuery = employeesQuery.Where(e => e.PositionId == positionId.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                employeesQuery = employeesQuery.Where(e =>
                    e.Code.ToLower().Contains(search) ||
                    (e.FirstName != null && e.FirstName.ToLower().Contains(search)) ||
                    (e.LastName != null && e.LastName.ToLower().Contains(search)));
            }

            // Lấy danh sách nhân viên từ database trước khi phân trang
            var employees = await employeesQuery.ToListAsync();
            var pagedEmployees = employees.ToPagedList(page, limit); // Phân trang trên danh sách đã tải

            //       ViewBag.Positions = _context.Positions
            //.Select(p => new SelectListItem
            //{
            //    Value = p.Id.ToString(),
            //    Text = p.Name
            //})
            //.ToList();
            ViewBag.Search = search;
            ViewBag.positionId = positionId;
            ViewBag.Positions = new SelectList(_context.Positions, "Id", "Name");
            return View(pagedEmployees); // Trả về IPagedList<Employee>
        }
        public async Task<IActionResult> AnalysesEmployees(string search, int? positionId, string timeType, DateTime? fromDate, DateTime? toDate, string time, int? quarter, int? quarterYear, int? year, string sortOrder, int page = 1)
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

            // Lấy danh sách phòng ban mà manager quản lý
            var managedDepartments = await _context.Departments
                .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3))
                .Select(d => d.Id)
                .ToListAsync();

            // Xử lý tham số thời gian
            int? month = null;
            int? monthYear = null;

            if (timeType == "month" && !string.IsNullOrEmpty(time) && DateTime.TryParse(time + "-01", out var parsedTime))
            {
                month = parsedTime.Month;
                monthYear = parsedTime.Year;
                fromDate = new DateTime(parsedTime.Year, parsedTime.Month, 1);
                toDate = fromDate.Value.AddMonths(1).AddDays(-1);
            }
            else if (timeType == "quarter" && quarter.HasValue && quarterYear.HasValue)
            {
                fromDate = new DateTime(quarterYear.Value, (quarter.Value - 1) * 3 + 1, 1);
                toDate = fromDate.Value.AddMonths(3).AddDays(-1);
            }
            else if (timeType == "year" && year.HasValue)
            {
                fromDate = new DateTime(year.Value, 1, 1);
                toDate = new DateTime(year.Value, 12, 31);
            }
            else if (timeType != "dateRange")
            {
                // For "total", clear all time parameters
                fromDate = null;
                toDate = null;
                quarter = null;
                quarterYear = null;
                month = null;
                monthYear = null;
                year = null;
            }

            // Gọi stored procedure với các tham số
            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@DepartmentId", managedDepartments.FirstOrDefault()),
        new SqlParameter("@FromDate", (object)fromDate ?? DBNull.Value),
        new SqlParameter("@ToDate", (object)toDate ?? DBNull.Value),
        new SqlParameter("@Quarter", (object)quarter ?? DBNull.Value),
        new SqlParameter("@QuarterYear", (object)quarterYear ?? DBNull.Value),
        new SqlParameter("@Month", (object)month ?? DBNull.Value),
        new SqlParameter("@MonthYear", (object)monthYear ?? DBNull.Value),
        new SqlParameter("@Year", (object)year ?? DBNull.Value)
    };

            var employeeScores = await _context.Set<EmployeeScoreSummary>()
                .FromSqlRaw("EXEC sp_GetEmployeeScoreSummary @DepartmentId, @FromDate, @ToDate, @Quarter, @QuarterYear, @Month, @MonthYear, @Year", parameters.ToArray())
                .ToListAsync();

            // Lọc thêm nếu có search
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                employeeScores = employeeScores
                    .Where(e => e.EmployeeName.ToLower().Contains(search))
                    .ToList();
            }

            // Áp dụng sắp xếp
            switch (sortOrder)
            {
                case "total_asc":
                    employeeScores = employeeScores.OrderBy(e => e.TotalJobs).ToList();
                    break;
                case "total_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.TotalJobs).ToList();
                    break;
                case "ontime_asc":
                    employeeScores = employeeScores.OrderBy(e => e.OnTimeCount).ToList();
                    break;
                case "ontime_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.OnTimeCount).ToList();
                    break;
                case "late_asc":
                    employeeScores = employeeScores.OrderBy(e => e.LateCount).ToList();
                    break;
                case "late_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.LateCount).ToList();
                    break;
                case "overdue_asc":
                    employeeScores = employeeScores.OrderBy(e => e.OverdueCount).ToList();
                    break;
                case "overdue_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.OverdueCount).ToList();
                    break;
                case "processing_asc":
                    employeeScores = employeeScores.OrderBy(e => e.ProcessingCount).ToList();
                    break;
                case "processing_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.ProcessingCount).ToList();
                    break;
                default:
                    employeeScores = employeeScores.OrderBy(e => e.EmployeeName).ToList();
                    break;
            }

            // Phân trang
            var pagedEmployeeScores = employeeScores.ToPagedList(page, limit);

            // Chuẩn bị dữ liệu cho view
            ViewBag.Search = search;
            ViewBag.PositionId = positionId;
            ViewBag.TimeType = timeType ?? "total";
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Time = time;
            ViewBag.Quarter = quarter;
            ViewBag.QuarterYear = quarterYear;
            ViewBag.Year = year;
            ViewBag.SortOrder = sortOrder;
            ViewBag.DepartmentName = _context.Departments
                .Where(d => d.Id == managedDepartments.FirstOrDefault())
                .Select(d => d.Name)
                .FirstOrDefault() ?? "All Departments";
            ViewBag.Departments = new SelectList(_context.Departments.Where(d => managedDepartments.Contains(d.Id)), "Id", "Name");
            ViewBag.Positions = new SelectList(_context.Positions, "Id", "Name");

            return View(pagedEmployeeScores);
        }
        public async Task<IActionResult> EmployeeScoreSummary(string search, int? departmentId, string timeType, DateTime? fromDate, DateTime? toDate, string time, int? quarter, int? year, string sortOrder, string evaluate, int page = 1)
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

            // Get departments managed by the manager (assuming PositionId = 3 indicates a manager)
            var managedDepartments = await _context.Departments
                .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3))
                .Select(d => d.Id)
                .ToListAsync();

            // If departmentId is provided, ensure it's one of the managed departments
            if (departmentId.HasValue && !managedDepartments.Contains(departmentId.Value))
            {
                departmentId = null; // Ignore invalid departmentId
            }

            // Handle time parameters
            int? month = null;
            if (timeType == "month" && !string.IsNullOrEmpty(time) && DateTime.TryParse(time + "-01", out var parsedTime))
            {
                month = parsedTime.Month;
                year = parsedTime.Year;
                fromDate = new DateTime(parsedTime.Year, parsedTime.Month, 1);
                toDate = fromDate.Value.AddMonths(1).AddDays(-1);
            }
            else if (timeType == "quarter" && quarter.HasValue && year.HasValue)
            {
                fromDate = new DateTime(year.Value, (quarter.Value - 1) * 3 + 1, 1);
                toDate = fromDate.Value.AddMonths(3).AddDays(-1);
            }
            else if (timeType == "year" && year.HasValue)
            {
                fromDate = new DateTime(year.Value, 1, 1);
                toDate = new DateTime(year.Value, 12, 31);
            }
            else if (timeType != "dateRange")
            {
                // For "total", clear all time parameters
                fromDate = null;
                toDate = null;
                quarter = null;
                month = null;
                year = null;
            }

            // Query to aggregate scores by employee
            var employeeScoresQuery = from s in _context.Scores
                                      join jme in _context.Jobmapemployees on s.JobMapEmployeeId equals jme.Id
                                      join e in _context.Employees on jme.EmployeeId equals e.Id
                                      join d in _context.Departments on e.DepartmentId equals d.Id
                                      where (departmentId == null || d.Id == departmentId)
                                            && (fromDate == null || s.CreateDate >= fromDate)
                                            && (toDate == null || s.CreateDate <= toDate)
                                      group new { s, e } by new { e.Id, e.FirstName, e.LastName } into g
                                      select new EmployeeScoreSummary
                                      {
                                          EmployeeId = (int)g.Key.Id,
                                          EmployeeName = (g.Key.FirstName + " " + g.Key.LastName).Trim(),
                                          TotalVolume = (int)(g.Sum(x => x.s.VolumeAssessment ?? 0)),
                                          TotalProgress = (int)(g.Sum(x => x.s.ProgressAssessment ?? 0)),
                                          TotalQuality = (int)(g.Sum(x => x.s.QualityAssessment ?? 0)),
                                          SummaryScore = g.Sum(x =>
                                              (decimal)(x.s.VolumeAssessment ?? 0) * 0.6m +
                                              (decimal)(x.s.ProgressAssessment ?? 0) * 0.15m +
                                              (decimal)(x.s.QualityAssessment ?? 0) * 0.25m),
                                          EvaluationResult = g.Sum(x =>
                                              (decimal)(x.s.VolumeAssessment ?? 0) * 0.6m +
                                              (decimal)(x.s.ProgressAssessment ?? 0) * 0.15m +
                                              (decimal)(x.s.QualityAssessment ?? 0) * 0.25m) >= 4.5m ? "Đạt" : "Chưa đạt"
                                      };

            // Execute query and apply additional filters
            var employeeScores = await employeeScoresQuery.ToListAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                employeeScores = employeeScores
                    .Where(e => e.EmployeeName.ToLower().Contains(search))
                    .ToList();
            }

            // Apply evaluation filter
            if (!string.IsNullOrEmpty(evaluate))
            {
                employeeScores = employeeScores
                    .Where(e => e.EvaluationResult == evaluate)
                    .ToList();
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "name_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.EmployeeName).ToList();
                    break;
                case "volume_asc":
                    employeeScores = employeeScores.OrderBy(e => e.TotalVolume).ToList();
                    break;
                case "volume_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.TotalVolume).ToList();
                    break;
                case "progress_asc":
                    employeeScores = employeeScores.OrderBy(e => e.TotalProgress).ToList();
                    break;
                case "progress_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.TotalProgress).ToList();
                    break;
                case "quality_asc":
                    employeeScores = employeeScores.OrderBy(e => e.TotalQuality).ToList();
                    break;
                case "quality_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.TotalQuality).ToList();
                    break;
                case "summary_asc":
                    employeeScores = employeeScores.OrderBy(e => e.SummaryScore).ToList();
                    break;
                case "summary_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.SummaryScore).ToList();
                    break;
                default:
                    employeeScores = employeeScores.OrderBy(e => e.EmployeeName).ToList();
                    break;
            }

            // Pagination
            var pagedEmployeeScores = employeeScores.ToPagedList(page, limit);

            // Prepare view data
            ViewBag.Search = search;
            ViewBag.DepartmentId = departmentId;
            ViewBag.TimeType = timeType ?? "total";
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.Time = time;
            ViewBag.Quarter = quarter;
            ViewBag.Year = year;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Evaluate = evaluate;
            ViewBag.DepartmentName = departmentId.HasValue
                ? _context.Departments.Where(d => d.Id == departmentId).Select(d => d.Name).FirstOrDefault() ?? "All Departments"
                : "All Departments";
            ViewBag.Departments = new SelectList(_context.Departments.Where(d => managedDepartments.Contains(d.Id)), "Id", "Name");

            return View(pagedEmployeeScores);
        }
        public async Task<IActionResult> ExportToExcel(string search, string timeType, DateTime? fromDate, DateTime? toDate, string time, int? quarter, int? quarterYear, int? year, string sortOrder)
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
                .Where(d => d.Employees.Any(e => e.Id == manager.Id && e.PositionId == 3))
                .Select(d => d.Id)
                .ToListAsync();

            var departmentName = _context.Departments
                .Where(d => d.Id == managedDepartments.FirstOrDefault())
                .Select(d => d.Name)
                .FirstOrDefault() ?? "All Departments";

            // Process time parameters
            int? month = null;
            int? monthYear = null;
            string selectedPeriod = "tổng hợp tất cả thời gian";

            if (timeType == "month" && !string.IsNullOrEmpty(time) && DateTime.TryParse(time + "-01", out var parsedTime))
            {
                month = parsedTime.Month;
                monthYear = parsedTime.Year;
                fromDate = new DateTime(parsedTime.Year, parsedTime.Month, 1);
                toDate = fromDate.Value.AddMonths(1).AddDays(-1);
                selectedPeriod = $"tháng {parsedTime:MM/yyyy}";
            }
            else if (timeType == "quarter" && quarter.HasValue && quarterYear.HasValue)
            {
                fromDate = new DateTime(quarterYear.Value, (quarter.Value - 1) * 3 + 1, 1);
                toDate = fromDate.Value.AddMonths(3).AddDays(-1);
                selectedPeriod = $"quý {quarter.Value}/{quarterYear.Value}";
            }
            else if (timeType == "year" && year.HasValue)
            {
                fromDate = new DateTime(year.Value, 1, 1);
                toDate = new DateTime(year.Value, 12, 31);
                selectedPeriod = $"năm {year.Value}";
            }
            else if (timeType == "dateRange" && fromDate.HasValue && toDate.HasValue)
            {
                selectedPeriod = $"từ {fromDate.Value:dd/MM/yyyy} đến {toDate.Value:dd/MM/yyyy}";
            }
            else
            {
                fromDate = null;
                toDate = null;
                quarter = null;
                quarterYear = null;
                month = null;
                monthYear = null;
                year = null;
            }

            // Call stored procedure
            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@DepartmentId", managedDepartments.FirstOrDefault()),
        new SqlParameter("@FromDate", (object)fromDate ?? DBNull.Value),
        new SqlParameter("@ToDate", (object)toDate ?? DBNull.Value),
        new SqlParameter("@Quarter", (object)quarter ?? DBNull.Value),
        new SqlParameter("@QuarterYear", (object)quarterYear ?? DBNull.Value),
        new SqlParameter("@Month", (object)month ?? DBNull.Value),
        new SqlParameter("@MonthYear", (object)monthYear ?? DBNull.Value),
        new SqlParameter("@Year", (object)year ?? DBNull.Value)
    };

            var employeeScores = await _context.Set<EmployeeScoreSummary>()
                .FromSqlRaw("EXEC sp_GetEmployeeScoreSummary @DepartmentId, @FromDate, @ToDate, @Quarter, @QuarterYear, @Month, @MonthYear, @Year", parameters.ToArray())
                .ToListAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                employeeScores = employeeScores
                    .Where(e => e.EmployeeName.ToLower().Contains(search))
                    .ToList();
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "total_asc":
                    employeeScores = employeeScores.OrderBy(e => e.TotalJobs).ToList();
                    break;
                case "total_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.TotalJobs).ToList();
                    break;
                case "ontime_asc":
                    employeeScores = employeeScores.OrderBy(e => e.OnTimeCount).ToList();
                    break;
                case "ontime_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.OnTimeCount).ToList();
                    break;
                case "late_asc":
                    employeeScores = employeeScores.OrderBy(e => e.LateCount).ToList();
                    break;
                case "late_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.LateCount).ToList();
                    break;
                case "overdue_asc":
                    employeeScores = employeeScores.OrderBy(e => e.OverdueCount).ToList();
                    break;
                case "overdue_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.OverdueCount).ToList();
                    break;
                case "processing_asc":
                    employeeScores = employeeScores.OrderBy(e => e.ProcessingCount).ToList();
                    break;
                case "processing_desc":
                    employeeScores = employeeScores.OrderByDescending(e => e.ProcessingCount).ToList();
                    break;
                default:
                    employeeScores = employeeScores.OrderBy(e => e.EmployeeName).ToList();
                    break;
            }

            if (!employeeScores.Any())
            {
                TempData["NoDataMessage"] = $"Không có dữ liệu để xuất Excel cho {selectedPeriod}.";
                return RedirectToAction("AnalysesEmployees", new { search, timeType, fromDate, toDate, time, quarter, quarterYear, year, sortOrder });
            }

            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Analysis Summary");

                    // Header
                    worksheet.Cells[1, 1].Value = $"Bảng tổng hợp phân tích {selectedPeriod}";
                    worksheet.Cells[1, 1, 1, 7].Merge = true;
                    worksheet.Cells[1, 1].Style.Font.Size = 14;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    // Column headers
                    worksheet.Cells[2, 1].Value = "STT";
                    worksheet.Cells[2, 2].Value = "Nhân sự";
                    worksheet.Cells[2, 3].Value = "Tổng số";
                    worksheet.Cells[2, 4].Value = "Đúng hạn";
                    worksheet.Cells[2, 5].Value = "Trễ hạn";
                    worksheet.Cells[2, 6].Value = "Quá hạn";
                    worksheet.Cells[2, 7].Value = "Đang xử lý";
                    worksheet.Cells[2, 1, 2, 7].Style.Font.Bold = true;
                    worksheet.Cells[2, 1, 2, 7].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[2, 1, 2, 7].Style.Fill.BackgroundColor.SetColor(Color.Green);
                    worksheet.Cells[2, 1, 2, 7].Style.Font.Color.SetColor(Color.White);
                    worksheet.Cells[2, 1, 2, 7].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    // Department summary
                    worksheet.Cells[3, 1].Value = "Tổng";
                    worksheet.Cells[3, 2].Value = departmentName;
                    worksheet.Cells[3, 3].Value = employeeScores.Sum(e => e.TotalJobs);
                    worksheet.Cells[3, 4].Value = employeeScores.Sum(e => e.OnTimeCount);
                    worksheet.Cells[3, 5].Value = employeeScores.Sum(e => e.LateCount);
                    worksheet.Cells[3, 6].Value = employeeScores.Sum(e => e.OverdueCount);
                    worksheet.Cells[3, 7].Value = employeeScores.Sum(e => e.ProcessingCount);
                    worksheet.Cells[3, 1, 3, 7].Style.Font.Bold = true;

                    // Employee data
                    for (int i = 0; i < employeeScores.Count; i++)
                    {
                        var score = employeeScores[i];
                        worksheet.Cells[i + 4, 1].Value = i + 1;
                        worksheet.Cells[i + 4, 2].Value = score.EmployeeName;
                        worksheet.Cells[i + 4, 3].Value = score.TotalJobs;
                        worksheet.Cells[i + 4, 4].Value = score.OnTimeCount;
                        worksheet.Cells[i + 4, 5].Value = score.LateCount;
                        worksheet.Cells[i + 4, 6].Value = score.OverdueCount;
                        worksheet.Cells[i + 4, 7].Value = score.ProcessingCount;
                    }

                    // Center STT column
                    worksheet.Cells[3, 1, employeeScores.Count + 3, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    // Auto-fit columns
                    worksheet.Cells.AutoFitColumns();

                    // Add charts
                    if (employeeScores.Any())
                    {
                        // Column chart
                        var columnChart = worksheet.Drawings.AddChart("ColumnChart", eChartType.ColumnClustered) as ExcelBarChart;
                        columnChart.SetPosition(employeeScores.Count + 5, 0, 1, 0);
                        columnChart.SetSize(800, 400);

                        int nameColumnIndex = 2;
                        int totalColumnIndex = 3;
                        int ontimeColumnIndex = 4;
                        int lateColumnIndex = 5;
                        int overdueColumnIndex = 6;

                        var labelRange = worksheet.Cells[4, nameColumnIndex, employeeScores.Count + 3, nameColumnIndex];
                        var totalRange = worksheet.Cells[4, totalColumnIndex, employeeScores.Count + 3, totalColumnIndex];
                        var ontimeRange = worksheet.Cells[4, ontimeColumnIndex, employeeScores.Count + 3, ontimeColumnIndex];
                        var lateRange = worksheet.Cells[4, lateColumnIndex, employeeScores.Count + 3, lateColumnIndex];
                        var overdueRange = worksheet.Cells[4, overdueColumnIndex, employeeScores.Count + 3, overdueColumnIndex];

                        var seriesTotal = columnChart.Series.Add(totalRange, labelRange);
                        seriesTotal.Header = "Tổng số";
                        var seriesOntime = columnChart.Series.Add(ontimeRange, labelRange);
                        seriesOntime.Header = "Đúng hạn";
                        var seriesLate = columnChart.Series.Add(lateRange, labelRange);
                        seriesLate.Header = "Trễ hạn";
                        var seriesOverdue = columnChart.Series.Add(overdueRange, labelRange);
                        seriesOverdue.Header = "Quá hạn";

                        seriesTotal.Fill.Color = Color.Blue;
                        seriesOntime.Fill.Color = Color.Green;
                        seriesLate.Fill.Color = Color.Red;
                        seriesOverdue.Fill.Color = Color.Yellow;

                        columnChart.Title.Text = $"Tổng hợp {selectedPeriod}";
                        columnChart.Legend.Position = eLegendPosition.Bottom;
                        columnChart.DataLabel.ShowValue = true;
                        columnChart.DataLabel.Position = eLabelPosition.OutEnd;

                        // Pie chart
                        var pieChart = worksheet.Drawings.AddChart("PieChart", eChartType.Pie3D) as ExcelPieChart;
                        pieChart.SetPosition(1, 0, 9, 0);
                        pieChart.SetSize(500, 350);

                        var pieDataRange = worksheet.Cells[4, totalColumnIndex, employeeScores.Count + 3, totalColumnIndex];
                        var pieLabelRange = worksheet.Cells[4, nameColumnIndex, employeeScores.Count + 3, nameColumnIndex];

                        var pieSeries = pieChart.Series.Add(pieDataRange, pieLabelRange) as ExcelPieChartSerie;
                        pieChart.Title.Text = "Theo số lượng công việc";
                        pieChart.Legend.Position = eLegendPosition.Left;

                        if (pieSeries != null)
                        {
                            pieSeries.DataLabel.ShowPercent = true;
                            pieSeries.DataLabel.Position = eLabelPosition.Center;
                        }
                    }

                    var stream = new MemoryStream(package.GetAsByteArray());
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Analysis_{selectedPeriod.Replace("/", "_").Replace("từ ", "").Replace("đến ", "to_")}.xlsx");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tạo Excel: {ex.Message}");
                TempData["NoDataMessage"] = "Có lỗi xảy ra khi xuất Excel.";
                return RedirectToAction("AnalysesEmployees", new { search, timeType, fromDate, toDate, time, quarter, quarterYear, year, sortOrder });
            }
        }
        public async Task<IActionResult> ExportToExcelScore(string search, string evaluate, string timeType, string time, int? quarter, int? year, string fromDate, string toDate)
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

            if (manager?.Employee == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Get departments managed by the manager (assuming PositionId = 3 indicates a manager)
            var managedDepartments = await _context.Departments
                .Where(d => d.Employees.Any(e => e.Id == manager.Employee.Id && e.PositionId == 3))
                .Select(d => d.Id)
                .ToListAsync();

            // Handle time parameters
            DateTime? parsedFromDate = null;
            DateTime? parsedToDate = null;
            string selectedPeriod = "Toàn bộ";

            if (timeType == "month" && !string.IsNullOrEmpty(time) && DateTime.TryParse(time + "-01", out var parsedTime))
            {
                parsedFromDate = new DateTime(parsedTime.Year, parsedTime.Month, 1);
                parsedToDate = parsedFromDate.Value.AddMonths(1).AddDays(-1);
                selectedPeriod = parsedTime.ToString("MM/yyyy");
            }
            else if (timeType == "quarter" && quarter.HasValue && year.HasValue)
            {
                parsedFromDate = new DateTime(year.Value, (quarter.Value - 1) * 3 + 1, 1);
                parsedToDate = parsedFromDate.Value.AddMonths(3).AddDays(-1);
                selectedPeriod = $"Quý {quarter}/{year}";
            }
            else if (timeType == "year" && year.HasValue)
            {
                parsedFromDate = new DateTime(year.Value, 1, 1);
                parsedToDate = new DateTime(year.Value, 12, 31);
                selectedPeriod = $"Năm {year}";
            }
            else if (timeType == "dateRange" && !string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate) &&
                     DateTime.TryParse(fromDate, out var parsedFrom) && DateTime.TryParse(toDate, out var parsedTo))
            {
                parsedFromDate = parsedFrom;
                parsedToDate = parsedTo;
                selectedPeriod = $"Từ {parsedFrom.ToString("dd/MM/yyyy")} Đến {parsedTo.ToString("dd/MM/yyyy")}";
            }
            else
            {
                // Default to "Toàn bộ" if no valid time filter is provided
                timeType = "total";
            }

            // Query to aggregate scores by employee
            var employeeScoresQuery = from s in _context.Scores
                                      join jme in _context.Jobmapemployees on s.JobMapEmployeeId equals jme.Id
                                      join e in _context.Employees on jme.EmployeeId equals e.Id
                                      join d in _context.Departments on e.DepartmentId equals d.Id
                                      where managedDepartments.Contains(d.Id) &&
                                            (parsedFromDate == null || s.CreateDate >= parsedFromDate) &&
                                            (parsedToDate == null || s.CreateDate <= parsedToDate)
                                      group new { s, e } by new { e.Id, e.FirstName, e.LastName } into g
                                      select new EmployeeScoreSummary
                                      {
                                          EmployeeId = (int)g.Key.Id,
                                          EmployeeName = (g.Key.FirstName + " " + g.Key.LastName).Trim(),
                                          TotalVolume = (int)(g.Sum(x => x.s.VolumeAssessment ?? 0)),
                                          TotalProgress = (int)(g.Sum(x => x.s.ProgressAssessment ?? 0)),
                                          TotalQuality = (int)(g.Sum(x => x.s.QualityAssessment ?? 0)),
                                          SummaryScore = g.Sum(x =>
                                              (decimal)(x.s.VolumeAssessment ?? 0) * 0.6m +
                                              (decimal)(x.s.ProgressAssessment ?? 0) * 0.15m +
                                              (decimal)(x.s.QualityAssessment ?? 0) * 0.25m),
                                          EvaluationResult = g.Sum(x =>
                                              (decimal)(x.s.VolumeAssessment ?? 0) * 0.6m +
                                              (decimal)(x.s.ProgressAssessment ?? 0) * 0.15m +
                                              (decimal)(x.s.QualityAssessment ?? 0) * 0.25m) >= 4.5m ? "Đạt" : "Chưa đạt"
                                      };

            // Apply search and evaluation filters
            var employeeScores = await employeeScoresQuery.ToListAsync();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                employeeScores = employeeScores
                    .Where(e => e.EmployeeName.ToLower().Contains(search))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(evaluate))
            {
                employeeScores = employeeScores
                    .Where(e => e.EvaluationResult == evaluate)
                    .ToList();
            }

            // Order by EmployeeId for consistency
            employeeScores = employeeScores.OrderBy(x => x.EmployeeId).ToList();

            if (!employeeScores.Any())
            {
                TempData["NoDataMessage"] = "Không có dữ liệu để xuất Excel.";
                return RedirectToAction("EmployeeScoreSummary");
            }

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh sách đánh giá");

                // Title
                worksheet.Cells[1, 1, 1, 7].Merge = true;
                worksheet.Cells[1, 1].Value = $"Bảng tổng hợp đánh giá {selectedPeriod}";
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 14;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Headers
                worksheet.Cells[2, 1].Value = "STT";
                worksheet.Cells[2, 2].Value = "Nhân viên";
                worksheet.Cells[2, 3].Value = "Tổng đánh giá khối lượng";
                worksheet.Cells[2, 4].Value = "Tổng đánh giá tiến độ";
                worksheet.Cells[2, 5].Value = "Tổng đánh giá chất lượng";
                worksheet.Cells[2, 6].Value = "Tổng đánh giá tổng hợp";
                worksheet.Cells[2, 7].Value = "Đánh giá";

                using (var range = worksheet.Cells[2, 1, 2, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Data
                int row = 3;
                int index = 0;
                foreach (var item in employeeScores)
                {
                    index++;
                    worksheet.Cells[row, 1].Value = index;
                    worksheet.Cells[row, 2].Value = item.EmployeeName;
                    worksheet.Cells[row, 3].Value = item.TotalVolume;
                    worksheet.Cells[row, 4].Value = item.TotalProgress;
                    worksheet.Cells[row, 5].Value = item.TotalQuality;
                    worksheet.Cells[row, 6].Value = Math.Round(item.SummaryScore, 2);
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "0.00";
                    worksheet.Cells[row, 7].Value = item.EvaluationResult;
                    worksheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 3, row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    row++;
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Add pie chart
                if (employeeScores.Any())
                {
                    var chart = worksheet.Drawings.AddChart("PieChart", eChartType.Pie3D) as ExcelPieChart;
                    chart.SetPosition(1, 0, 9, 0);
                    chart.SetSize(500, 350);

                    var dataRange = worksheet.Cells[3, 3, employeeScores.Count + 2, 3];
                    var labelRange = worksheet.Cells[3, 2, employeeScores.Count + 2, 2];

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

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DanhSachDanhGia_{selectedPeriod.Replace("/", "_").Replace(" ", "_")}.xlsx");
            }
        }
        public async Task<IActionResult> Details(int? page, long? id)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            // Lấy danh sách công việc liên quan đến nhân viên và lưu vào ViewBag
            var jobMaps = await _context.Jobmapemployees
                         .Where(jm => jm.EmployeeId == id && jm.IsDelete != true)
                         .Include(jm => jm.Job)
                             .ThenInclude(j => j.Category)
                         .Include(jm => jm.Scores)
                         .ToListAsync(); // Tải danh sách trước

            var pagedJobMaps = jobMaps.ToPagedList(pageNumber, pageSize); // Sau đó mới phân trang

            ViewBag.JobMaps = pagedJobMaps;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_Details", employee);
            }

            return View(employee);
        }

        // GET: ProjectManager/Employees/Create
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id");
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Id");
            return View();
        }

        // POST: ProjectManager/Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DepartmentId,PositionId,Code,FirstName,LastName,Gender,Birthday,Phone,Email,HireDate,Address,Avatar,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id", employee.DepartmentId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Id", employee.PositionId);
            return View(employee);
        }

        // GET: ProjectManager/Employees/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id", employee.DepartmentId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Id", employee.PositionId);
            return View(employee);
        }

        // POST: ProjectManager/Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,DepartmentId,PositionId,Code,FirstName,LastName,Gender,Birthday,Phone,Email,HireDate,Address,Avatar,IsDelete,IsActive,CreateDate,UpdateDate,CreateBy,UpdateBy")] Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
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
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id", employee.DepartmentId);
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Id", employee.PositionId);
            return View(employee);
        }

        // GET: ProjectManager/Employees/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: ProjectManager/Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(long id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
